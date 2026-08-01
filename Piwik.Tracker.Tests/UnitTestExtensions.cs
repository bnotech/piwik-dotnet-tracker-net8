using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Piwik.Tracker.Tests
{
    internal static class UnitTestExtensions
    {
        public static string GetAuthorityAndPath(this Uri uri)
        {
            return uri?.GetLeftPart(UriPartial.Path) ?? string.Empty;
        }

        public static IEnumerable<KeyValuePair<string, string>> ToKeyValuePairs(this NameValueCollection nameValueCollection)
        {
            return nameValueCollection.AllKeys
                .Where(key => key is not null)
                .Select(key => new KeyValuePair<string, string>(
                    key!,
                    nameValueCollection[key!] ?? string.Empty));
        }
    }
}