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
        /// <summary>
        /// URL to perform the search
        /// </summary>
        private string _url = "http://evene.lefigaro.fr/citations/mot.php?mot=";

        /// <summary>
        /// Pagination of the search result
        /// </summary>
        private static int _page = 1;

        /// <summary>
        /// True if we have reached the end of the pagination of results
        /// </summary>
        private bool _reachedEnd = false;

        /// <summary>
        /// Save search's keywords to check the next function call (getQuotes)
        /// If _query is equal to the function's parameter, increment the _page variable
        /// If _query has a new value, start a new search at the fist _page = 1
        /// </summary>
        private string _query;

        /*
         * ************
         * COLLECTIONS
         * ************
         */
        /// <summary>
        /// Private authors collection
        /// </summary>
        private static ObservableKeyedCollection _searchCollection { get; set; }

        /// <summary>
        /// Authors Collection
        /// </summary>
        public static ObservableKeyedCollection SearchCollection {
            get {
                if (_searchCollection == null) {
                    _searchCollection = new ObservableKeyedCollection();
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
                SearchCollection.Clear();
            }
            return await LoadData();
        }

        public async Task<bool> GetQuotes(string query) {
            // Checks if it's a new search
            if (query != _query) {
                _page = 1;
                _reachedEnd = false;
            }

            if (_reachedEnd) {
                return false;
            }
            if (!NetworkInterface.GetIsNetworkAvailable()) {
                return false;
            }

            // We must watch if we've reached the end of the search.
            // If there's 0 quote added in the function call, we're at the end.
            int quotesAdded = 0;            

            // Save the last query (if it's not an empty string)
            _query = query.Length > 0 ? query : _query;

            // URL building
            _url += query + "&page=" + _page;
            
            HttpClient http = new HttpClient();

            try {
                string responseBodyAsText = await http.GetStringAsync(_url);

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

                    if (quote.Content == null) continue; // check 1 (anything but a quote)

                    quote.Author        = author_match.Count > 0 ? author_match[0].ToString() : null;
                    quote.AuthorLink    = authorLink_match.Count > 0 ? "http://www.evene.fr" + authorLink_match[0].ToString() : null;
                    quote.Reference     = reference_match.Count > 0 ? reference_match[0].ToString() : null;
                    quote.Link          = quoteLink_match.Count > 0 ? quoteLink_match[0].ToString() : null;

                    if (quote.Author == null) continue; // check 2 (section, no quote)
                    quote = Controller.Normalize(quote);

                    SearchCollection.Add(quote);
                    quotesAdded++;
                }

                if (quotesAdded == 0) {
                    // If we're here, we've reached the end of the search
                    _reachedEnd = true;
                    _page = 0;
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
            return SearchCollection.Count > 0;
        }

        /// <summary>
        /// Update the favorite icon of a quote
        /// </summary>
        /// <param name="key"></param>
        public static void SyncFavorites(string key) {
            if (SearchCollection.Contains(key)) {
                Quote quote = SearchCollection[key];
                quote.IsFavorite = FavoritesController.GetFavoriteIcon(key);
            }
        }
    }
}
