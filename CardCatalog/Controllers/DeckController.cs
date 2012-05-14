using System;
using System.Linq;
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

                return RedirectToAction("edit", new { id = deck.Id });
            }
        }

        public ActionResult Edit(int id)
        {
            DeckViewModel viewModel;
            using (var session = MvcApplication.DocumentStore.OpenSession())
            {
                ViewBag.Expansions = session.Query<ExpansionCardCount.Result, ExpansionCardCount>().Take(1000).ToList();
                var deck = session.Load<Deck>(id);
                var cards = session.Load<Card>(deck.Columns.SelectMany(c => c.CardIds).Distinct()).ToDictionary(c => "cards/" + c.Id, c => c);

                viewModel = DeckViewModel.Convert(deck, cards);
            }

            return View((DeckViewModel)viewModel);
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

                return RedirectToAction("edit", new { id = deck.Id });
            }
        }
    }
}