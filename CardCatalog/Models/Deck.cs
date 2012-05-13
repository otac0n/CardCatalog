using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CardCatalog.Models
{
    public class Deck
    {
        public string Name { get; set; }

        public string OwnerId { get; set; }

        public List<string> Columns { get; set; }

        public class Column
        {
            public List<string> CardIds { get; set; }
        }
    }
}
