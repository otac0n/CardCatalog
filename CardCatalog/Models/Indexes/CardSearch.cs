using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Raven.Client.Indexes;
using Raven.Abstractions.Indexing;

namespace CardCatalog.Models.Indexes
{
    public class CardSearch : AbstractIndexCreationTask<Card, CardSearch.Result>
    {
        public CardSearch()
        {
            this.Map = cards => from c in cards
                                select new Result
                                {
                                    Expansion = c.Expansion,
                                    Rarity = c.Rarity,
                                    Artist = string.Join(Environment.NewLine, c.NormalizedFaces.Select(f => f.Artist)),
                                    Colors = string.Join(" ", c.Colors),
                                    ConvertedManaCost = c.NormalizedFaces.Select(f => f.ConvertedManaCost).First(),
                                    Name = string.Join(Environment.NewLine, c.NormalizedFaces.Select(f => f.Name)),
                                    Power = c.NormalizedFaces.Select(f => f.Power).First(),
                                    Text = string.Join(Environment.NewLine,
                                        c.NormalizedFaces.Select(f => f.Name)
                                        .Concat(c.NormalizedFaces.SelectMany(f => f.CardText))
                                        .Concat(c.NormalizedFaces.Select(f => f.Types))
                                        .Concat(c.NormalizedFaces.SelectMany(f => f.FlavorText))),
                                    Toughness = c.NormalizedFaces.Select(f => f.Toughness).First(),
                                    Types = string.Join(Environment.NewLine, c.NormalizedFaces.Select(f => f.Types)),
                                };

            this.Index(r => r.Expansion, FieldIndexing.NotAnalyzed);
            this.Index(r => r.Rarity, FieldIndexing.NotAnalyzed);
            this.Index(r => r.Artist, FieldIndexing.Analyzed);
            this.Index(r => r.Colors, FieldIndexing.Analyzed);
            this.Index(r => r.ConvertedManaCost, FieldIndexing.NotAnalyzed);
            this.Index(r => r.Name, FieldIndexing.Analyzed);
            this.Index(r => r.Power, FieldIndexing.NotAnalyzed);
            this.Index(r => r.Text, FieldIndexing.Analyzed);
            this.Index(r => r.Toughness, FieldIndexing.NotAnalyzed);
            this.Index(r => r.Types, FieldIndexing.Analyzed);

            this.Sort(r => r.ConvertedManaCost, SortOptions.Int);
        }

        public class Result
        {
            public string Expansion { get; set; }

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