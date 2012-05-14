using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;
using CardCatalog.Models;
using Raven.Client.Linq;
using System.Collections.Generic;
using CardCatalog.Models.Indexes;
using System.Linq.Expressions;

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

                if (card == null)
                {
                    return HttpNotFound();
                }

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

        public ActionResult Search()
        {
            var search = (from k in Request.QueryString.AllKeys
                          from v in Request.QueryString.GetValues(k)
                          select new
                          {
                              Field = k,
                              Value = v,
                          });

            using (var session = MvcApplication.DocumentStore.OpenSession())
            {
                var searchQuery = session.Advanced.LuceneQuery<CardSearch.Result, CardSearch>();
                bool needsAnd = false;
                foreach (var term in search)
                {
                    if (needsAnd)
                    {
                        searchQuery = searchQuery.AndAlso();
                    }

                    needsAnd = true;

                    switch (term.Field.ToLower())
                    {
                        case "artist":
                            searchQuery = searchQuery.WhereContains("Artist", term.Value);
                            break;

                        case "cost":
                            searchQuery = searchQuery.WhereEquals("ConvertedManaCost", term.Value);
                            break;

                        case "color":
                            searchQuery = searchQuery.WhereContains("Colors", term.Value);
                            break;

                        case "expansion":
                            searchQuery = searchQuery.WhereEquals("Expansion", term.Value);
                            break;

                        case "name":
                            searchQuery = searchQuery.WhereContains("Name", term.Value);
                            break;

                        case "owned":
                            if (term.Value == "true")
                            {
                                searchQuery = searchQuery.WhereGreaterThanOrEqual("Owned", 1);
                            }
                            else
                            {
                                searchQuery = searchQuery.WhereLessThan("Owned", 1);
                            }

                            break;

                        case "power":
                            searchQuery = searchQuery.WhereEquals("Power", term.Value);
                            break;

                        case "rarity":
                            searchQuery = searchQuery.WhereEquals("Rarity", term.Value);
                            break;

                        case "toughness":
                            searchQuery = searchQuery.WhereEquals("Toughness", term.Value);
                            break;

                        case "type":
                            searchQuery = searchQuery.WhereContains("Types", term.Value);
                            break;

                        case "text":
                            searchQuery = searchQuery.WhereContains("Text", term.Value);;
                            break;

                        default:
                            searchQuery = searchQuery.WhereEquals(term.Field, term.Value);
                            break;
                    }
                }

                RavenQueryStatistics stats;
                var results = searchQuery.OrderBy("Name").Statistics(out stats).Include(x => x.CardId).ToList();
                var cards = session.Load<Card>(results.Select(r => r.CardId));

                return Json(cards, JsonRequestBehavior.AllowGet);
            }
        }
    }
}