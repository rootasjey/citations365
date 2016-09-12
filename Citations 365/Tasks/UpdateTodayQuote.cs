using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tasks.Models;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.UI.Notifications;

namespace Tasks
{
    public sealed class UpdateTodayQuote : IBackgroundTask {
        BackgroundTaskDeferral _deferral;
        volatile bool _cancelRequested = false;
        private string[] _links = {
            "http://evene.lefigaro.fr/citations/citation-jour.php",
            "http://evene.lefigaro.fr/citations",
            "http://evene.lefigaro.fr/citations/citation-jour.php?page="};

        private const string DAILY_QUOTE_CONTENT = "DailyQuoteContent";
        private const string DAILY_QUOTE_AUTHOR = "DailyQuoteAuthor";
        private const string DAILY_LIST_FILENAME = "dailyList.txt";

        public async void Run(IBackgroundTaskInstance taskInstance) {
            _deferral = taskInstance.GetDeferral();

            taskInstance.Canceled += new BackgroundTaskCanceledEventHandler(OnCanceled);

            List<BackgroundQuote> quotesList = null;
            Random random = new Random();

            StorageFile savedQuotesFile = await ApplicationData.Current.LocalFolder.GetFileAsync(DAILY_LIST_FILENAME);
            var timelapse = DateTime.Now.Subtract(savedQuotesFile.DateCreated.DateTime);

            if (timelapse.Hours > 6)
            {
                // Get new fresh data
                var randomLinks = completeLinkAndRandomize(_links, random);
                quotesList = await MultipleFetchs(randomLinks);
                await SaveDailyList(quotesList);
            }
            else
            {
                // Pick a random quote
                quotesList = await RetrieveDailyList(savedQuotesFile);
            }

            if (quotesList != null && quotesList.Count > 0)
            {
                int pick = random.Next(quotesList.Count);
                UpdateTile(quotesList[pick]);
                SaveDailyQuote(quotesList[pick]);                
            }            

            _deferral.Complete();
        }

        private string[] completeLinkAndRandomize(string[] links, Random random)
        {
            int length = links.Length;
            int page = random.Next(333);
            int pos = random.Next(length);

            links[length - 1] = links.LastOrDefault() + page;

            string temp = (string)links.GetValue(pos);
            links[pos] = links.LastOrDefault();

            if (pos != length - 1)
            {
                links[length - 1] = temp;
            }            

            return links;
        }

        public string RetrieveDailyQuote() {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            return (string)localSettings.Values[DAILY_QUOTE_CONTENT];
        }

        public void SaveDailyQuote(BackgroundQuote dailyQuote) {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values[DAILY_QUOTE_CONTENT] = dailyQuote.Content;
            localSettings.Values[DAILY_QUOTE_AUTHOR] = dailyQuote.Author;
        }

        private async Task SaveDailyList(List<BackgroundQuote> quotesList)
        {
            try
            {
                StorageFile savedFile =
                    await ApplicationData.Current.LocalFolder.CreateFileAsync(DAILY_LIST_FILENAME, CreationCollisionOption.ReplaceExisting);

                using (Stream writeStream = await savedFile.OpenStreamForWriteAsync())
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(List<BackgroundQuote>));
                    serializer.WriteObject(writeStream, quotesList);
                    await writeStream.FlushAsync();
                    writeStream.Dispose();
                }

                //return true;
            }
            catch (Exception e)
            {
                //throw;
                //return false;
            }
        }

        private async Task<List<BackgroundQuote>> RetrieveDailyList(StorageFile file)
        {
            try
            {
                List<BackgroundQuote> results = null;

                using (Stream readStream = await file.OpenStreamForReadAsync())
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(List<BackgroundQuote>));
                    results = (List<BackgroundQuote>)serializer.ReadObject(readStream);
                    await readStream.FlushAsync();
                    readStream.Dispose();
                }
                return results;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private async Task<List<BackgroundQuote>> MultipleFetchs(string[] links)
        {
            int attemps = 0;
            List<BackgroundQuote> quotes = null;

            while (quotes?.Count == null && attemps < links.Length)
            {
                quotes = await Fetch(links[attemps]);
                attemps++;
            }

            return quotes;
        }

        /// <summary>
        /// Get online data (quotes) from the url
        /// </summary>
        /// <param name="url">URL string to request</param>
        /// <returns>Number of results added to the collection</returns>
        private async Task<List<BackgroundQuote>> Fetch(string url) {
            string responseBodyAsText;
            List<BackgroundQuote> fetchedQuotes = new List<BackgroundQuote>();

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

                    if (authorNode != null) { // if the quote as an author
                        authorName = authorNode.InnerText;
                        authorLink = "http://www.evene.fr" + authorNode.GetAttributeValue("href", "");
                    }

                    string quoteLink = content.ChildNodes.FirstOrDefault().GetAttributeValue("href", "");

                    string referenceName = "";
                    int separator = authorAndReference.InnerText.LastIndexOf('/');
                    if (separator > -1) {
                        referenceName = authorAndReference.InnerText.Substring(separator + 2);
                    }

                    string quoteContent = DeleteHTMLTags(content.InnerText);
                    authorName = DeleteHTMLTags(authorName);
                    
                    fetchedQuotes.Add(new BackgroundQuote(quoteContent, authorName, authorLink, null, referenceName, quoteLink));
                }

                return fetchedQuotes;

            } catch (HttpRequestException hre) {
                return fetchedQuotes; // the request failed
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

        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason) {
            //
            // Indicate that the background task is canceled.
            //
            _cancelRequested = true;
        }
    }
}
