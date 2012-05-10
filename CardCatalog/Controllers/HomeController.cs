using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CardCatalog.Models;

namespace CardCatalog.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            using (var session = MvcApplication.DocumentStore.OpenSession())
            {
                var ownedCards = session.Query<OwnershipCount>("CardOwnershipCounts").ToList();

                var cards = session.Load<Card>(ownedCards.Select(o => "cards/" + o.CardId));

                return View(cards);
            }
        }

        public ActionResult About()
        {
            return View();
        }

        private class OwnershipCount
        {
            public int CardId { get; set; }
            public int Count { get; set; }
        }
    }
}
