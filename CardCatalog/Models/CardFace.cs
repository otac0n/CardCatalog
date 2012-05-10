﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CardCatalog.Models
{
    public class CardFace
    {
        public int Id { get; set; }

        public string Artist { get; set; }

        public string CardNumber { get; set; }

        public string[] CardText { get; set; }

        public int? ConvertedManaCost { get; set; }

        public string[] FlavorText { get; set; }

        public string ManaCost { get; set; }

        public string Name { get; set; }

        public string Power { get; set; }

        public string Toughness { get; set; }

        public string Types { get; set; }
    }
}