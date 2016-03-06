using citations365.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace citations365.Controllers
{
    public class AuthorsController
    {
        /*
         * ***********
         * VARIABLES
         * ***********
         */
        private static ObservableCollection<Author> _authorsCollection { get; set; }

        public static ObservableCollection<Author> AuthorsCollection {
            get {
                if (_authorsCollection == null) {
                    _authorsCollection = new ObservableCollection<Author>();
                }   return _authorsCollection;
            }
        }


        public async Task<bool> LoadData() {
            if (!IsDataLoaded()) {
                return await GetAuthors();
            }   return false;
        }

        /// <summary>
        /// Delete old data and fetch new data
        /// </summary>
        public async Task<bool> Reload() {
            if (IsDataLoaded()) {
                _authorsCollection.Clear();
            }
            return await LoadData();
        }

        /// <summary>
        /// Return true if the data is already loaded
        /// </summary>
        /// <returns>True if data is already loaded</returns>
        public bool IsDataLoaded() {
            return _authorsCollection.Count > 0;
        }

        public async Task<bool> GetAuthors() {
            // Try in IO first

            // Try from the web
        }

        public async Task<bool> LoadAuthors() {

        }

        public async Task<bool> SaveAuthors() {
            if (_authorsCollection.Count < 1) {
                return true;
            } else {
                try {
                    await DataSerializer<ObservableCollection<Quote>>.SaveObjectsAsync(_authorsCollection, "FavoritesCollection.xml");
                    return true;
                } catch (IsolatedStorageException exception) {
                    return false;
                }
            }
        }
    }
}
