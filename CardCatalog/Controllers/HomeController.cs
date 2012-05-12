using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CardCatalog.Models;
using CardCatalog.Models.Indexes;

namespace CardCatalog.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            using (var session = MvcApplication.DocumentStore.OpenSession())
            {
                var ownedCards = session.Query<CardOwnershipCount.Result, CardOwnershipCount>().ToList();

                var cards = session.Load<Card>(ownedCards.Select(o => o.CardId));

                return View(cards);
            }
        }

        public ActionResult About()
        {
            return View();
        }
    }
}
