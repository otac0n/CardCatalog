using System.Text.RegularExpressions;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace CardCatalog
{
    public static class HtmlHelperExtensions
    {
        public static MvcHtmlString Json(this HtmlHelper html, object value)
        {
            return new MvcHtmlString(JsonConvert.SerializeObject(value));
        }

        public static MvcHtmlString CardText(this HtmlHelper html, string text)
        {
            var markup = html.Encode(text);

            markup = Regex.Replace(markup, @"\([^()]+\)", "<em>$&</em>");

            markup = Regex.Replace(
                markup,
                @"{(\w+)}",
                m => "<img src=\"/Images/Icons/" + m.Groups[1].Value.ToLower() + ".jpg\" alt=\"" + m.Value + "\" />");

            return new MvcHtmlString(markup);
        }
    }
}