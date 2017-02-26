using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tasks.Models;
using Windows.Foundation;

namespace Tasks.Factory {
    public sealed class Data {
        private static string[] _links = {
            "http://evene.lefigaro.fr/citations/citation-jour.php?page=",
            "http://evene.lefigaro.fr/citations/citation-jour.php",
            "http://evene.lefigaro.fr/citations/theme/proverbes-francais-france.php"
        };

        public static IAsyncOperation<IList<Quote>> FetchNewQuotesAsync() {
            return FetchNewQuotes().AsAsyncOperation();
        }

        private static async Task<IList<Quote>> FetchNewQuotes() {
            var randomLinks = PickRandomLink(_links);
            return await FetchAndRetry(randomLinks);
        }

        private static string[] PickRandomLink(string[] links) {
            Random random = new Random();
            int page = random.Next(333);
            links[0] = links.First() + page;
            return links;
        }

        private static async Task<IList<Quote>> FetchAndRetry(string[] links) {
            int attemps = 0;
            bool isEmpty = true;
            IList<Quote> quotes = null;

            while (isEmpty && attemps < links.Length) {
                quotes = await Fetch(links[attemps]);
                attemps++;
                isEmpty = quotes.Count == 0;
            }

            return quotes;
        }

        /// <summary>
        /// Get online data (quotes) from the url
        /// </summary>
        /// <param name="url">URL string to request</param>
        /// <returns>Number of results added to the collection</returns>
        private static async Task<IList<Quote>> Fetch(string url) {
            string responseBodyAsText;
            List<Quote> fetchedQuotes = new List<Quote>();

            if (!NetworkInterface.GetIsNetworkAvailable()) {
                return fetchedQuotes;
            }

            HttpClient http = new HttpClient();

            try {
                HttpResponseMessage message = await http.GetAsync(url);
                responseBodyAsText = await message.Content.ReadAsStringAsync();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(responseBodyAsText);

                var quotes = doc.DocumentNode.Descendants("article");

                foreach (HtmlNode q in quotes) {
                    var content = q.Descendants("div").Where(x => x.GetAttributeValue("class", "") == "figsco__quote__text").FirstOrDefault();
                    var authorAndReference = q.Descendants("div").Where(x => x.GetAttributeValue("class", "") == "figsco__fake__col-9").FirstOrDefault();

                    if (content == null) continue; // check if this is a valid quote
                    if (authorAndReference == null) continue;

                    var authorNode = authorAndReference.Descendants("a").FirstOrDefault();

                    string authorName = "Anonyme";
                    string authorLink = "";
                    string quoteLink = content.ChildNodes.FirstOrDefault().GetAttributeValue("href", "");
                    string quoteContent = DeleteHTMLTags(content.InnerText);
                    string referenceName = "";

                    if (authorNode != null) { // if the quote as an author
                        authorName = authorNode.InnerText.Contains("Vos avis") ? authorName : authorNode.InnerText;
                        authorLink = "http://www.evene.fr" + authorNode.GetAttributeValue("href", "");
                    }

                    int separator = authorAndReference.InnerText.IndexOf('/');
                    if (separator > -1) {
                        referenceName = authorAndReference.InnerText.Substring(separator + 2);
                    }

                    authorName = DeleteHTMLTags(authorName);

                    fetchedQuotes.Add(Normalize(
                        new Quote() {
                            Content     = quoteContent,
                            Author      = authorName,
                            AuthorLink  = authorLink,
                            Date        = null,
                            Reference   = referenceName,
                            Link        = quoteLink
                        }
                    ));
                }

                return fetchedQuotes;

            } catch (HttpRequestException hre) {
                return fetchedQuotes; // the request failed
            }
        }

        /// <summary>
        /// Delete HTML tags from the quote props and checks values
        /// </summary>
        /// <param name="quote"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Normalize the text:
        /// Remove the html tags (<h1></h1>, ...), ans special chars (&amp;)
        /// </summary>
        /// <param name="text">Normalize</param>
        public static string DeleteHTMLTags(string text) {
            if (text == null) {
                return null;
            }

            text = Regex.Replace(text, @"<(.|\n)*?>", string.Empty);

            text = text
                .Replace("&laquo;", "")
                .Replace("&ldquo;", "")
                .Replace("&rdquo;", "")
                .Replace("&nbsp;", "")
                .Replace("&raquo;", "")
                .Replace("&#039;", "'")
                .Replace("&quot;", "'")
                .Replace("&amp;", "&")
                .Replace("[+]", "")
                .Replace("[", "")
                .Replace("]", "")
                .Replace("\n", "")
                .Replace("  ", "");

            return text;
        }

    }
}
