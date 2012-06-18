namespace CardCatalog.Models
{
    using System.Collections.Generic;

    public class Format
    {
        public Format()
        {
            this.Expansions = new List<string>();
            this.BannedCards = new List<string>();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public List<string> Expansions { get; set; }

        public List<string> BannedCards { get; set; }
    }
}