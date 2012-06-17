using System;
using System.Linq;
using System.Web.Mvc;
using CardCatalog.Models;
using CardCatalog.Models.Indexes;
using Raven.Client.Linq;

namespace CardCatalog.Controllers
{
    public class DeckController : Controller
    {
        public ActionResult Index(int page = 1)
        {
            const int ResultsPerPage = 50;
            if (page < 1)
            {
                page = 1;
            }

            using (var session = MvcApplication.DocumentStore.OpenSession())
            {
                var results = session.Query<Deck>().Skip((page - 1) * ResultsPerPage).Take(ResultsPerPage).ToList();

                return View(results);
            }
        }

        public ActionResult Create()
        {
            using (var session = MvcApplication.DocumentStore.OpenSession())
            {
                ViewBag.Expansions = session.Query<ExpansionCardCount.Result, ExpansionCardCount>().Take(1000).ToList();
            }

            return View("Edit", new DeckViewModel());
        }

        [HttpPost]
        public ActionResult Create(Deck deck)
        {
            if (!ModelState.IsValid)
            {
                throw new Exception("Fuck!");
            }

            using (var session = MvcApplication.DocumentStore.OpenSession())
            {
                session.Store(deck);
                session.SaveChanges();

                return RedirectToAction("edit", new { id = deck.Id, slug = deck.Name.Slugify() });
            }
        }

        public ActionResult Edit(int id, string slug = "")
        {
            DeckViewModel viewModel;
            using (var session = MvcApplication.DocumentStore.OpenSession())
            {
                var deck = session.Load<Deck>(id);

                if (deck == null)
                {
                    return HttpNotFound();
                }

                if (deck.Name.Slugify() != slug)
                {
                    return RedirectToAction("edit", new { id = deck.Id, slug = deck.Name.Slugify() });
                }

                var cards = session.Load<Card>(deck.Columns.SelectMany(c => c.CardIds).Distinct()).ToDictionary(c => "cards/" + c.Id, c => c);

                viewModel = DeckViewModel.Convert(deck, cards);
                ViewBag.Expansions = session.Query<ExpansionCardCount.Result, ExpansionCardCount>().Take(1000).ToList();
            }

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Edit(int id, Deck deck)
        {
            if (!ModelState.IsValid)
            {
                throw new Exception("Fuck!");
            }

            using (var session = MvcApplication.DocumentStore.OpenSession())
            {
                deck.Id = id;
                session.Store(deck);
                session.SaveChanges();

                return RedirectToAction("edit", new { id = deck.Id, slug = deck.Name.Slugify() });
            }
        }
    }
}