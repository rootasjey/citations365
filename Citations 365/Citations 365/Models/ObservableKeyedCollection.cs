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


        private int _page = 3; // TODO: Set back to 1
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
        
        public virtual async Task<int> BuildAndFetch(string query = "") {
            string url = "";
            return await Fetch(url);
        }
        
        public virtual async Task<int> Fetch(string url) {
            int quotesAdded = 0;
            string responseBodyAsText;

            if (!NetworkInterface.GetIsNetworkAvailable()) {
                return await HandleFailedFetch();
            }

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

                    if (content == null) continue; // check if this is a valid quote
                    if (authorAndReference == null) continue;

                    var authorNode = authorAndReference.Descendants("a").FirstOrDefault();
                    string authorName = "Anonyme";
                    string authorLink = "";

                    if (authorNode != null) { // if the quote as an author
                        authorName = authorNode.InnerText.Contains("Vos avis") ? authorName : authorNode.InnerText;
                        authorLink = "http://www.evene.fr" + authorNode.GetAttributeValue("href", "");
                    }

                    string quoteLink = content.ChildNodes.FirstOrDefault().GetAttributeValue("href", "");

                    string referenceName = "";
                    int separator = authorAndReference.InnerText.IndexOf('/');
                    
                    if (separator > -1) {
                        referenceName = authorAndReference.InnerText.Substring(separator + 2);
                    }

                    Quote quote = new Quote() {
                        Content = content.InnerText,
                        Author = authorName,
                        AuthorLink = authorLink,
                        Reference = referenceName,
                        Link = quoteLink
                    };

                    quote               = Formatter.Normalize(quote);
                    quote.IsFavorite    = IsFavorite(quote);

                    if (!Contains(quote.Link)) {
                        Add(quote);
                        quotesAdded++;
                    }
                }

                if (quotesAdded == 0) {
                    HasMoreItems = false;
                    Page = 0;

                } else {
                    HasMoreItems = true;
                }

                if (AllowOffline && Count > 0 && Count < 100) {
                    SaveIO();
                }

                Page++;

                return quotesAdded;

            } catch {
                HasMoreItems = false;
                return await HandleFailedFetch();
            }
        }
        
        public virtual async Task<int> HandleFailedFetch() {
            return await LoadIO();
        }
        
        public virtual bool IsFavorite(Quote quote) {
            return false;
        }

        public virtual bool IsDataLoaded() {
            return Count > 0;
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


        /* ***************
         * EVENTS HANDLERS
         * ***************
         */
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
            int itemsCount = await BuildAndFetch();
            return new LoadMoreItemsResult { Count = (uint)itemsCount };
        }
    }
}
