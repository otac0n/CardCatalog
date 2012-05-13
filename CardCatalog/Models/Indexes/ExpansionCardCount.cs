using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Raven.Client.Indexes;

namespace CardCatalog.Models.Indexes
{
    public class ExpansionCardCount : AbstractIndexCreationTask<Expansion, ExpansionCardCount.Result>
    {
        public ExpansionCardCount()
        {
            this.Map = expansions => from e in expansions
                                     select new Result
                                     {
                                         Name = e.Name,
                                         Count = e.Cards.Count,
                                     };
            this.Reduce = results => from r in results
                                     group r by r.Name into g
                                     select new Result
                                     {
                                         Name = g.Key,
                                         Count = g.Sum(e => e.Count),
                                     };
        }

        public class Result
        {
            public string Name { get; set; }

            public int Count { get; set; }
        }
    }
}