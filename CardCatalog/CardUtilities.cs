using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CardCatalog.Models;
using HtmlAgilityPack;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Raven.Client;
using Intervals;
using Newtonsoft.Json;
using System.Diagnostics;

namespace CardCatalog
{
    public static class CardUtilities
    {
        private static readonly object fetchStateMutex = new object();

        public static Card ReadOrScrapeCard(this IDocumentSession session, int id)
        {
            bool ignore;
            return ReadOrScrapeCard(session, id, out ignore);
        }

        public static Card ReadOrScrapeCard(this IDocumentSession session, int id, out bool previouslyScraped)
        {
            previouslyScraped = true;

            // First, try to fetch the card from the database.
            var card = (from c in session.Query<Card>()
                        where c.NormalizedFaces.Any(f => f.Id == id)
                        select c).SingleOrDefault();

            // If the card is missing, fetch and save it.
            if (card == null)
            {
                if (UsingFetchState(fetchState => fetchState.IsCardMissing(id)))
                {
                    return null;
                }

                previouslyScraped = false;
                card = CardUtilities.ScrapeCard(id);

                UsingFetchState(fetchState =>
                {
                    if (card == null)
                    {
                        fetchState.AddMissingCard(id);
                    }
                });

                if (card != null)
                {
                    session.Store(card);
                    session.SaveChanges();
                }
            }

            return card;
        }

        public static Card ScrapeCard(int id)
        {
            var normalizedDoc = new HtmlDocument();
            var printedDoc = new HtmlDocument();
            using (var client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                var normalizedHtml = client.DownloadString("http://gatherer.wizards.com/Pages/Card/Details.aspx?multiverseid=" + id);
                var printedHtml = client.DownloadString("http://gatherer.wizards.com/Pages/Card/Details.aspx?printed=tru&multiverseid=" + id);
                normalizedDoc.LoadHtml(normalizedHtml);
                printedDoc.LoadHtml(printedHtml);
            }

            var normalizedFaceNodes = normalizedDoc.DocumentNode.SelectNodes("//table[contains(concat(' ',normalize-space(@class),' '),' cardDetails ')]");
            var printedFaceNodes = printedDoc.DocumentNode.SelectNodes("//table[contains(concat(' ',normalize-space(@class),' '),' cardDetails ')]");
            if (normalizedFaceNodes == null)
            {
                return null;
            }

            var expansionAnchor = normalizedDoc.DocumentNode.SelectSingleNode("//div[@class='label' and text()[contains(.,'Expansion:')]]/following-sibling::*[1]/div/a[2]");
            var expansion = expansionAnchor == null ? null : HtmlEntity.DeEntitize(expansionAnchor.InnerText.Trim());

            var raritySpan = normalizedDoc.DocumentNode.SelectSingleNode("//div[@class='label' and text()[contains(.,'Rarity:')]]/following-sibling::*[1]/span");
            var rarity = raritySpan == null ? null : HtmlEntity.DeEntitize(raritySpan.Attributes["class"].Value);

            var normalizedFaces = (from HtmlNode faceNode in normalizedFaceNodes
                                   select ReadFace(faceNode)).ToList();
            var printedFaces = (from HtmlNode faceNode in printedFaceNodes
                                select ReadFace(faceNode)).ToList();

            return new Card
            {
                Id = normalizedFaces.First().Id,
                Expansion = expansion,
                Rarity = rarity,
                NormalizedFaces = normalizedFaces,
                PrintedFaces = printedFaces,
            };
        }

        private static CardFace ReadFace(HtmlNode node)
        {
            if (node == null)
            {
                return null;
            }

            Func<string, string> mapMana = cost =>
                cost == "Black" ? "{B}" :
                cost == "Black or Green" ? "{BG}" :
                cost == "Black or Red" ? "{BR}" :
                cost == "Blue" ? "{U}" :
                cost == "Blue or Black" ? "{UB}" :
                cost == "Blue or Red" ? "{UR}" :
                cost == "Green" ? "{G}" :
                cost == "Green or Blue" ? "{GU}" :
                cost == "Green or White" ? "{GW}" :
                cost == "Phyrexian Black" ? "{BP}" :
                cost == "Phyrexian Blue" ? "{UP}" :
                cost == "Phyrexian Green" ? "{GP}" :
                cost == "Phyrexian Red" ? "{RP}" :
                cost == "Phyrexian White" ? "{WP}" :
                cost == "Red" ? "{R}" :
                cost == "Red or Green" ? "{RG}" :
                cost == "Red or White" ? "{RW}" :
                cost == "Snow" ? "{S}" :
                cost == "Tap" ? "{T}" :
                cost == "Untap" ? "{Q}" :
                cost == "Variable Colorless" ? "{X}" :
                cost == "White" ? "{W}" :
                cost == "White or Black" ? "{WB}" :
                cost == "White or Blue" ? "{WU}" :
                "{" + cost + "}";

            var cardImage = node.SelectSingleNode("./descendant::img[1]");
            var id = int.Parse(Regex.Match(cardImage.Attributes["src"].Value, @"multiverseid=(?<id>\d+)").Groups["id"].Value);

            var nameDiv = node.SelectSingleNode("./descendant::div[@class='label' and text()[contains(.,'Card Name:')]]/following-sibling::*[1]");
            var name = HtmlEntity.DeEntitize(nameDiv.InnerText).Trim();

            var manaImages = node.SelectNodes("./descendant::div[@class='label' and text()[contains(.,'Mana Cost:')]]/following-sibling::*[1]/img");
            var mana = manaImages == null
                ? null
                : string.Join("", from HtmlNode image in manaImages
                                  let cost = HtmlEntity.DeEntitize(image.Attributes["alt"].Value)
                                  select mapMana(cost));

            var convertedManaDiv = node.SelectSingleNode("./descendant::div[@class='label' and text()[contains(.,'Converted Mana Cost:')]]/following-sibling::*[1]");
            var convertedMana = convertedManaDiv == null ? (int?)null : int.Parse(convertedManaDiv.InnerText.Trim());

            var typesDiv = node.SelectSingleNode("./descendant::div[@class='label' and text()[contains(.,'Types:')]]/following-sibling::*[1]");
            var types = typesDiv == null ? null : Regex.Replace(HtmlEntity.DeEntitize(typesDiv.InnerText).Trim(), @"\s+", " ");

            var cardTextDivs = node.SelectNodes("./descendant::div[@class='label' and text()[contains(.,'Card Text:')]]/following-sibling::*[1]/div");
            foreach (HtmlNode div in cardTextDivs ?? Enumerable.Empty<HtmlNode>())
            {
                var costImages = div.SelectNodes("./descendant::img");
                foreach (HtmlNode img in costImages ?? Enumerable.Empty<HtmlNode>())
                {
                    img.ParentNode.ReplaceChild(node.OwnerDocument.CreateTextNode(mapMana(img.Attributes["alt"].Value)), img);
                }
            }

            var cardText = cardTextDivs == null
                ? null
                : (from HtmlNode div in cardTextDivs
                   let text = HtmlEntity.DeEntitize(div.InnerText)
                   let fixedText = Regex.Replace(
                       text,
                       @"(?:\b|\G)(?:o(?:(?<name>[0-9X])|o?(?<name>[BUGRW])|c(?<name>[T]))|(?<name>[0-9XBUGRWT])(?=o(?:[0-9X]|o?[BUGRW]|c[T])))",
                       "{${name}}")
                   select fixedText).ToArray();

            var flavorTextDivs = node.SelectNodes("./descendant::div[@class='label' and text()[contains(.,'Flavor Text:')]]/following-sibling::*[1]/div");
            var flavorText = flavorTextDivs == null
                ? null
                : (from HtmlNode div in flavorTextDivs
                   select HtmlEntity.DeEntitize(div.InnerText)).ToArray();

            var ptDiv = node.SelectSingleNode("./descendant::div[@class='label' and text()[contains(.,'P/T:')]]/following-sibling::*[1]");
            var pt = ptDiv == null ? null : HtmlEntity.DeEntitize(ptDiv.InnerText).Trim().Replace("{1/2}", "½").Split('/');
            var power = pt == null ? null : pt[0].Trim();
            var toughness = pt == null ? null : pt[1].Trim();

            var hlDiv = node.SelectSingleNode("./descendant::div[@class='label' and text()[contains(.,'Hand/Life:')]]/following-sibling::*[1]");
            var hl = hlDiv == null ? null : Regex.Match(HtmlEntity.DeEntitize(hlDiv.InnerText), @"\(Hand Modifier: (?<hand>.+) , Life Modifier: (?<life>.+)\)");
            var hand = hl == null ? null : hl.Groups["hand"].Value;
            var life = hl == null ? null : hl.Groups["life"].Value;

            var loyaltyDiv = node.SelectSingleNode("./descendant::div[@class='label' and text()[contains(.,'Loyalty:')]]/following-sibling::*[1]");
            var loyalty = loyaltyDiv == null ? (int?)null : int.Parse(loyaltyDiv.InnerText.Trim());

            var cardNumberDiv = node.SelectSingleNode("./descendant::div[@class='label' and text()[contains(.,'Card #:')]]/following-sibling::*[1]");
            var cardNumber = cardNumberDiv == null ? null : cardNumberDiv.InnerText.Trim();

            var artistAnchor = node.SelectSingleNode("./descendant::div[@class='label' and text()[contains(.,'Artist:')]]/following-sibling::*[1]/a");
            var artist = artistAnchor == null ? null : HtmlEntity.DeEntitize(artistAnchor.InnerText.Trim());

            return new CardFace
            {
                Id = id,
                Artist = artist,
                CardNumber = cardNumber,
                CardText = cardText,
                ConvertedManaCost = convertedMana,
                FlavorText = flavorText,
                Hand = hand,
                Life = life,
                Loyalty = loyalty,
                ManaCost = mana,
                Name = name,
                Power = power,
                Toughness = toughness,
                Types = types,
            };
        }

        private static T UsingFetchState<T>(Func<FetchState, T> func)
        {
            T result;
            lock (fetchStateMutex)
            {
                using (var session = MvcApplication.DocumentStore.OpenSession())
                {
                    var fetchState = session.Load<FetchState>("current");
                    if (fetchState == null)
                    {
                        fetchState = new FetchState();
                        session.Store(fetchState, "current");
                    }

                    result = func(fetchState);

                    session.SaveChanges();
                }
            }

            return result;
        }

        private static void UsingFetchState(Action<FetchState> action)
        {
            var ignore = UsingFetchState(fetchState => { action(fetchState); return true; });
        }

        private class FetchState
        {
            public FetchState()
            {
                this.MissingCardRanges = new List<CardRange>();
            }

            public List<CardRange> MissingCardRanges { get; set; }

            public void AddMissingCard(int id)
            {
                this.MissingCardRanges =
                    this.MissingCardRanges
                    .Cast<IInterval<int>>()
                    .UnionWith(new CardRange { Start = id, End = id + 1 })
                    .Cast<CardRange>()
                    .ToList();
            }

            public bool IsCardMissing(int id)
            {
                return this.MissingCardRanges.Cast<IInterval<int>>().Contains(id);
            }

            public class CardRange : IInterval<int>
            {
                public IInterval<int> Clone(int start, bool startInclusive, int end, bool endInclusive)
                {
                    return new CardRange { Start = start, End = end };
                }

                public int Start { get; set; }

                public int End { get; set; }

                [JsonIgnore]
                bool IInterval<int>.StartInclusive
                {
                    get { return true; }
                }

                [JsonIgnore]
                bool IInterval<int>.EndInclusive
                {
                    get { return false; }
                }
            }
        }

        public class BackgroundScraper
        {
            private static readonly TimeSpan MinScrapeTimeSpan = TimeSpan.FromSeconds(0.5);
            private readonly List<int> cardsToCheck = new List<int>();
            private readonly Random rand = new Random();
            private int errorCount;

            public TimeSpan? ScrapeSingle()
            {
                // If we don't have any cards left to check (or if we have never populated the list).
                if (cardsToCheck.Count == 0)
                {

                    // If we still have no cards to check, we are finished.
                    if (cardsToCheck.Count == 0)
                    {
                        return null;
                    }
                }

                var ix = rand.Next(cardsToCheck.Count);
                var id = cardsToCheck[ix];

                try
                {
                    bool previouslyScraped;
                    using (var session = MvcApplication.DocumentStore.OpenSession())
                    {
                        session.ReadOrScrapeCard(id, out previouslyScraped);
                    }

                    this.errorCount = 0;
                    cardsToCheck.RemoveAt(ix);
                    if (previouslyScraped)
                    {
                        return TimeSpan.Zero;
                    }
                    else
                    {
                        return MinScrapeTimeSpan;
                    }
                }
                catch (WebException ex)
                {
                    this.errorCount++;
                    Debug.WriteLine("Error fetching card with id={0}.  The error message was:\n{1}", id, ex);
                    return TimeSpan.FromMilliseconds(Math.Pow(2, this.errorCount) * MinScrapeTimeSpan.TotalMilliseconds);
                }
            }
        }
    }
}