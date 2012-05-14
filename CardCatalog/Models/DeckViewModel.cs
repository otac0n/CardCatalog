using System.Collections.Generic;
using System.Linq;

namespace CardCatalog.Models
{
    public class DeckViewModel
    {
        public DeckViewModel()
        {
            this.Columns = new List<Column>();
        }

        public int Id { get; set; }

        public List<Column> Columns { get; set; }

        public class Column
        {
            public Column()
            {
                this.Cards = new List<Card>();
            }

            public List<Card> Cards { get; set; }

            public static Column Convert(Deck.Column column, Dictionary<string, Card> cards)
            {
                return new Column
                {
                    Cards = column.CardIds.Select(c => cards[c]).ToList(),
                };
            }
        }

        public static DeckViewModel Convert(Deck deck, Dictionary<string, Card> cards)
        {
            return new DeckViewModel
            {
                Id = deck.Id,
                Columns = deck.Columns.Select(c => Column.Convert(c, cards)).ToList(),
            };
        }
    }
}