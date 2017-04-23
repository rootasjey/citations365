using citations365.Models;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.Storage;

namespace citations365.Controllers {
    public class TodayController
    {
        /*
         * **********
         * VARIABLES
         * **********
         */
        private const string DAILY_QUOTE = "DailyQuote"; // composite value
        private const string DAILY_QUOTE_CONTENT = "DailyQuoteContent";
        private const string DAILY_QUOTE_AUTHOR = "DailyQuoteAuthor";
        private const string DAILY_QUOTE_AUTHOR_LINK = "DailyQuoteAuthorLink";
        private const string DAILY_QUOTE_REFERENCE = "DailyQuoteReference";
        private const string DAILY_QUOTE_LINK = "DailyQuoteLink";
        
        private static TodayCollection _todayCollection { get; set; }
        public static TodayCollection TodayCollection {
            get {
                if (_todayCollection==null) {
                    _todayCollection = new TodayCollection();
                }
                return _todayCollection;
            }
        }
        
        public static async Task<bool> LoadData() {
            await FavoritesController.Initialize();

            if (IsDataLoaded()) return true;

            int added = await FetchAndRecover();

            Quote lockscreenQuote = GetLockScreenQuote();
            if (lockscreenQuote != null) {
                TodayCollection.Insert(0, lockscreenQuote);
            }

            if (added > 0) return true;
            return false;
        }

        public static async Task<int> FetchAndRecover() {
            int added = 0;

            // Normal fetch
            added = await TodayCollection.BuildAndFetch();
            if (added > 0) return added;

            // If failed, fetch from page 2
            TodayCollection.Page++;
            added = await TodayCollection.BuildAndFetch();
            if (added > 0) return added;

            // If failed, fetch a random category
            added = await TodayCollection.BuildAndFetch("http://evene.lefigaro.fr/citations/mot.php?mot=absurde");
            return added;
        }

        public static Quote GetLockScreenQuote() {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            ApplicationDataCompositeValue composite =
                (ApplicationDataCompositeValue)localSettings.Values[DAILY_QUOTE];

            if (composite != null) {
                Quote quote = new Quote() {
                    Content     = (string)composite[DAILY_QUOTE_CONTENT],
                    Author      = (string)composite[DAILY_QUOTE_AUTHOR],
                    AuthorLink  = (string)composite[DAILY_QUOTE_AUTHOR_LINK],
                    Reference   = (string)composite[DAILY_QUOTE_REFERENCE],
                    Link        = (string)composite[DAILY_QUOTE_LINK]
                };
                return quote;
            }
            return null;
        }
        
        public static void CheckHeroQuote() {
            if (TodayCollection.Count == 0) {
                return;
            }

            Quote lastFetchedQuote = GetLockScreenQuote();
            Quote heroQuote = TodayCollection[0];

            if (lastFetchedQuote == null) {
                return;
            }

            if (lastFetchedQuote.Link == heroQuote.Link) {
                return; // the hero quote is the last fetched quote
            }

            // update the hero quote's value
            // (inserting a new quote at 0 won't set it as hero quote)
            heroQuote.Content       = lastFetchedQuote.Content;
            heroQuote.Author        = lastFetchedQuote.Author;
            heroQuote.AuthorLink    = lastFetchedQuote.AuthorLink;
            heroQuote.Date          = lastFetchedQuote.Date;
            heroQuote.IsFavorite    = lastFetchedQuote.IsFavorite;
            heroQuote.Reference     = lastFetchedQuote.Reference;
        }
        
        
        public static bool IsDataLoaded() {
            return TodayCollection.Count > 0;
        }
        
        public static void SyncFavorites(string key) {
            if (TodayCollection.Contains(key)) {
                Quote quote = TodayCollection[key];
                quote.IsFavorite = FavoritesController.IsFavorite(key);
            }
        }
        
        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (e.NewItems != null)
                foreach (Quote item in e.NewItems)
                    item.PropertyChanged += QuotePropertyChanged;

            if (e.OldItems != null)
                foreach (Quote item in e.OldItems)
                    item.PropertyChanged -= QuotePropertyChanged;
        }

        private void QuotePropertyChanged(object sender, PropertyChangedEventArgs e) {
        }
    }
}
