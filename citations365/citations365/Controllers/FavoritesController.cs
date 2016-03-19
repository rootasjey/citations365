using citations365.Models;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;

namespace citations365.Controllers {
    public class FavoritesController {
        /*
         * ***********
         * VARIABLES
         * ***********
         */
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
        private static FavoritesCollection _favoritesCollection { get; set; }

        public static FavoritesCollection FavoritesCollection {
            get {
                return _favoritesCollection;
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
        public FavoritesController() {

        }

        /*
         * ********
         * METHODS
         * ********
         */
        public static async Task<bool> Initialize() {
            if (!IsDataLoaded()) {
                _favoritesCollection = await LoadFavoritesCollection();
            }
            return true;
        }

        /// <summary>
        /// Load Favorites quotes from IO
        /// </summary>
        /// <returns>True if the data has been loaded</returns>
        public static async Task<FavoritesCollection> LoadFavoritesCollection() {
            try {
                FavoritesCollection collection = await DataSerializer<FavoritesCollection>.RestoreObjectsAsync("FavoritesCollection.xml");
                if (collection != null) {
                    return collection;

                } else {
                    return new FavoritesCollection(); ;
                }
            } catch (IsolatedStorageException exception) {
                return new FavoritesCollection();
            }
        }

        /// <summary>
        /// Save to IO the favortites quotes
        /// </summary>
        /// <returns>True if the save succededreturns>
        public static async Task<bool> SaveFavoritesCollection() {
            if (FavoritesCollection.Count < 1) {
                return true;
            } else {
                try {
                    await DataSerializer<FavoritesCollection>.SaveObjectsAsync(FavoritesCollection, "FavoritesCollection.xml");
                    return true;
                } catch (IsolatedStorageException exception) {
                    return false;
                }
            }
        }

        /// <summary>
        /// Delete old data and fetch new data
        /// </summary>
        public static async Task<bool> Reload() {
            if (IsDataLoaded()) {
                _page = 0;
                FavoritesCollection.Clear();
            }
            return await Initialize();
        }

        /// <summary>
        /// Return true if the data is already loaded
        /// </summary>
        /// <returns>True if data is already loaded</returns>
        public static bool IsDataLoaded() {
            return FavoritesCollection != null;
        }

        /// <summary>
        /// Add a quote into favorites in save the list
        /// </summary>
        /// <param name="quote">The quote to be added in favorites</param>
        /// <returns>True if the quote has been correctly added and the list has been saved</returns>
        public static async Task<bool> AddFavorite(Quote quote) {
            if (quote == null) {
                return false;
            }

            if (!_favoritesCollection.Contains(quote.Link)) {
                _favoritesCollection.Add(quote);
                return await SaveFavoritesCollection();
            }   return false;
        }

        /// <summary>
        /// Remove a quote from favorites in save the list
        /// </summary>
        /// <param name="quote">The quote to be removed from favorites</param>
        /// <returns>True if the quote has been correctly removed and the list has been saved</returns>
        public static async Task<bool> RemoveFavorite(Quote quote) {
            if (quote == null) {
                return false;
            }

            if (_favoritesCollection.Contains(quote.Link)) {
                _favoritesCollection.Remove(quote);
                return await SaveFavoritesCollection();
            }   return false;
        }

        /// <summary>
        /// Check if a quote is in favorites
        /// </summary>
        /// <param name="quote">The quote to be tested</param>
        /// <returns>True if he quote is in favorites</returns>
        public static bool IsFavorite(Quote quote) {
            if (quote == null) {
                return false;
            }

            if (_favoritesCollection.Contains(quote.Link)) {
                return true;
            }   return false;
        }

        public static bool IsFavorite(string key) {
            if (key == null) {
                return false;
            }

            if (_favoritesCollection.Contains(key)) {
                return true;
            }   return false;
        }

        /// <summary>
        /// Return a specific Glyph Icon if the quote favorited
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static char GetFavoriteIcon(string key) {
            if (IsFavorite(key)) {
                return Quote.FavoriteIcon;
            }
            return Quote.UnFavoriteIcon;
        }
    }
}
