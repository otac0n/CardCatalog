using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CardCatalog.Models
{
    public class Card
    {
        public int Id { get; set; }

        public string Expansion { get; set; }

        public string Rarity { get; set; }

        public CardFace FrontFace { get; set; }

        public CardFace BackFace { get; set; }
    }
}
