using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Web.Infrastructure;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using StackExchange.Profiling;
using StackExchange.Profiling.MVCHelpers;

[assembly: WebActivator.PreApplicationStartMethod(
    typeof(CardCatalog.App_Start.MiniProfilerPackage), "PreStart")]

[assembly: WebActivator.PostApplicationStartMethod(
    typeof(CardCatalog.App_Start.MiniProfilerPackage), "PostStart")]

namespace CardCatalog.App_Start
{
    public static class MiniProfilerPackage
    {
        public static void PreStart()
        {
            DynamicModuleUtility.RegisterModule(typeof(MiniProfilerStartupModule));

            GlobalFilters.Filters.Add(new ProfilingActionFilter());
        }

        public static void PostStart()
        {
            var copy = ViewEngines.Engines.ToList();
            ViewEngines.Engines.Clear();
            foreach (var item in copy)
            {
                ViewEngines.Engines.Add(new ProfilingViewEngine(item));
            }
        }
    }

    public class MiniProfilerStartupModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.BeginRequest += (sender, e) =>
            {
                var request = ((HttpApplication)sender).Request;
                if (request.IsLocal)
                {
                    MiniProfiler.Start();
                }
            };

            context.EndRequest += (sender, e) =>
            {
                MiniProfiler.Stop();
            };
        }

        public void Dispose()
        {
        }
    }
}