using citations365.Controllers;
using HtmlAgilityPack;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Data;

namespace citations365.Models {
    public class ObservableKeyedCollection : KeyedCollection<string, Quote>,
        INotifyCollectionChanged, INotifyPropertyChanged, ISupportIncrementalLoading {

        /* ******
         * EVENTS
         * ******
         */
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        /* *********
         * VARIABLES
         * *********
         */
        /// <summary>
        /// Collection's name
        /// Useful to save it to the app storage
        /// </summary>
        public virtual string Name { get; }

        /// <summary>
        /// Specifies the number of items to load
        /// for the LoadMoreItemsAsync method (not necessary in our scenario)
        /// </summary>
        public virtual uint ItemsToLoad {
            get {
                return 15;
            }
        }

        /// <summary>
        /// If true, last most recent quotes will be saved to Isolated Storage
        /// </summary>
        public virtual bool AllowOffline {
            get {
                return false;
            }
        }

        
        private int _page = 1;
        /// <summary>
        /// Quote's Pagination (as all quotes are not fetched in the same time)
        /// </summary>
        public virtual int Page {
            get {
                return _page;
            }
            set {
                if (_page != value) {
                    _page = value;
                }
            }
        }

        private string _redirectedURL;
        /// <summary>
        /// For some requests, the url is re-written, so we save it to fetch next pages
        /// </summary>
        public virtual string RedirectedURL {
            get {
                return _redirectedURL;
            }
            set {
                if (_redirectedURL != value) {
                    _redirectedURL = value;
                }
            }
        }

        
        private bool _hasMoreItems = false;
        /// <summary>
        /// Tells if more items can be fetched
        /// </summary>
        public virtual bool HasMoreItems {
            get {
                return _hasMoreItems;
            }
            set {
                if (_hasMoreItems != value) {
                    _hasMoreItems = value;
                }
            }
        }

        public ObservableKeyedCollection() {

        }

        /* *******
         * METHODS
         * *******
         */
        /// <summary>
        /// Specifies the key value for the dictionnary
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected override string GetKeyForItem(Quote item) {
            return item.Link;
        }

        /// <summary>
        /// Delete all item from the collection
        /// </summary>
        protected override void ClearItems() {
            base.ClearItems();
            NotifyCollectionChanged(NotifyCollectionChangedAction.Reset);
        }

        /// <summary>
        /// Add an item to the collection with its associated key
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        protected override void InsertItem(int index, Quote item) {
            base.InsertItem(index, item);
            NotifyCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
        }

        /// <summary>
        /// Delete an item from the collection with its associated key
        /// </summary>
        /// <param name="index"></param>
        protected override void RemoveItem(int index) {
            var item = this.Items[index];
            base.RemoveItem(index);
            NotifyCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);
        }

        /// <summary>
        /// Replace the item at the specified index by the new on passed to the method
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        protected override void SetItem(int index, Quote item) {
            base.SetItem(index, item);
            NotifyCollectionChanged(NotifyCollectionChangedAction.Replace, item, index);
        }

        /// <summary>
        /// Notifies when an collection's property has changed
        /// </summary>
        /// <param name="propertyName"></param>
        private void NotifyPropertyChanged(String propertyName) {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Notifies when an action has been performed on the collection
        /// </summary>
        /// <param name="action">The corresponding action</param>
        /// <param name="item">The item involved in the action</param>
        /// <param name="index">The index in the collection where the action took place</param>
        private void NotifyCollectionChanged(NotifyCollectionChangedAction action, Quote item, int index) {
            NotifyCollectionChangedEventHandler handler = CollectionChanged;
            if (null != handler) {
                handler(this, new NotifyCollectionChangedEventArgs(action, item, index));
            }
        }

        /// <summary>
        /// Notifies when an action has been performed on the collection
        /// </summary>
        /// <param name="action">The corresponding action</param>
        private void NotifyCollectionChanged(NotifyCollectionChangedAction action) {
            NotifyCollectionChangedEventHandler handler = CollectionChanged;
            if (null != handler) {
                handler(this, new NotifyCollectionChangedEventArgs(action));
            }
        }

        IAsyncOperation<LoadMoreItemsResult> ISupportIncrementalLoading.LoadMoreItemsAsync(uint count) {
            return LoadMoreItemsAsync(count).AsAsyncOperation();
        }

        /// <summary>
        /// Fires when we reached the end of the ListView
        /// and more items can be fetched
        /// </summary>
        /// <param name="count">Number of items to get</param>
        /// <returns>Number of items fetched</returns>
        public async Task<LoadMoreItemsResult> LoadMoreItemsAsync(uint count) {
            int itemsCount = await BuildAndFetch();
            return new LoadMoreItemsResult { Count = (uint)itemsCount };
        }

        /// <summary>
        /// Build the url and fetch quotes
        /// </summary>
        public virtual async Task<int> BuildAndFetch(string query = "") {
            // URL building
            string url = "";
            return await Fetch(url);
        }

        /// <summary>
        /// Get online data (quotes) from the url
        /// </summary>
        /// <param name="url">URL string to request</param>
        /// <returns>Number of results added to the collection</returns>
        public async Task<int> Fetch(string url) {
            int quotesAdded = 0;
            string responseBodyAsText;

            // If there's no internet connection
            if (!NetworkInterface.GetIsNetworkAvailable()) {
                await handleFailedFetch(); // Load data from IO
                return 0;
            }

            // Fetch the content from a web source
            HttpClient http = new HttpClient();

            try {
                HttpResponseMessage message = await http.GetAsync(url);
                RedirectedURL = message.RequestMessage.RequestUri.ToString();
                responseBodyAsText = await message.Content.ReadAsStringAsync();

                // HTML Document building
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(responseBodyAsText);
                
                // Loop
                var quotes = doc.DocumentNode.Descendants("article");
                foreach (HtmlNode q in quotes) {
                    var content = q.Descendants("div").Where(x => x.GetAttributeValue("class", "") == "figsco__quote__text").FirstOrDefault();
                    var authorAndReference = q.Descendants("div").Where(x => x.GetAttributeValue("class", "") == "figsco__fake__col-9").FirstOrDefault();

                    if (content == null)            continue; // check if this is a valid quote
                    if (authorAndReference == null) continue; // ------------------------------

                    var authorNode = authorAndReference.Descendants("a").FirstOrDefault();
                    string authorName = "De Anonyme";
                    string authorLink = "";

                    if (authorNode != null) { // if the quote as an author
                        authorName = "De " + authorNode.InnerText;
                        authorLink = "http://www.evene.fr" + authorNode.GetAttributeValue("href", "");
                    }
                                        
                    string quoteLink = content.ChildNodes.FirstOrDefault().GetAttributeValue("href", "");

                    string referenceName = "";
                    int separator = authorAndReference.InnerText.LastIndexOf('/');
                    if (separator > -1) {
                        referenceName = authorAndReference.InnerText.Substring(separator + 2);
                    }

                    Quote quote         = new Quote();
                    quote.Content       = content.InnerText;
                    quote.Author        = authorName;
                    quote.AuthorLink    = authorLink;
                    quote.Reference     = referenceName;
                    quote.Link          = quoteLink;

                    quote = Controller.Normalize(quote);

                    if (!Contains(quote.Link)) {
                        Add(quote);
                        quotesAdded++;
                    }
                }

                if (quotesAdded == 0) { // If true, we've reached the end of the search
                    HasMoreItems = false;
                    Page = 0;

                } else {
                    HasMoreItems = true;
                }

                if (AllowOffline && Page == 1) { // save the first quotes to IO
                    SaveIO();
                }

                Page++; // fetch the next quotes' page the next time

                // Test that we've got at least one piece of data
                return quotesAdded;

            } catch (HttpRequestException hre) {
                // The request failed, load quotes from IO
                HasMoreItems = false;
                await handleFailedFetch();
                return Count;
            }
        }

        public bool IsDataLoaded() {
            return Count > 0;
        }

        /// <summary>
        /// Save the collection to the isolated storage
        /// </summary>
        /// <param name="xmlName"></param>
        /// <returns></returns>
        public async Task<bool> SaveIO() {
            if (Count < 1) {
                return true;
            } else {
                try {
                    await DataSerializer<ObservableKeyedCollection>.SaveObjectsAsync(this, Name);
                    return true;
                } catch (IsolatedStorageException exception) {
                    return false; // error
                }
            }
        }

        /// <summary>
        /// Load the collection from the IO
        /// </summary>
        /// <param name="xmlName">Collection's name</param>
        /// <returns></returns>
        public async Task<bool> LoadIO () {
            try {
                ObservableKeyedCollection collection = await DataSerializer<ObservableKeyedCollection>.RestoreObjectsAsync(Name);
                if (collection != null) {
                    foreach (Quote quote in collection) {
                        Add(quote);
                    }
                    return true;
                }
                return false;
            } catch (IsolatedStorageException exception) {
                return false;
            }
        }

        /// <summary>
        /// Fired when the Fetch method fail to get data
        /// </summary>
        /// <returns></returns>
        public virtual async Task<bool> handleFailedFetch() {
            return true;
        }


        /* ***************
         * HELPERS METHODS
         * ***************
         */
        /// <summary>
        /// Delete HTML tags from the quote props and checks values
        /// </summary>
        /// <param name="quote"></param>
        /// <returns></returns>
        public Quote Normalize(Quote quote) {
            // Delete HTML
            quote.Author = DeleteHTMLTags(quote.Author);
            quote.Content = DeleteHTMLTags(quote.Content);
            quote.Reference = DeleteHTMLTags(quote.Reference);

            quote.IsFavorite = FavoritesController.GetFavoriteIcon(quote.Link);

            // Check values
            if (quote.Author.Contains("Vos avis")) {
                quote.Author = "Anonyme";
            }
            return quote;
        }

        /// <summary>
        /// Normalize the text:
        /// Remove the html tags (<h1></h1>, ...), ans special chars (&amp;)
        /// </summary>
        /// <param name="text">Normalize</param>
        public string DeleteHTMLTags(string text) {
            if (text == null) {
                return null;
            }

            text = Regex.Replace(text, @"<(.|\n)*?>", string.Empty);

            text = text
                .Replace("&laquo;", "")
                .Replace("&ldquo;", "")
                .Replace("&rdquo;", "")
                .Replace("&nbsp;", "")
                .Replace("&raquo;", "")
                .Replace("&#039;", "'")
                .Replace("&quot;", "'")
                .Replace("&amp;", "&")
                .Replace("[+]", "")
                .Replace("[", "")
                .Replace("]", "")
                .Replace("\n", "")
                .Replace("  ", "");

            return text;
        }
    }
}
