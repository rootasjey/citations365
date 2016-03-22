using citations365.Models;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
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

        /// <summary>
        /// Describes the collection's type which fired the Add/Remove method 
        /// to update others collections
        /// </summary>
        public enum CollectionType {
            authors,
            favorites,
            search,
            today
        }
        
        /*
         * ************
         * COLLECTIONS
         * ************
         */
        /// <summary>
        /// Today quotes collection
        /// </summary>
        private static ObservableKeyedCollection _favoritesCollection { get; set; }

        public static ObservableKeyedCollection FavoritesCollection {
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
                _favoritesCollection.CollectionChanged += CollectionChanged;
            }
            return true;
        }

        /// <summary>
        /// Load Favorites quotes from IO
        /// </summary>
        /// <returns>True if the data has been loaded</returns>
        public static async Task<ObservableKeyedCollection> LoadFavoritesCollection() {
            try {
                ObservableKeyedCollection collection = await DataSerializer<ObservableKeyedCollection>.RestoreObjectsAsync("FavoritesCollection.xml");
                if (collection != null) {
                    return collection;

                } else {
                    return new ObservableKeyedCollection(); ;
                }
            } catch (IsolatedStorageException exception) {
                return new ObservableKeyedCollection();
            }
        }

        /// <summary>
        /// Save to IO the favortites quotes
        /// </summary>
        /// <returns>True if the save succededreturns>
        public static async Task<bool> SaveFavoritesCollection() {
            try {
                await DataSerializer<ObservableKeyedCollection>.SaveObjectsAsync(FavoritesCollection, "FavoritesCollection.xml");
                return true;
            } catch (IsolatedStorageException exception) {
                return false;
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
                quote.IsFavorite = Quote.FavoriteIcon;      // Update favorite icon
                FavoritesCollection.Add(quote);            // Add quote to favorites collection
                return await SaveFavoritesCollection();     // Save the collection to storage
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
                //quote.IsFavorite = GetFavoriteIcon(quote.Link);
                FavoritesCollection.Remove(quote.Link);
                return await SaveFavoritesCollection();
            }   return false;
        }

        public static async Task<bool> RemoveFavorite(Quote quote, CollectionType collectionType) {
            if (quote == null) {
                return false;
            }

            var key = quote.Link;
            if (FavoritesCollection.Contains(key)) {
                FavoritesCollection.Remove(key);
                SyncAllFavorites(key, collectionType);
                return await SaveFavoritesCollection();
            }
            return false;
        }

        /// <summary>
        /// Notify others collections on add/remove favorite quote
        /// </summary>
        private static void SyncAllFavorites(string key, CollectionType collectionType) {
            switch (collectionType) {
                case CollectionType.authors:
                    TodayController.SyncFavorites(key);
                    SearchController.SyncFavorites(key);
                    break;
                case CollectionType.favorites:
                    TodayController.SyncFavorites(key);
                    SearchController.SyncFavorites(key);
                    break;
                case CollectionType.search:
                    TodayController.SyncFavorites(key);
                    break;
                case CollectionType.today:
                    SearchController.SyncFavorites(key);
                    break;
                default:
                    break;
            }
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

        /// <summary>
        /// Notify Collection changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            
        }

        private static void QuotePropertyChanged(object sender, PropertyChangedEventArgs e) {
        }
    }
}
