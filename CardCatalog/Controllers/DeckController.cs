﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CardCatalog.Models;

namespace CardCatalog.Controllers
{
    public class DeckController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            return View(new Deck());
        }
    }
}