﻿using System.Web.Mvc;
using Newtonsoft.Json;

namespace CardCatalog
{
    public static class HtmlHelperExtensions
    {
        public static MvcHtmlString Json(this HtmlHelper html, object value)
        {
            return new MvcHtmlString(JsonConvert.SerializeObject(value));
        }
    }
}