using System;
using System.Linq;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace CardCatalog.Models.Indexes
{
    public class CardSearch : AbstractMultiMapIndexCreationTask<CardSearch.Result>
    {
        public CardSearch()
        {
            this.AddMap<Card>(cards => from c in cards
                                       select new Result
                                       {
                                           CardId = c.Id.ToString(),
                                           Expansion = c.Expansion,
                                           Owned = 0,
                                           Rarity = c.Rarity,
                                           Artist = string.Join(Environment.NewLine, c.NormalizedFaces.Select(f => f.Artist)),
                                           Colors = string.Join(" ", c.Colors),
                                           ConvertedManaCost = c.NormalizedFaces.Select(f => f.ConvertedManaCost).Where(x => x != null).FirstOrDefault(),
                                           Name = string.Join(Environment.NewLine, c.NormalizedFaces.Select(f => f.Name).Where(x => !string.IsNullOrEmpty(x))),
                                           Power = string.Join(Environment.NewLine, c.NormalizedFaces.Select(f => f.Power).Where(x => !string.IsNullOrEmpty(x))),
                                           Text = string.Join(Environment.NewLine,
                                               c.NormalizedFaces.Select(f => f.Name)
                                               .Concat(c.NormalizedFaces.SelectMany(f => f.CardText))
                                               .Concat(c.NormalizedFaces.Select(f => f.Types))
                                               .Concat(c.NormalizedFaces.SelectMany(f => f.FlavorText))),
                                           Toughness = string.Join(Environment.NewLine, c.NormalizedFaces.Select(f => f.Toughness).Where(x => !string.IsNullOrEmpty(x))),
                                           Types = string.Join(Environment.NewLine, c.NormalizedFaces.Select(f => f.Types).Where(x => !string.IsNullOrEmpty(x))),
                                       });

            this.AddMap<Ownership>(ownerships => from o in ownerships
                                                 select new Result
                                                 {
                                                     CardId = o.CardId,
                                                     Owned = 1,
                                                     Expansion = string.Empty,
                                                     Rarity = string.Empty,
                                                     Artist = string.Empty,
                                                     Colors = string.Empty,
                                                     ConvertedManaCost = -1,
                                                     Name = string.Empty,
                                                     Power = string.Empty,
                                                     Text = string.Empty,
                                                     Toughness = string.Empty,
                                                     Types = string.Empty,
                                                 });

            this.Reduce = results => from r in results
                                     group r by r.CardId into g
                                     select new Result
                                     {
                                         CardId = g.Key,
                                         Owned = g.Sum(r => r.Owned),
                                         Expansion = g.Select(r => r.Expansion).Where(x => !string.IsNullOrEmpty(x)).FirstOrDefault(),
                                         Rarity = g.Select(r => r.Rarity).Where(x => !string.IsNullOrEmpty(x)).FirstOrDefault(),
                                         Artist = string.Join(Environment.NewLine, g.Select(r => r.Artist).Where(x => !string.IsNullOrEmpty(x))),
                                         Colors = string.Join(" ", g.Select(r => r.Colors).Where(x => !string.IsNullOrEmpty(x))),
                                         ConvertedManaCost = g.Select(r => r.ConvertedManaCost).Where(x => x != null && x != -1).FirstOrDefault(),
                                         Name = string.Join(Environment.NewLine, g.Select(r => r.Name).Where(x => !string.IsNullOrEmpty(x))),
                                         Power = string.Join(Environment.NewLine, g.Select(r => r.Power).Where(x => !string.IsNullOrEmpty(x))),
                                         Text = string.Join(Environment.NewLine, g.Select(r => r.Text).Where(x => !string.IsNullOrEmpty(x))),
                                         Toughness = string.Join(Environment.NewLine, g.Select(r => r.Toughness).Where(x => !string.IsNullOrEmpty(x))),
                                         Types = string.Join(Environment.NewLine, g.Select(r => r.Types).Where(x => !string.IsNullOrEmpty(x))),
                                     };

            this.Index(r => r.Expansion, FieldIndexing.NotAnalyzed);
            this.Index(r => r.Rarity, FieldIndexing.NotAnalyzed);
            this.Index(r => r.Artist, FieldIndexing.Analyzed);
            this.Index(r => r.Colors, FieldIndexing.Analyzed);
            this.Index(r => r.ConvertedManaCost, FieldIndexing.Analyzed);
            this.Index(r => r.Name, FieldIndexing.Analyzed);
            this.Index(r => r.Power, FieldIndexing.NotAnalyzed);
            this.Index(r => r.Text, FieldIndexing.Analyzed);
            this.Index(r => r.Toughness, FieldIndexing.NotAnalyzed);
            this.Index(r => r.Types, FieldIndexing.Analyzed);

            this.Sort(r => r.ConvertedManaCost, SortOptions.Int);
        }

        public class Result
        {
            public string CardId { get; set; }

            public string Expansion { get; set; }

            public int Owned { get; set; }

            public string Rarity { get; set; }

            public string Artist { get; set; }

            public string Colors { get; set; }

            public decimal? ConvertedManaCost { get; set; }

            public string Name { get; set; }

            public string Power { get; set; }

            public string Text { get; set; }

            public string Toughness { get; set; }

            public string Types { get; set; }
        }
    }
}