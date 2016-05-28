using BackgroundTasks.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundTasks.Controllers
{
    public class AuthorsController
    {
        /*
         * ***********
         * VARIABLES
         * ***********
         */
        /// <summary>
        /// Authors list url
        /// </summary>
        private const string _url = "http://www.evene.fr/citations/dictionnaire-citations-auteurs.php";
        
        /*
         * ************
         * COLLECTIONS
         * ************
         */
        /// <summary>
        /// Private authors collection
        /// </summary>
        private static ObservableCollection<Author> _authorsCollection { get; set; }

        /// <summary>
        /// Authors Collection
        /// </summary>
        public static ObservableCollection<Author> AuthorsCollection {
            get {
                if (_authorsCollection == null) {
                    _authorsCollection = new ObservableCollection<Author>();
                }   return _authorsCollection;
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
        public AuthorsController() {
            //LoadData();
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
            if (!IsDataLoaded()) {
                return await GetAuthors();
            }   return true;
        }

        /// <summary>
        /// Delete old data and fetch new data
        /// </summary>
        public async Task<bool> Reload() {
            if (IsDataLoaded()) {
                AuthorsCollection.Clear();
            }
            return await LoadAuthors();
        }

        /// <summary>
        /// Return true if the data is already loaded
        /// </summary>
        /// <returns>True if data is already loaded</returns>
        public bool IsDataLoaded() {
            return AuthorsCollection.Count > 0;
        }

        public async Task<bool> GetAuthors() {
            // Try in IO first
            bool ioAvailable = await LoadAuthorsIO();

            // Try from the web
            if (ioAvailable) {
                //return await UpdateAuthors();
                return true;
            }

            return await LoadAuthors();
        }

        /// <summary>
        /// Fetch authors from the web
        /// </summary>
        /// <returns>True if the data was successfully retrieved from the web</returns>
        public async Task<bool> LoadAuthors() {
            if (NetworkInterface.GetIsNetworkAvailable()) {
                HttpClient http = new HttpClient();

                try {
                    string responseBodyAsText = await http.GetStringAsync(_url);
                    // Create a html document to parse the data
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(responseBodyAsText);

                    string[] authorsNames = doc.DocumentNode.Descendants("a").Where(x => (string)x.GetAttributeValue("class", "") == "N11 txtC30").Select(y => (string)y.InnerText).ToArray();
                    string[] authorsLinks = doc.DocumentNode.Descendants("a").Where(x => (string)x.GetAttributeValue("class", "") == "N11 txtC30").Select(y => (string)y.GetAttributeValue("href", "")).ToArray();

                    for (int i = 0; i < authorsNames.Length; i++) {
                        Author author = new Author() {
                            Name = authorsNames[i],
                            Link = authorsLinks[i],
                            ImageLink = "ms-appx:///Assets/Icons/gray.png"
                        };
                        AuthorsCollection.Add(author);
                    }

                    SaveAuthors();
                    return true;

                } catch (HttpRequestException hre) {
                    return false;
                }

            } else {
                return false;
            }
        }

        public async Task<bool> UpdateAuthors() {
            foreach (Author author in AuthorsCollection) {
                author.ImageLink = "ms-appx:///Assets/Icons/gray.png";
            }
            return await SaveAuthors();
        }

        /// <summary>
        /// Load authors from IO
        /// </summary>
        /// <returns>True if the data was successfully loaded</returns>
        public async Task<bool> LoadAuthorsIO() {
            try {
                ObservableCollection<Author> collection = await DataSerializer<ObservableCollection<Author>>.RestoreObjectsAsync("AuthorsCollection.xml");
                if (collection != null) {
                    foreach (Author author in collection) {
                        //author.ImageLink = "ms-appx:///Assets/Icons/Contacts_Filled.png";
                        AuthorsCollection.Add(author);
                    }
                    return true;
                }
                return false;
            } catch (IsolatedStorageException exception) {
                return false;
            }
        }

        /// <summary>
        /// Save authors to IO
        /// </summary>
        /// <returns>True if the data was successfully saved</returns>
        public async Task<bool> SaveAuthors() {
            if (AuthorsCollection.Count < 1) {
                return true;
            } else {
                try {
                    await DataSerializer<ObservableCollection<Author>>.SaveObjectsAsync(AuthorsCollection, "AuthorsCollection.xml");
                    return true;
                } catch (IsolatedStorageException exception) {
                    return false;
                }
            }
        }
    }
}
