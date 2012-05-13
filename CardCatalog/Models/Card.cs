using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CardCatalog.Models
{
    public class Card
    {
        private static Dictionary<char, string> colorMap = new Dictionary<char, string>
        {
            { 'B', "Black" },
            { 'U', "Blue" },
            { 'G', "Green" },
            { 'R', "Red" },
            { 'W', "White" },
        };

        public int Id { get; set; }

        public string Expansion { get; set; }

        public string Rarity { get; set; }

        public IList<string> Colors
        {
            get
            {
                return (from f in this.NormalizedFaces ?? Enumerable.Empty<CardFace>()
                        from c in f.ManaCost ?? Enumerable.Empty<char>()
                        where colorMap.ContainsKey(c)
                        select colorMap[c])
                       .Distinct()
                       .ToList();
            }
        }

        public List<CardFace> NormalizedFaces { get; set; }

        public List<CardFace> PrintedFaces { get; set; }
    }
}
