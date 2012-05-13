using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CardCatalog.Models;
using CardCatalog.Models.Indexes;

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
            using (var session = MvcApplication.DocumentStore.OpenSession())
            {
                ViewBag.Expansions = session.Query<ExpansionCardCount.Result, ExpansionCardCount>().Take(1000).ToList();
            }

            return View(new Deck());
        }
    }
}
