using citations365.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace citations365.Controllers
{
    public class TodayController
    {
        /*
         * **********
         * VARIABLES
         * **********
         */
        /// <summary>
        /// Url which will be used to fetch today quotes
        /// </summary>
        private string _url = "http://evene.lefigaro.fr/citations/citation-jour.php?page=";

        /// <summary>
        /// Quote's Pagination (as all quotes are not fetched in the same time)
        /// </summary>
        private static int _page = 0;
        
        /*
         * ************
         * COLLECTIONS
         * ************
         */
        /// <summary>
        /// Today quotes collection
        /// </summary>
        private static ObservableCollection<Quote> _todayCollection { get; set; }

        public static ObservableCollection<Quote> TodayCollection {
            get {
                if (_todayCollection == null) {
                    _todayCollection = new ObservableCollection<Quote>();
                }
                return _todayCollection;
            }
        }

        /// <summary>
        /// Initialize the controller here
        /// </summary>
        public TodayController() {
            
        }

        /*
         * ********
         * METHODS
         * ********
         */
        public async Task<bool> LoadData() {
            if (!IsDataLoaded()) {
                return await GetTodayQuotes();
            }
            return false;
        }

        /// <summary>
        /// Fetch the web link and extract content
        /// </summary>
        public async Task<bool> GetTodayQuotes() {
            // If there's no internet connection
            if (!NetworkInterface.GetIsNetworkAvailable()) {
                // Load data from IO
                await Controller.LoadToday();
                return IsDataLoaded();
            }

            // Fetch the content from a web source
            HttpClient httpClient = new HttpClient();

            try {
                string responseBodyAsText = await httpClient.GetStringAsync(_url + _page);

                // Create a html document to parse the data
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(responseBodyAsText);

                // Regex Definitions
                Regex content_regex = new Regex("<div class=\"figsco__quote__text\">" + "((.|\n)*?)" + "</a></div>");
                Regex author_regex = new Regex("<div class=\"figsco__fake__col-9\">" + "((.|\n)*?)" + "</a>");
                Regex authorLink_regex = new Regex("/celebre/biographie/" + "((.|\n)*?)" + ".php");
                Regex reference_regex = new Regex("</a>" + "((.|\n)*?)" + "/" + "((.|\n)*?)" + "</br>");
                Regex quoteLink_regex = new Regex("/citation/" + "((.|\n)*?)" + ".php");

                // Loop
                string[] quotesArray = doc.DocumentNode.Descendants("article").Select(y => y.InnerHtml).ToArray();
                foreach (string q in quotesArray) {
                    MatchCollection content_match = content_regex.Matches(q);
                    MatchCollection author_match = author_regex.Matches(q);
                    MatchCollection authorLink_match = authorLink_regex.Matches(q);
                    MatchCollection reference_match = reference_regex.Matches(q);
                    MatchCollection quoteLink_match = quoteLink_regex.Matches(q);

                    Quote quote = new Quote();
                    quote.content = content_match.Count > 0 ? content_match[0].ToString() : null;

                    if (quote.content == null) continue;

                    quote.author = author_match.Count > 0 ? author_match[0].ToString() : null;
                    quote.authorLink = authorLink_match.Count > 0 ? "http://www.evene.fr" + authorLink_match[0].ToString() : null;
                    quote.reference = reference_match.Count > 0 ? reference_match[0].ToString() : null;
                    quote.link = quoteLink_match.Count > 0 ? quoteLink_match[0].ToString() : null;

                    quote = Normalize(quote);

                    TodayCollection.Add(quote);
                }

                if (_page == 0) { // save the first quotes to IO
                    Controller.SaveToday();
                }
                // Test that we've got at least one piece of data
                return IsDataLoaded();

            } catch (HttpRequestException hre) {
                // The request failed, load quotes from IO
                await Controller.LoadToday();
                return IsDataLoaded();
            }            
        }

        /// <summary>
        /// Delete HTML tags from the quote props and checks values
        /// </summary>
        /// <param name="quote"></param>
        /// <returns></returns>
        private Quote Normalize(Quote quote) {
            // Delete HTML
            quote.author = Controller.DeleteHTMLTags(quote.author);
            quote.content = Controller.DeleteHTMLTags(quote.content);
            quote.reference = Controller.DeleteHTMLTags(quote.reference);

            // Check values
            if (quote.author.Contains("Vos avis")) {
                quote.author = "Anonyme";
            }
            return quote;
        }

        /// <summary>
        /// Delete old data and fetch new data
        /// </summary>
        public async Task<bool> Reload() {
            if (IsDataLoaded()) {
                _page = 0;
                TodayCollection.Clear();
            }
            return await LoadData();
        }

        /// <summary>
        /// Return true if the data is already loaded
        /// </summary>
        /// <returns>True if data is already loaded</returns>
        public bool IsDataLoaded() {
            return TodayCollection.Count > 0;
        }
    }
}
