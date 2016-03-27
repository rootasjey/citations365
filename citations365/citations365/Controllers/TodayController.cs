using citations365.Models;
using HtmlAgilityPack;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace citations365.Controllers {
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
        private static int _page = 1;

        /*
         * ************
         * COLLECTIONS
         * ************
         */
        /// <summary>
        /// Today quotes collection
        /// </summary>
        private static ObservableKeyedCollection _todayCollection { get; set; }

        public static ObservableKeyedCollection TodayCollection {
            get {
                if (_todayCollection == null) {
                    _todayCollection = new ObservableKeyedCollection();
                }
                return _todayCollection;
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
        public TodayController() {
            TodayCollection.CollectionChanged += CollectionChanged;
        }

        /*
         * ********
         * METHODS
         * ********
         */
        /// <summary>
        /// Load today quotes and the favorites collection
        /// </summary>
        /// <returns>True if the data has been loaded</returns>
        public async Task<bool> LoadData() {
            // Initialize the favorites collection
            await FavoritesController.Initialize();

            if (!IsDataLoaded()) {
                return await GetTodayQuotes();
            }
            return false;
        }

        /// <summary>
        /// Fetch the web link and extract content
        /// </summary>
        public async Task<bool> GetTodayQuotes() {
            string author = "";
            string reference = "";

            // If there's no internet connection
            if (!NetworkInterface.GetIsNetworkAvailable()) {
                await LoadToday(); // Load data from IO
                return IsDataLoaded();
            }

            // URL Building
            if (_page < 2) {
                _url = _url.Substring(0, (_url.Length - 6));
            } else {
                _url = _url + _page;
            }

            // Fetch the content from a web source
            HttpClient httpClient = new HttpClient();

            try {
                string responseBodyAsText = await httpClient.GetStringAsync(_url);

                // Create a html document to parse the data
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(responseBodyAsText);

                // Regex Definitions
                Regex content_regex     = new Regex("<div class=\"figsco__quote__text\">" + "((.|\n)*?)" + "</a></div>");
                Regex author_regex      = new Regex("<div class=\"figsco__fake__col-9\">" + "((.|\n)*?)" + "<br>");
                Regex authorLink_regex  = new Regex("/celebre/biographie/" + "((.|\n)*?)" + ".php");
                Regex quoteLink_regex   = new Regex("/citation/" + "((.|\n)*?)" + ".php");

                // Loop
                string[] quotesArray = doc.DocumentNode.Descendants("article").Select(y => y.InnerHtml).ToArray();
                foreach (string q in quotesArray) {
                    MatchCollection content_match       = content_regex.Matches(q);
                    MatchCollection author_match        = author_regex.Matches(q);
                    MatchCollection authorLink_match    = authorLink_regex.Matches(q);
                    MatchCollection quoteLink_match     = quoteLink_regex.Matches(q);

                    Quote quote = new Quote();
                    quote.Content = content_match.Count > 0 ? content_match[0].ToString() : null;

                    if (quote.Content == null) continue;

                    // REFERENCE TEST (Test if there's a reference)
                    string authorAndRef = author_match.Count > 0 ? author_match[0].ToString() : null;
                    if (authorAndRef == null) continue; // check 2 (a quote must have an author || Anonyme)

                    int separator = authorAndRef.LastIndexOf('/');

                    if (separator < 0) {
                        author = authorAndRef;
                    } else {
                        if (authorAndRef.Substring(separator - 1).StartsWith("</a>")) {
                            separator -= 1;
                        }

                        author = authorAndRef.Substring(0, separator);
                        reference = authorAndRef.Substring(separator + 2);
                        if (reference.StartsWith("a>")) reference = ""; // cans get </a>, so empty the var
                    }

                    quote.Author = author;
                    quote.AuthorLink = authorLink_match.Count > 0 ? "http://www.evene.fr" + authorLink_match[0].ToString() : null;
                    quote.Reference = reference;
                    quote.Link = quoteLink_match.Count > 0 ? quoteLink_match[0].ToString() : null;

                    quote = Controller.Normalize(quote);

                    TodayCollection.Add(quote);
                }

                if (_page == 0) { // save the first quotes to IO
                    SaveToday();
                }

                _page++; // fetch the next quotes' page the next time

                // Test that we've got at least one piece of data
                return IsDataLoaded();

            } catch (HttpRequestException hre) {
                // The request failed, load quotes from IO
                await LoadToday();
                return IsDataLoaded();
            }            
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

        /// <summary>
        /// Save to IO the first quotes from the todayCollection
        /// </summary>
        /// <returns>True if the save succeded</returns>
        public static async Task<bool> SaveToday() {
            if (TodayCollection.Count < 1) {
                return true;
            } else {
                try {
                    await DataSerializer<ObservableKeyedCollection>.SaveObjectsAsync(TodayCollection, "TodayCollection.xml");
                    return true;
                } catch (IsolatedStorageException exception) {
                    return false; // error
                }
            }
        }

        /// <summary>
        /// Load from IO the quotes saved before
        /// </summary>
        /// <returns>True if the retrieve succeded</returns>
        public static async Task<bool> LoadToday() {
            try {
                ObservableKeyedCollection collection = await DataSerializer<ObservableKeyedCollection>.RestoreObjectsAsync("TodayCollection.xml");
                if (collection != null) {
                    _todayCollection = collection;
                    return true;
                }
                return false;
            } catch (IsolatedStorageException exception) {
                return false;
            }
        }

        /// <summary>
        /// Initialize the favorite quotes collection from the FavoritesController
        /// </summary>
        /// <returns></returns>
        public async Task<bool> InitalizeFavorites() {
            return await FavoritesController.Initialize();
        }

        /// <summary>
        /// Update the favorite icon of a quote
        /// </summary>
        /// <param name="key"></param>
        public static void SyncFavorites(string key) {
            if (TodayCollection.Contains(key)) {
                Quote quote = TodayCollection[key];
                quote.IsFavorite = FavoritesController.GetFavoriteIcon(key);
            }
        }
        
        /// <summary>
        /// Notify Collection changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (e.NewItems != null)
                foreach (Quote item in e.NewItems)
                    item.PropertyChanged += QuotePropertyChanged;

            if (e.OldItems != null)
                foreach (Quote item in e.OldItems)
                    item.PropertyChanged -= QuotePropertyChanged;
        }

        private void QuotePropertyChanged(object sender, PropertyChangedEventArgs e) {
            //if (e.PropertyName == "Content") {
            //}
        }
    }
}
