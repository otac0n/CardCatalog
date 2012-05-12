using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;
using CardCatalog.Models;
using Raven.Client.Linq;

namespace CardCatalog.Controllers
{
    public class CardController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Details(int id)
        {
            using (var session = MvcApplication.DocumentStore.OpenSession())
            {
                var card = session.ReadOrScrapeCard(id);

                if (card.Id != id)
                {
                    return RedirectToActionPermanent("details", new { id = card.Id });
                }

                var cardId = "cards/" + id;
                var ownerships = (from o in session.Query<Ownership>()
                                  where o.CardId == cardId
                                  select o)
                                 .Customize(x => x.WaitForNonStaleResultsAsOfLastWrite())
                                 .ToList();

                ViewBag.Ownerships = ownerships;
                return View(card);
            }
        }

        public ActionResult Add(int id, int quantity)
        {
            string userName = this.User == null || string.IsNullOrWhiteSpace(this.User.Identity.Name)
                ? null
                : this.User.Identity.Name;

            using (var session = MvcApplication.DocumentStore.OpenSession())
            {
                var cardId = "cards/" + id;
                for (int i = 0; i < quantity; i++)
                {
                    session.Store(new Ownership { CardId = cardId, Owner = userName });
                }

                session.SaveChanges();
            }

            return RedirectToAction("details", new { id });
        }

        public ActionResult Find(string q)
        {
            var url = string.Format(
                "http://gatherer.wizards.com/Handlers/InlineCardSearch.ashx?nameFragment={0}&cacheBust={1}",
                Uri.EscapeDataString(q),
                DateTime.Now.ToFileTimeUtc());

            using (var client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                return Content(client.DownloadString(url), "text/json");
            }
        }
    }
}