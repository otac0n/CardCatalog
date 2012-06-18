namespace CardCatalog.Models
{
    using System.Collections.Generic;
    using System.Linq;

    public class FormatViewModel
    {
        public FormatViewModel()
        {
            this.Expansions = new List<string>();
            this.BannedCards = new List<Card>();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public List<string> Expansions { get; set; }

        public List<Card> BannedCards { get; set; }

        public static FormatViewModel Convert(Format format, Dictionary<string, Card> cards)
        {
            return new FormatViewModel
            {
                Id = format.Id,
                Name = format.Name,
                Expansions = format.Expansions.ToList(),
                BannedCards = format.BannedCards.Select(c => cards[c]).ToList(),
            };
        }
    }
}