using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CardCatalog.Models
{
    public class Ownership
    {
        public int Id { get; set; }
        public int CardId { get; set; }
        public string Owner { get; set; }
    }
}