using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;

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
            return View();
        }

        public ActionResult Find(string q)
        {
            var url = string.Format(
                "http://gatherer.wizards.com/Handlers/InlineCardSearch.ashx?nameFragment={0}&cacheBust={1}",
                Uri.EscapeDataString(q),
                DateTime.Now.ToFileTimeUtc());

            using (var client = new WebClient())
            {
                return Content(client.DownloadString(url), "text/json");
            }
        }
    }
}
