using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using JWS.Fields.Librray.Classes;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Layouts;
using Sitecore.SecurityModel;

namespace JWS.Fields.Librray.Helpers
{
    public static class SitecoreHelper
    {
        private const string A_ZA_ZS = "[^a-zA-Z-0-9\\s]";
        private const string PATTERN = @"[\s]{2,}";
        private const string SPACE = " ";
        private const string UNDERSCORE = "-";

        public static ID ToSitecoreId(this Guid itemId)
        {
            return new ID(itemId);
        }

        public static Item ToSitecoreItem(this Guid itemId)
        {
            return ToSitecoreItem(new ID(itemId));
        }

        public static Item ToSitecoreItem(this Guid itemId, Database database)
        {
            return database.GetItem(new ID(itemId));
        }

        public static Item ToSitecoreItem(this ID itemId, Database db)
        {
            using (new SecurityDisabler())
            {
                return db.GetItem(itemId);
            }
        }

        public static Item ToSitecoreItem(this ID itemId)
        {
            using (new SecurityDisabler())
            {
                var db = Factory.GetDatabase(Constants.Databases.WEBDB) ??
                         Factory.GetDatabase(Constants.Databases.MASTERDB);
                return db?.GetItem(itemId);
            }
        }

        public static string FixedName(this string name, bool proper = false)
        {
            if (string.IsNullOrEmpty(name)) return name;
            name = name.Trim();
            var cleanName = Regex.Replace(name.Trim(), A_ZA_ZS, SPACE);
            cleanName = Regex.Replace(cleanName, PATTERN, SPACE).ToLower().Trim();
            cleanName = cleanName.Replace(SPACE, UNDERSCORE);
            return (proper ? cleanName.Capitalized() : cleanName);
        }

        public static string Capitalized(this string value)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value);
        }


        public static List<RenderingReference> GetListOfSublayouts(this Item item)
        {
            return item?.Visualization.GetRenderings(Sitecore.Context.Device, true).ToList() ??
                   new List<RenderingReference>();
        }

        public static string GetCanonicalUrl()
        {
            var request = HttpContext.Current.Request;

            var uri = new Uri(request.Url, request.RawUrl);
            var url = uri.AbsolutePath;
            var baseUrl = uri.GetComponents(UriComponents.Scheme | UriComponents.HostAndPort, UriFormat.Unescaped);

            if (Settings.AliasesActive && Sitecore.Context.Database.Aliases.Exists(url))
            {
                var targetItem =
                    Sitecore.Context.Database.GetItem(Sitecore.Context.Database.Aliases.GetTargetID(url));
                return string.Format("{0}{1}", baseUrl, Sitecore.Links.LinkManager.GetItemUrl(targetItem));
            }

            var itemUrl = Sitecore.Links.LinkManager.GetItemUrl(Sitecore.Context.Item);
            return url != itemUrl ? string.Format("{0}{1}", baseUrl, itemUrl) : null;
        }
    }
}
