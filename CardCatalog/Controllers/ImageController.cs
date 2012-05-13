using System.Net;
using System.Web.Mvc;
using System.Web.Routing;

namespace CardCatalog.Controllers
{
    public class ImageController : Controller
    {
        public ActionResult Card(int id, int side)
        {
            var fileName = Server.MapPath("~/Images/card-" + id + "-" + side + ".jpg");

            if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(fileName)))
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(fileName));
            }

            if (!System.IO.File.Exists(fileName))
            {
                Models.Card card;
                using (var session = MvcApplication.DocumentStore.OpenSession())
                {
                    card = session.ReadOrScrapeCard(id);
                }

                if (card == null || card.Id != id || card.NormalizedFaces.Count <= side)
                {
                    return HttpNotFound();
                }

                string address;
                if (side == 0)
                {
                    address = string.Format("http://gatherer.wizards.com/Handlers/Image.ashx?multiverseid={0}&type=card", id);
                }
                else if (card.NormalizedFaces[side].Id == id)
                {
                    address = string.Format("http://gatherer.wizards.com/Handlers/Image.ashx?multiverseid={0}&type=card&options=rotate180", id);
                }
                else
                {
                    address = string.Format("http://gatherer.wizards.com/Handlers/Image.ashx?multiverseid={0}&type=card", card.NormalizedFaces[side].Id);
                }

                using (var client = new WebClient())
                {
                    client.DownloadFile(address, fileName);
                }
            }

            return File(fileName, "image/jpeg");
        }

        internal static void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Card Images", "images/card-{id}-{side}.jpg", new { controller = "Image", action = "Card" });
        }
    }
}
