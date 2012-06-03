using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;
using CardCatalog.Models;
using CardCatalog.Models.Indexes;
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

        public ActionResult Search(int page = 1)
        {
            const int ResultsPerPage = 14;
            if (page < 1)
            {
                page = 1;
            }

            var search = (from k in Request.QueryString.AllKeys
                          where k.ToLower() != "page"
                          from v in Request.QueryString.GetValues(k)
                          select new
                          {
                              Field = k,
                              Value = v,
                          });

            using (var session = MvcApplication.DocumentStore.OpenSession())
            {
                var searchQuery = session.Query<CardSearch.Result, CardSearch>();

                foreach (var termIterm in search)
                {
                    var term = termIterm; // Capture variable for closure.

                    switch (term.Field.ToLower())
                    {
                        case "artist":
                            searchQuery = searchQuery.Where(r => r.Artist == term.Value);
                            break;

                        case "cost":
                            var value = term.Value.Trim();
                            if (value.StartsWith(">="))
                            {
                                var cost = decimal.Parse(value.Substring(2).Trim());
                                searchQuery = searchQuery.Where(r => r.ConvertedManaCost >= cost);
                            }
                            else if (value.StartsWith(">"))
                            {
                                var cost = decimal.Parse(value.Substring(1).Trim());
                                searchQuery = searchQuery.Where(r => r.ConvertedManaCost > cost);
                            }
                            else if (value.StartsWith("<="))
                            {
                                var cost = decimal.Parse(value.Substring(2).Trim());
                                searchQuery = searchQuery.Where(r => r.ConvertedManaCost <= cost);
                            }
                            else if (value.StartsWith("<"))
                            {
                                var cost = decimal.Parse(value.Substring(1).Trim());
                                searchQuery = searchQuery.Where(r => r.ConvertedManaCost < cost);
                            }
                            else
                            {
                                var cost = decimal.Parse(value);
                                searchQuery = searchQuery.Where(r => r.ConvertedManaCost == cost);
                            }

                            break;

                        case "color":
                            searchQuery = searchQuery.Where(r => r.Colors == term.Value);
                            break;

                        case "expansion":
                            searchQuery = searchQuery.Where(r => r.Expansion == term.Value);
                            break;

                        case "name":
                            searchQuery = searchQuery.Where(r => r.Name == term.Value);
                            break;

                        case "owned":
                            if (term.Value == "true")
                            {
                                searchQuery = searchQuery.Where(r => r.Owned >= 1);
                            }
                            else
                            {
                                searchQuery = searchQuery.Where(r => r.Owned < 1);
                            }

                            break;

                        case "power":
                            searchQuery = searchQuery.Where(r => r.Power == term.Value);
                            break;

                        case "rarity":
                            searchQuery = searchQuery.Where(r => r.Rarity == term.Value);
                            break;

                        case "toughness":
                            searchQuery = searchQuery.Where(r => r.Toughness == term.Value);
                            break;

                        case "type":
                            searchQuery = searchQuery.Where(r => r.Types == term.Value);
                            break;

                        case "text":
                            searchQuery = searchQuery.Where(r => r.Text == term.Value);
                            break;

                        default:
                            throw new ArgumentException("Unknown query parameter.", term.Field);
                            break;
                    }
                }

                RavenQueryStatistics stats;
                var results = searchQuery
                    .Include(x => x.CardId)
                    .Statistics(out stats)
                    .OrderBy(r => r.Name)
                    .Skip((page - 1) * ResultsPerPage)
                    .Take(ResultsPerPage)
                    .ToList();
                var cards = session
                    .Load<Card>(results.Select(r => r.CardId));
                var pages = (stats.TotalResults + ResultsPerPage - 1) / ResultsPerPage;

                var ownershipCounts = session
                    .Query<CardOwnershipCount.Result, CardOwnershipCount>()
                    .Where(r => r.CardId.In(cards.Select(c => "cards/" + c.Id)))
                    .ToLookup(o => o.CardId, o => o.Count);

                var output = new
                {
                    Page = pages == 0 ? 0 : page,
                    Pages = pages,
                    CardsInfo = (from c in cards
                                 let ownedCount = ownershipCounts["cards/" + c.Id].SingleOrDefault()
                                 select new
                                 {
                                     Card = c,
                                     OwnedCount = ownedCount,
                                 }).ToList()
                };

                return Json(output, JsonRequestBehavior.AllowGet);
            }
        }
    }
}