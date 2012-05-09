using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;
using CardCatalog.Models;
using HtmlAgilityPack;

namespace CardCatalog.Controllers
{
    public class CardController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Details(int id)
        {
            using (var session = MvcApplication.DocumentStore.OpenSession())
            {
                var card = session.Load<Card>(id);

                if (card == null)
                {
                    card = ScrapeCard(id);
                    session.Store(card);
                    session.SaveChanges();
                }

                var ownerships = (from o in session.Query<Ownership>()
                                  where o.CardId == id
                                  select o).ToList();

                ViewBag.Ownerships = ownerships;
                return View(card);
            }
        }

        public ActionResult Add(int id)
        {
            string userName = this.User == null || string.IsNullOrWhiteSpace(this.User.Identity.Name)
                ? null
                : this.User.Identity.Name;

            using (var session = MvcApplication.DocumentStore.OpenSession())
            {
                session.Store(new Ownership { CardId = id, Owner = userName });
                session.SaveChanges();
            }

            return RedirectToAction("details", new { id });
        }

        public ActionResult Find(string q)
        {
            var url = string.Format(
                "http://gatherer.wizards.com/Handlers/InlineCardSearch.ashx?nameFragment={0}&cacheBust={1}",
                Uri.EscapeDataString(q),
                DateTime.Now.ToFileTimeUtc());

            using (var client = new WebClient())
            {
                return Content(client.DownloadString(url), "text/json");
            }
        }

        private Card ScrapeCard(int id)
        {
            var doc = new HtmlDocument();
            using (var client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                var html = client.DownloadString("http://gatherer.wizards.com/Pages/Card/Details.aspx?multiverseid=" + id);
                doc.LoadHtml(html);
            }

            Func<string, string> mapMana = cost =>
                cost == "Black" ? "{B}" :
                cost == "Blue" ? "{U}" :
                cost == "Red" ? "{R}" :
                cost == "White" ? "{W}" :
                cost == "Green" ? "{G}" :
                "{" + cost + "}";

            var nameDiv = doc.DocumentNode.SelectSingleNode("//div[@class='label' and text()[contains(.,'Card Name:')]]/following-sibling::*[1]");
            var name = HtmlEntity.DeEntitize(nameDiv.InnerText.Trim());

            var manaImages = doc.DocumentNode.SelectNodes("//div[@class='label' and text()[contains(.,'Mana Cost:')]]/following-sibling::*[1]/img");
            var mana = manaImages == null
                ? null
                : string.Join("", from HtmlNode image in manaImages
                                  let cost = HtmlEntity.DeEntitize(image.Attributes["alt"].Value)
                                  select mapMana(cost));

            var convertedManaDiv = doc.DocumentNode.SelectSingleNode("//div[@class='label' and text()[contains(.,'Converted Mana Cost:')]]/following-sibling::*[1]");
            var convertedMana = convertedManaDiv == null ? (int?)null : int.Parse(convertedManaDiv.InnerText.Trim());

            var typesDiv = doc.DocumentNode.SelectSingleNode("//div[@class='label' and text()[contains(.,'Types:')]]/following-sibling::*[1]");
            var types = typesDiv == null ? null : HtmlEntity.DeEntitize(typesDiv.InnerText.Trim());

            var cardTextDivs = doc.DocumentNode.SelectNodes("//div[@class='label' and text()[contains(.,'Card Text:')]]/following-sibling::*[1]/div");
            foreach (HtmlNode div in cardTextDivs ?? Enumerable.Empty<HtmlNode>())
            {
                var costImages = div.SelectNodes("./img");
                foreach (HtmlNode img in costImages ?? Enumerable.Empty<HtmlNode>())
                {
                    div.ReplaceChild(doc.CreateTextNode(mapMana(img.Attributes["alt"].Value)), img);
                }
            }

            var cardText = cardTextDivs == null
                ? null
                : (from HtmlNode div in cardTextDivs
                   select HtmlEntity.DeEntitize(div.InnerText)).ToArray();

            var flavorTextDivs = doc.DocumentNode.SelectNodes("//div[@class='label' and text()[contains(.,'Flavor Text:')]]/following-sibling::*[1]/div");
            var flavorText = flavorTextDivs == null
                ? null
                : (from HtmlNode div in flavorTextDivs
                   select HtmlEntity.DeEntitize(div.InnerText)).ToArray();

            var ptDiv = doc.DocumentNode.SelectSingleNode("//div[@class='label' and text()[contains(.,'P/T:')]]/following-sibling::*[1]");
            var pt = ptDiv == null ? null : ptDiv.InnerText.Split('/');
            var power = pt == null ? null : pt[0].Trim();
            var toughness = pt == null ? null : pt[1].Trim();

            var expansionAnchor = doc.DocumentNode.SelectSingleNode("//div[@class='label' and text()[contains(.,'Expansion:')]]/following-sibling::*[1]/div/a[2]");
            var expansion = expansionAnchor == null ? null : HtmlEntity.DeEntitize(expansionAnchor.InnerText.Trim());

            var raritySpan = doc.DocumentNode.SelectSingleNode("//div[@class='label' and text()[contains(.,'Rarity:')]]/following-sibling::*[1]/span");
            var rarity = raritySpan == null ? null : HtmlEntity.DeEntitize(raritySpan.Attributes["class"].Value);

            var cardNumberDiv = doc.DocumentNode.SelectSingleNode("//div[@class='label' and text()[contains(.,'Card #:')]]/following-sibling::*[1]");
            var cardNumber = cardNumberDiv == null ? null : cardNumberDiv.InnerText.Trim();

            var artistAnchor = doc.DocumentNode.SelectSingleNode("//div[@class='label' and text()[contains(.,'Artist:')]]/following-sibling::*[1]/a");
            var artist = artistAnchor == null ? null : HtmlEntity.DeEntitize(artistAnchor.InnerText.Trim());

            return new Card
            {
                Id = id,
                Artist = artist,
                CardNumber = cardNumber,
                CardText = cardText,
                ConvertedManaCost = convertedMana,
                Expansion = expansion,
                FlavorText = flavorText,
                ManaCost = mana,
                Name = name,
                Power = power,
                Rarity = rarity,
                Toughness = toughness,
                Types = types,
            };
        }
    }
}
