using citations365.Models;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;

namespace citations365.Controllers {
    public class FavoritesController {

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
        private static ObservableCollection<Quote> _favoritesCollection { get; set; }

        public static ObservableCollection<Quote> FavoritesCollection {
            get {
                if (_favoritesCollection == null) {
                    _favoritesCollection = new ObservableCollection<Quote>();
                }
                return _favoritesCollection;
            }
        }

        /// <summary>
        /// Initialize the controller here
        /// </summary>
        public FavoritesController() {

        }

        /*
         * ********
         * METHODS
         * ********
         */
        public async Task<bool> LoadData() {
            if (!IsDataLoaded()) {
                return await LoadFavoritesQuotes();
            }   return false;
        }

        /// <summary>
        /// Load Favorites quotes from IO
        /// </summary>
        /// <returns>True if the data has been loaded</returns>
        public async Task<bool> LoadFavoritesQuotes() {
            try {
                ObservableCollection<Quote> collection = await DataSerializer<ObservableCollection<Quote>>.RestoreObjectsAsync("FavoritesCollection.xml");
                if (collection != null) {
                    _favoritesCollection = collection;
                    return true;
                }   return false;
            } catch (IsolatedStorageException exception) {
                return false;
            }
        }

        /// <summary>
        /// Save to IO the favortites quotes
        /// </summary>
        /// <returns>True if the save succededreturns>
        public static async Task<bool> SaveFavorites() {
            if (FavoritesCollection.Count < 1) {
                return true;
            } else {
                try {
                    await DataSerializer<ObservableCollection<Quote>>.SaveObjectsAsync(FavoritesCollection, "FavoritesCollection.xml");
                    return true;
                } catch (IsolatedStorageException exception) {
                    return false;
                }
            }
        }

        /// <summary>
        /// Delete old data and fetch new data
        /// </summary>
        public async Task<bool> Reload() {
            if (IsDataLoaded()) {
                _page = 0;
                FavoritesCollection.Clear();
            }
            return await LoadData();
        }

        /// <summary>
        /// Return true if the data is already loaded
        /// </summary>
        /// <returns>True if data is already loaded</returns>
        public bool IsDataLoaded() {
            return FavoritesCollection.Count > 0;
        }

        /// <summary>
        /// Add a quote into favorites in save the list
        /// </summary>
        /// <param name="quote">The quote to be added in favorites</param>
        /// <returns>True if the quote has been correctly added and the list has been saved</returns>
        public async Task<bool> AddFavorite(Quote quote) {
            if (quote == null) {
                return false;
            }

            if (!_favoritesCollection.Contains(quote)) {
                _favoritesCollection.Add(quote);
                return await SaveFavorites();
            }   return false;
        }

        /// <summary>
        /// Remove a quote from favorites in save the list
        /// </summary>
        /// <param name="quote">The quote to be removed from favorites</param>
        /// <returns>True if the quote has been correctly removed and the list has been saved</returns>
        public async Task<bool> RemoveFavorite(Quote quote) {
            if (quote == null) {
                return false;
            }

            if (_favoritesCollection.Contains(quote)) {
                _favoritesCollection.Remove(quote);
                return await SaveFavorites();
            }   return false;
        }

        /// <summary>
        /// Check if a quote is in favorites
        /// </summary>
        /// <param name="quote">The quote to be tested</param>
        /// <returns>True if he quote is in favorites</returns>
        public bool IsFavorite(Quote quote) {
            if (quote == null) {
                return false;
            }

            if (_favoritesCollection.Contains(quote)) {
                return true;
            }   return false;
        }
    }
}
