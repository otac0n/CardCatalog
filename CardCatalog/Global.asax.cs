using System;
using System.Threading;
using System.Web.Mvc;
using System.Web.Routing;
using CardCatalog.Controllers;
using Raven.Client.Document;
using Raven.Client.Indexes;

namespace CardCatalog
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode,
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static DocumentStore DocumentStore { get; private set; }

        private static CancellationTokenSource backgroundCancellation;

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            ImageController.RegisterRoutes(routes);

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            DocumentStore = new DocumentStore
            {
                ConnectionStringName = "RavenDB",
            };
            DocumentStore.Initialize();
            IndexCreation.CreateIndexes(typeof(MvcApplication).Assembly, DocumentStore);
            MvcMiniProfiler.RavenDb.Profiler.AttachTo(DocumentStore);

            backgroundCancellation = RepeatingBackgroundTask.Start(TimeSpan.FromSeconds(10), new CardUtilities.BackgroundScraper().ScrapeSingle);
        }

        protected void Application_Stop()
        {
            backgroundCancellation.Cancel();
        }
    }
}