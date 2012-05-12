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

        public List<CardFace> NormalizedFaces { get; set; }
    }
}
