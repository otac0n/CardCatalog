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
                                    Artist = c.NormalizedFaces.Select(f => f.Artist),
                                    ConvertedManaCost = c.NormalizedFaces.Select(f => f.ConvertedManaCost),
                                    Name = c.NormalizedFaces.Select(f => f.Name),
                                    Power = c.NormalizedFaces.Select(f => f.Power),
                                    Toughness = c.NormalizedFaces.Select(f => f.Toughness),
                                    Types = c.NormalizedFaces.Select(f => f.Types),
                                };

            this.Index(r => r.Expansion, FieldIndexing.NotAnalyzed);
            this.Index(r => r.Rarity, FieldIndexing.NotAnalyzed);
            this.Index(r => r.Artist, FieldIndexing.Analyzed);
            this.Index(r => r.ConvertedManaCost, FieldIndexing.NotAnalyzed);
            this.Index(r => r.Name, FieldIndexing.Analyzed);
            this.Index(r => r.Power, FieldIndexing.NotAnalyzed);
            this.Index(r => r.Toughness, FieldIndexing.NotAnalyzed);
            this.Index(r => r.Types, FieldIndexing.Analyzed);

            this.Sort(r => r.ConvertedManaCost, SortOptions.Int);
        }

        public class Result
        {
            public string Expansion { get; set; }

            public string Rarity { get; set; }

            public IEnumerable<string> Artist { get; set; }

            public IEnumerable<int?> ConvertedManaCost { get; set; }

            public IEnumerable<string> Name { get; set; }

            public IEnumerable<string> Power { get; set; }

            public IEnumerable<string> Toughness { get; set; }

            public IEnumerable<string> Types { get; set; }
        }
    }
}