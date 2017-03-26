using citations365.Helpers;
using citations365.Models;
using HtmlAgilityPack;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;

namespace citations365.Controllers {
    public class TodayController
    {
        /*
         * **********
         * VARIABLES
         * **********
         */
        private const string DAILY_QUOTE = "DailyQuote"; // composite value
        private const string DAILY_QUOTE_CONTENT = "DailyQuoteContent";
        private const string DAILY_QUOTE_AUTHOR = "DailyQuoteAuthor";
        private const string DAILY_QUOTE_AUTHOR_LINK = "DailyQuoteAuthorLink";
        private const string DAILY_QUOTE_REFERENCE = "DailyQuoteReference";
        private const string DAILY_QUOTE_LINK = "DailyQuoteLink";

        /// <summary>
        /// Save a quote object which is in the viewport
        /// </summary>
        public static Quote _lastPosition;

        private static bool _backgroundChanged = false;

        public static bool backgroundChanged {
            get {
                return _backgroundChanged;
            }
            set {
                if (value != _backgroundChanged) {
                    _backgroundChanged = value;
                }
            }
        }

        private static string UnsplashURL {
            get {
                return "https://unsplash.it/1500?random";
            }
        }

        private static string NasaURL {
            get {
                return "http://apod.nasa.gov/apod/";
            }
        }

        /*
         * ************
         * COLLECTIONS
         * ************
         */
        /// <summary>
        /// Today quotes collection
        /// </summary>
        private static TodayCollection _todayCollection { get; set; }
        public static TodayCollection TodayCollection {
            get {
                if (_todayCollection==null) {
                    _todayCollection = new TodayCollection();
                }
                return _todayCollection;
            }
        }

        public TodayController() {
            //TodayCollection.CollectionChanged += CollectionChanged;
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
            await FavoritesController.Initialize();

            if (IsDataLoaded()) return true;

            int added = await FetchAndRecover();

            Quote lockscreenQuote = GetLockScreenQuote();
            if (lockscreenQuote != null) {
                TodayCollection.Insert(0, lockscreenQuote);
            }

            if (added > 0) return true;
            return false;
        }

        public async Task<int> FetchAndRecover() {
            int added = 0;

            // Normal fetch
            added = await TodayCollection.BuildAndFetch();
            if (added > 0) return added;

            // If failed, fetch from page 2
            TodayCollection.Page++;
            added = await TodayCollection.BuildAndFetch();
            if (added > 0) return added;

            // If failed, fetch a random category
            added = await TodayCollection.BuildAndFetch("http://evene.lefigaro.fr/citations/mot.php?mot=absurde");
            return added;
        }

        public static Quote GetLockScreenQuote() {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            ApplicationDataCompositeValue composite =
                (ApplicationDataCompositeValue)localSettings.Values[DAILY_QUOTE];

            if (composite != null) {
                Quote quote = new Quote() {
                    Content     = (string)composite[DAILY_QUOTE_CONTENT],
                    Author      = (string)composite[DAILY_QUOTE_AUTHOR],
                    AuthorLink  = (string)composite[DAILY_QUOTE_AUTHOR_LINK],
                    Reference   = (string)composite[DAILY_QUOTE_REFERENCE],
                    Link        = (string)composite[DAILY_QUOTE_LINK]
                };
                return quote;
            }
            return null;
        }

        /// <summary>
        /// Check that the Hero Quote is the most recent
        /// </summary>
        public static void CheckHeroQuote() {
            if (TodayCollection.Count == 0) {
                return;
            }

            Quote lastFetchedQuote = GetLockScreenQuote();
            Quote heroQuote = TodayCollection[0];

            if (lastFetchedQuote == null) {
                return;
            }

            if (lastFetchedQuote.Link == heroQuote.Link) {
                return; // the hero quote is the last fetched quote
            }

            // update the hero quote's value
            // (inserting a new quote at 0 won't set it as hero quote)
            heroQuote.Content       = lastFetchedQuote.Content;
            heroQuote.Author        = lastFetchedQuote.Author;
            heroQuote.AuthorLink    = lastFetchedQuote.AuthorLink;
            heroQuote.Date          = lastFetchedQuote.Date;
            heroQuote.IsFavorite    = lastFetchedQuote.IsFavorite;
            heroQuote.Reference     = lastFetchedQuote.Reference;
        }

        /// <summary>
        /// Delete old data and fetch new ones
        /// </summary>
        public async Task<bool> Reload() {
            if (IsDataLoaded()) {
                TodayCollection.Clear();
            }
            return await LoadData();
        }

        /// <summary>
        /// Return true if the data is already loaded
        /// </summary>
        /// <returns>True if data is already loaded</returns>
        public bool IsDataLoaded() {
            //return TodayCollection.Count > 0;
            return TodayCollection.Count > 0;
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
                quote.IsFavorite = FavoritesController.IsFavorite(key);
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

        /// <summary>
        /// Save the ListView position 
        /// to continue where the user left when he comes back on the page
        /// </summary>
        public void SavePosition() {

        }

        public static async Task<string> GetAppBackgroundURL() {
            if (!backgroundChanged) {
                return SettingsController.GetAppBackgroundURL();
            }

            string background = SettingsController.GetAppBackground();
            string backgroundURL = "";
            backgroundChanged = false;

            switch (background) {
                case "nasa":
                    backgroundURL = await GetNasaImage();
                    break;
                case "unsplash":
                    backgroundURL = UnsplashURL;
                    break;
                default:
                    backgroundURL = "";
                    break;
            }

            string name = SettingsController.GenerateAppBackgroundName();

            StorageFile wallpaper = await ImageHelper.SaveLockscreenImage(name, backgroundURL);
            SettingsController.UpdateAppBackgroundURL(wallpaper.Path);
            SettingsController.UpdateAppBackgroundName(name);
            SettingsController.UpdateAppBackgroundType(background);
            
            return wallpaper.Path;
        }

        private static async Task<string> GetNasaImage() {
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = null;
            try {
                response = await httpClient.GetAsync(NasaURL);
                response.EnsureSuccessStatusCode();
                string responseBodyAsText = await response.Content.ReadAsStringAsync();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(responseBodyAsText);

                string start = "<a";
                string end = ".jpg";

                Regex regex = new Regex(start + "(.*?)" + end);
                MatchCollection matches = regex.Matches(responseBodyAsText);

                if (matches.Count > 0) {
                    return "http://apod.nasa.gov/apod/" + matches[0].ToString().Substring(9);
                }

                return GetDefaultNasaImage();
            } catch {
                return GetDefaultNasaImage();
            }
        }

        private static string GetDefaultNasaImage() {
            return "/Assets/Backgrounds/nasa.jpg";
        }
    }
}
