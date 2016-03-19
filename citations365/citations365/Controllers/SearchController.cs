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
    public class SearchController
    {
        /*
         * ***********
         * VARIABLES
         * ***********
         */
        private string _url = "http://evene.lefigaro.fr/citations/mot.php?mot=amour&page=";

        private static int _page = 1;
        /*
         * ************
         * COLLECTIONS
         * ************
         */
        /// <summary>
        /// Private authors collection
        /// </summary>
        private static ObservableCollection<Quote> _searchCollection { get; set; }

        /// <summary>
        /// Authors Collection
        /// </summary>
        public static ObservableCollection<Quote> SearchCollection {
            get {
                if (_searchCollection == null) {
                    _searchCollection = new ObservableCollection<Quote>();
                }
                return _searchCollection;
            }
        }

        /*
         * ***********
         * CONSTRUCTOR
         * ***********
         */
        /// <summary>
        /// Initialize the controller
        /// </summary>
        public SearchController() {

        }

        /*
         * ********
         * METHODS
         * ********
         */
        /// <summary>
        /// Populate authors collection
        /// </summary>
        /// <returns>True if data was successfully loaded</returns>
        public async Task<bool> LoadData() {
            return IsDataLoaded();
        }

        /// <summary>
        /// Delete old data and fetch new data
        /// </summary>
        public async Task<bool> Reload() {
            if (IsDataLoaded()) {
                _searchCollection.Clear();
            }
            return await LoadData();
        }

        public async Task<bool> GetQuotes(string query) {
            if (query == null) {
                return false;
            }
            if (!NetworkInterface.GetIsNetworkAvailable()) {
                return false;
            }

            HttpClient http = new HttpClient();

            try {
                string responseBodyAsText = await http.GetStringAsync(_url + _page);

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(responseBodyAsText);

                // Regex Definitions
                Regex content_regex     = new Regex("<div class=\"figsco__quote__text\">" + "((.|\n)*?)" + "</a></div>");
                Regex author_regex      = new Regex("<div class=\"figsco__fake__col-9\">" + "((.|\n)*?)" + "</a>");
                Regex authorLink_regex  = new Regex("/celebre/biographie/" + "((.|\n)*?)" + ".php");
                Regex reference_regex   = new Regex("</a>" + "((.|\n)*?)" + "/" + "((.|\n)*?)" + "</br>");
                Regex quoteLink_regex   = new Regex("/citation/" + "((.|\n)*?)" + ".php");

                // Loop
                string[] quotesArray = doc.DocumentNode.Descendants("article").Select(y => y.InnerHtml).ToArray();
                foreach (string q in quotesArray) {
                    MatchCollection content_match       = content_regex.Matches(q);
                    MatchCollection author_match        = author_regex.Matches(q);
                    MatchCollection authorLink_match    = authorLink_regex.Matches(q);
                    MatchCollection reference_match     = reference_regex.Matches(q);
                    MatchCollection quoteLink_match     = quoteLink_regex.Matches(q);

                    Quote quote = new Quote();
                    quote.Content = content_match.Count > 0 ? content_match[0].ToString() : null;

                    if (quote.Content == null) continue;

                    quote.Author        = author_match.Count > 0 ? author_match[0].ToString() : null;
                    quote.AuthorLink    = authorLink_match.Count > 0 ? "http://www.evene.fr" + authorLink_match[0].ToString() : null;
                    quote.Reference     = reference_match.Count > 0 ? reference_match[0].ToString() : null;
                    quote.Link          = quoteLink_match.Count > 0 ? quoteLink_match[0].ToString() : null;

                    quote = Controller.Normalize(quote);

                    SearchCollection.Add(quote);
                }

                _page++; // fetch the next quotes' page the next time
                return IsDataLoaded();

            } catch (HttpRequestException hre) {
                return false;
            }
        }

        /// <summary>
        /// Return true if the data is already loaded
        /// </summary>
        /// <returns>True if data is already loaded</returns>
        public bool IsDataLoaded() {
            return _searchCollection.Count > 0;
        }
    }
}
