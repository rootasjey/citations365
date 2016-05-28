using BackgroundTasks.Models;
using HtmlAgilityPack;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.UI.Notifications;

namespace BackgroundTasks {
    public sealed class UpdateTodayQuote : IBackgroundTask {

        BackgroundTaskDeferral _deferral;
        string _url = "http://evene.lefigaro.fr/citations/citation-jour.php";

        public async void Run(IBackgroundTaskInstance taskInstance) {
            _deferral = taskInstance.GetDeferral();

            BackgroundQuote recent = await Fetch(_url);
            UpdateTile(recent);

            _deferral.Complete();
        }

        /// <summary>
        /// Get online data (quotes) from the url
        /// </summary>
        /// <param name="url">URL string to request</param>
        /// <returns>Number of results added to the collection</returns>
        private async Task<BackgroundQuote> Fetch(string url) {
            string responseBodyAsText;

            // If there's no internet connection
            if (!NetworkInterface.GetIsNetworkAvailable()) {
                return new BackgroundQuote();
            }

            // Fetch the content from a web source
            HttpClient http = new HttpClient();

            try {
                HttpResponseMessage message = await http.GetAsync(url);
                responseBodyAsText = await message.Content.ReadAsStringAsync();

                // HTML Document building
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(responseBodyAsText);

                // Loop
                var quotes = doc.DocumentNode.Descendants("article");
                foreach (HtmlNode q in quotes) {
                    var content = q.Descendants("div").Where(x => x.GetAttributeValue("class", "") == "figsco__quote__text").FirstOrDefault();
                    var authorAndReference = q.Descendants("div").Where(x => x.GetAttributeValue("class", "") == "figsco__fake__col-9").FirstOrDefault();

                    if (content == null) continue; // check if this is a valid quote
                    if (authorAndReference == null) continue; // ------------------------------

                    var authorNode = authorAndReference.Descendants("a").FirstOrDefault();
                    string authorName = "De Anonyme";
                    string authorLink = "";

                    if (authorNode != null) { // if the quote as an author
                        authorName = "De " + authorNode.InnerText;
                        authorLink = "http://www.evene.fr" + authorNode.GetAttributeValue("href", "");
                    }

                    string quoteLink = content.ChildNodes.FirstOrDefault().GetAttributeValue("href", "");

                    string referenceName = "";
                    int separator = authorAndReference.InnerText.LastIndexOf('/');
                    if (separator > -1) {
                        referenceName = authorAndReference.InnerText.Substring(separator + 2);
                    }

                    string quote = DeleteHTMLTags(content.InnerText);
                    authorName = DeleteHTMLTags(authorName);

                    //return quote + " - " + authorName;
                    return new BackgroundQuote(quote, authorName, authorLink, null, referenceName, quoteLink);
                }

                return new BackgroundQuote();

            } catch (HttpRequestException hre) {
                // The request failed
                return new BackgroundQuote();
            }
        }

        /// <summary>
        /// Normalize the text:
        /// Remove the html tags (<h1></h1>, ...), ans special chars (&amp;)
        /// </summary>
        /// <param name="text">Normalize</param>
        public string DeleteHTMLTags(string text) {
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

        /// <summary>
        /// Update the application's tile with the most recent quote
        /// </summary>
        /// <param name="quote">The quote's content to update the tile with</param>
        public void UpdateTile(BackgroundQuote quote) {
            XmlDocument tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWideText09);
            XmlNodeList tileTextAttributes = tileXml.GetElementsByTagName("text");

            tileTextAttributes[0].InnerText = quote.Author;
            tileTextAttributes[1].InnerText = quote.Content;

            // Square tile
            XmlDocument squareTileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150Text02);
            XmlNodeList squareTileTextAttributes = squareTileXml.GetElementsByTagName("text");
            //squareTileTextAttributes[0].AppendChild(squareTileXml.CreateTextNode("Hello World! My very own tile notification"));
            squareTileTextAttributes[0].InnerText = quote.Author;
            squareTileTextAttributes[1].InnerText = quote.Content;

            // Integration of the two tile templates
            IXmlNode node = tileXml.ImportNode(squareTileXml.GetElementsByTagName("binding").Item(0), true);
            tileXml.GetElementsByTagName("visual").Item(0).AppendChild(node);

            // Tile Notification
            TileNotification tileNotification = new TileNotification(tileXml);
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
        }
    }
}
