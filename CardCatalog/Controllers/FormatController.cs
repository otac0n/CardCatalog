using System;
using System.Linq;
using System.Web.Mvc;
using CardCatalog.Models;
using CardCatalog.Models.Indexes;

namespace CardCatalog.Controllers
{
    public class FormatController : Controller
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
                var results = session.Query<Format>().Skip((page - 1) * ResultsPerPage).Take(ResultsPerPage).ToList();

                return View(results);
            }
        }

        public ActionResult Create()
        {
            using (var session = MvcApplication.DocumentStore.OpenSession())
            {
                ViewBag.Expansions = session.Query<ExpansionCardCount.Result, ExpansionCardCount>().Take(1000).ToList();
            }

            return View("Edit", new FormatViewModel());
        }

        [HttpPost]
        public ActionResult Create(Format format)
        {
            if (!ModelState.IsValid)
            {
                throw new Exception("Fuck!");
            }

            using (var session = MvcApplication.DocumentStore.OpenSession())
            {
                session.Store(format);
                session.SaveChanges();

                return RedirectToAction("edit", new { id = format.Id, slug = format.Name.Slugify() });
            }
        }

        public ActionResult Edit(int id, string slug = "")
        {
            FormatViewModel viewModel;
            using (var session = MvcApplication.DocumentStore.OpenSession())
            {
                var format = session.Load<Format>(id);

                if (format == null)
                {
                    return HttpNotFound();
                }

                if (format.Name.Slugify() != slug)
                {
                    return RedirectToAction("edit", new { id = format.Id, slug = format.Name.Slugify() });
                }

                var cards = session.Load<Card>(format.BannedCardIds.Distinct()).ToDictionary(c => "cards/" + c.Id, c => c);

                viewModel = FormatViewModel.Convert(format, cards);
                ViewBag.Expansions = session.Query<ExpansionCardCount.Result, ExpansionCardCount>().Take(1000).ToList();
            }

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Edit(int id, Format format)
        {
            if (!ModelState.IsValid)
            {
                throw new Exception("Fuck!");
            }

            using (var session = MvcApplication.DocumentStore.OpenSession())
            {
                format.Id = id;
                session.Store(format);
                session.SaveChanges();

                return RedirectToAction("edit", new { id = format.Id, slug = format.Name.Slugify() });
            }
        }
    }
}