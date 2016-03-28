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

        /// <summary>
        /// Save a quote object which is in the viewport
        /// </summary>
        public static Quote _lastPosition;

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
            // Initialize the favorites collection
            await FavoritesController.Initialize();

            //if (!IsDataLoaded()) {
            //    return await GetTodayQuotes();
            //}
            //return false;
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

        /// <summary>
        /// Save the ListView position 
        /// to continue where the user left when he comes back on the page
        /// </summary>
        public void SavePosition() {

        }
    }
}
