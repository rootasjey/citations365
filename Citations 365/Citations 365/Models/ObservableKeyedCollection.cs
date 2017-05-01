using citations365.Services;
using HtmlAgilityPack;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Data;

namespace citations365.Models {
    public class ObservableKeyedCollection : KeyedCollection<string, Quote>,
        INotifyCollectionChanged, INotifyPropertyChanged, ISupportIncrementalLoading {

        #region variables
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;
        
        public virtual string Name { get; }
        
        public virtual uint ItemsToLoad {
            get {
                return 15;
            }
        }

        public virtual bool AllowOffline {
            get {
                return true;
            }
        }

        private string _Query;

        public virtual string Query {
            get { return _Query; }
            set { _Query = value; }
        }

        private int _page = 1; // TODO: Set back to 1
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

        private string _URL;

        public string URL {
            get { return _URL; }
            set { _URL = value; }
        }


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

        public ObservableKeyedCollection Favorites { get; set; }

        #endregion variables

        #region key operations
        protected override string GetKeyForItem(Quote item) {
            return item.Link;
        }
        
        protected override void ClearItems() {
            base.ClearItems();
            NotifyCollectionChanged(NotifyCollectionChangedAction.Reset);
        }

        protected override void InsertItem(int index, Quote item) {
            base.InsertItem(index, item);
            NotifyCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
        }
        
        protected override void RemoveItem(int index) {
            var item = Items[index];
            base.RemoveItem(index);
            NotifyCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);
        }
        
        protected override void SetItem(int index, Quote item) {
            base.SetItem(index, item);
            NotifyCollectionChanged(NotifyCollectionChangedAction.Replace, item, index);
        }
        #endregion key operations

        #region data fetch

        /// <summary>
        /// Fetch an url and send back the body response as a string
        /// </summary>
        /// <param name="url">URL to fetch</param>
        /// <returns></returns>
        public virtual async Task<string> FetchAsync(string url) {
            if (!NetworkInterface.GetIsNetworkAvailable()) {
                return null;
            }

            HttpClient http = new HttpClient();
            string responseBodyAsText;

            try {
                HttpResponseMessage message = await http.GetAsync(url);
                RedirectedURL = message.RequestMessage.RequestUri.ToString();
                responseBodyAsText = await message.Content.ReadAsStringAsync();
                return responseBodyAsText;

            } catch {
                return null;
            }
        }
        
        /// <summary>
        /// Use this method to load quotes into the collection the 1st time
        /// </summary>
        /// <returns></returns>
        public virtual async Task LoadQuotes() {

        }

        /// <summary>
        /// When the collection has already quotes loaded, 
        /// use this method to load more quotes
        /// </summary>
        /// <returns></returns>
        public virtual async Task LoadMoreQuotes() {

        }

        #endregion data fetch

        #region search
        /// <summary>
        /// Use this method to search for quotes from a specified url
        /// </summary>
        /// <param name="query">quotes query (ex.: eyes, chocolate)</param>
        /// <returns></returns>
        public virtual async Task<int> Search(string query="") {
            return 0;
        }

        /// <summary>
        /// When a 1st search has already been done.
        /// Use this method for incremental loading
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public virtual async Task<int> SearchMore(string query="") {
            return 0;
        }

        #endregion search

        #region io
        public virtual async Task<int> HandleFailedFetch() {
            return await LoadIO();
        }

        public virtual async Task<bool> SaveIO() {
            if (Count == 0) return true;
            await Settings.SaveQuotesAsync(this);
            return false;
        }

        public virtual async Task<int> LoadIO() {
            try {
                var savedQuotes = await Settings.LoadQuotesAsync(Name + ".json");
                if (savedQuotes == null) return 0;

                foreach (var q in savedQuotes) {
                    Add(q);
                }

                return Count;
            } 
            catch {
                return Count;
            }
        }
        #endregion io

        /* ***************
         * EVENTS HANDLERS
         * ***************
         */
        #region events
        private void NotifyPropertyChanged(String propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        private void NotifyCollectionChanged(NotifyCollectionChangedAction action, Quote item, int index) {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, item, index));
        }
        
        private void NotifyCollectionChanged(NotifyCollectionChangedAction action) {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action));
        }

        IAsyncOperation<LoadMoreItemsResult> ISupportIncrementalLoading.LoadMoreItemsAsync(uint count) {
            return LoadMoreItemsAsync(count).AsAsyncOperation();
        }
        
        public virtual async Task<LoadMoreItemsResult> LoadMoreItemsAsync(uint count) {
            return new LoadMoreItemsResult { Count = count };
        }
        #endregion events
    }
}
