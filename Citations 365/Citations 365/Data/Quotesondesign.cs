using citations365.Models;
using citations365.Services;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;


// https://quotesondesign.com/api-v4-0/
namespace citations365.Data {
    public class Quotesondesign : SourceModel, INotifyPropertyChanged {
        public override string Name => "Quotesondesign";
        public override string Language => "EN";
        public override bool HasAuthors { get => false; }
        public override bool HasSearch { get => false; }


        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override async Task LoadRecent() {
            if (RecentList != null && RecentList.Count > 0) return;
            RecentList = new RecentListQuotes() { Favorites = FavoritesList };

            if (!NetworkInterface.GetIsNetworkAvailable()) {
                return;
            }

            await RecentList.LoadQuotes();
            NotifyPropertyChanged("RecentLoaded");
        }        
    }

    public class RecentListQuotes: ObservableKeyedCollection {
        public override string Name => "RecentListQuotes";

        public override uint ItemsToLoad => 20;

        public RecentListQuotes() { HasMoreItems = true; }

        async Task<bool> EnsureResponseOK(string response) {
            if (response != null) return true;
            int added = await HandleFailedFetch();
            HasMoreItems = false;
            return false;
        }

        void SaveQuotesToIO() {
            if (Count == 0 || Count > 70) return;
            SaveIO();
        }

        public override async Task LoadQuotes() {
            string url = "http://quotesondesign.com/wp-json/posts?filter[orderby]=rand&filter[posts_per_page]=" + ItemsToLoad;
            var response = await FetchAsync(url);

            var responseOK = await EnsureResponseOK(response);
            if (!responseOK) return;

            var array = JArray.Parse(response);
            var added = 0;

            foreach (JObject item in array) {
                var quote = new Quote() {
                    Author = (string)item["title"],
                    Content = (string)item["content"],
                    Link = (string)item["link"]
                };
                quote = Formatter.Normalize(quote);

                if (Contains(quote.Link)) continue;

                Add(quote);
                added++;
            }

            if (added == 0) HasMoreItems = false;
            SaveQuotesToIO();
        }

        public override async Task LoadMoreQuotes() {
            await LoadQuotes();
        }

        public override async Task<LoadMoreItemsResult> LoadMoreItemsAsync(uint count) {
            await LoadMoreQuotes();
            return new LoadMoreItemsResult { Count = count };
        }
        
    }
}
