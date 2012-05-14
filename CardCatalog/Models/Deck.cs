using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CardCatalog.Models
{
    public class Deck
    {
        public Deck()
        {
            this.Columns = new List<Column>();
        }

        public int Id { get; set; }

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
