using System.Linq;
using Raven.Client.Indexes;

namespace CardCatalog.Models.Indexes
{
    public class CardOwnershipCount : AbstractIndexCreationTask<Ownership, CardOwnershipCount.Result>
    {
        public CardOwnershipCount()
        {
            this.Map = ownerships => from o in ownerships
                                     select new Result
                                     {
                                         CardId = o.CardId,
                                         Count = 1,
                                     };

            this.Reduce = results => from r in results
                                     group r by r.CardId into g
                                     select new Result
                                     {
                                         CardId = g.Key,
                                         Count = g.Sum(r => r.Count),
                                     };
        }

        public class Result
        {
            public string CardId { get; set; }

            public int Count { get; set; }
        }
    }
}