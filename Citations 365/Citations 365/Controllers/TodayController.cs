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

        private string unsplashURL {
            get {
                return "https://unsplash.it/1500?random";
            }
        }

        private string nasaURL {
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

        /*
         * ***********
         * CONSTRUCTOR
         * ***********
         */
        /// <summary>
        /// Initialize the controller
        /// </summary>
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

            if (IsDataLoaded()) {
                return true;
            }

            int added = await TodayCollection.BuildAndFetch();
            if (added > 0) {
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Delete old data and fetch new data
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

        public async Task<string> GetAppBackgroundURL() {
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
                    backgroundURL = unsplashURL;
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

        private async Task<string> GetNasaImage() {
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = null;
            try {
                response = await httpClient.GetAsync(nasaURL);
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

        private string GetDefaultNasaImage() {
            return "/Assets/Backgrounds/nasa.jpg";
        }
    }
}
