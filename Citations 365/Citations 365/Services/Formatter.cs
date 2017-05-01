using citations365.Models;
using System.Text.RegularExpressions;

namespace citations365.Services {
    public class Formatter {
        public static Quote Normalize(Quote quote) {
            quote.Author = DeleteHTMLTags(quote.Author);
            quote.Content = DeleteHTMLTags(quote.Content);
            quote.Reference = DeleteHTMLTags(quote.Reference);

            if (quote.Reference.Contains("Vos avis")) {
                int index = quote.Reference.IndexOf("Vos avis");
                quote.Reference = quote.Reference.Substring(0, index);
            }
            return quote;
        }


        public static string DeleteHTMLTags(string text) {
            if (text == null) {
                return "";
            }

            text = Regex.Replace(text, @"<(.|\n)*?>", string.Empty);

            text = text
                .Replace("&#8230;", "...")
                .Replace("&#8211;", "-")
                .Replace("&#8220;", "\"")
                .Replace("&#8221;", "\"")
                .Replace("&#8217;", "'")
                .Replace("&#8216;", "'")
                .Replace("&laquo;", "")
                .Replace("&ldquo;", "")
                .Replace("&rdquo;", "")
                .Replace("&nbsp;", "")
                .Replace("&raquo;", "")
                .Replace("&#039;", "'")
                .Replace("&quot;", "'")
                .Replace("&amp;", "&")
                .Replace("&#038;", "&")
                .Replace("[+]", "")
                .Replace("[", "")
                .Replace("]", "")
                .Replace("\n", "")
                .Replace("  ", "");

            return text;
        }
    }
}
