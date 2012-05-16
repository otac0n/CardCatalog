using System.Collections.Generic;

namespace CardCatalog.Models
{
    public class Deck
    {
        public Deck()
        {
            this.Columns = new List<Column>();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public List<Column> Columns { get; set; }

        public class Column
        {
            public Column()
            {
                this.CardIds = new List<string>();
            }

            public List<string> CardIds { get; set; }
        }
    }
}