using citations365.Models;
using citations365.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace citations365.Data {
    public class SourceModel : INotifyPropertyChanged {
        #region variables
        public virtual ObservableKeyedCollection RecentList { get; set; }
        public virtual ObservableKeyedCollection FavoritesList { get; set; }
        public virtual ObservableKeyedCollection ResultsList { get; set; }
        public virtual ObservableCollection<Author> AuthorsList { get; set; }
        public virtual ObservableKeyedCollection AuthorQuotesList { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual bool IsLoadingAuthor { get; set; }
        public virtual bool AuthorLoaded { get; set; }
        public string LastAuthorRequest { get; set; }
        public string AuthorQuotesURL { get; set; }
        public Author LastAuthorFetched { get; set; }

        public virtual string Name { get; }

        public virtual string Language { get; }

        private bool _HasAuthors;

        public virtual bool HasAuthors {
            get { return _HasAuthors; }
            set { _HasAuthors = value; }
        }

        private bool _HasSearch;

        public virtual bool HasSearch {
            get { return _HasSearch; }
            set { _HasSearch = value; }
        }

        public string LastSearchQuery { get; set; }

        #endregion variables

        #region recent
        public virtual async Task LoadRecent() { }

        public virtual void CheckHeroQuote() { }
        #endregion recent

        #region search

        public virtual async Task<int> Search(string query = "") {
            //int found = await ResultsList.BuildAndFetch(query);
            //return found;
            return await ResultsList.Search();
        }

        #endregion search

        #region authors
        public virtual async Task LoadAuthors() { }

        public virtual async Task FetchAuthors() { }

        // TODO: replace with LoadAuthors & FetchAuthors
        public virtual async Task GetAuthors() { }

        public virtual async Task<Author> LoadAuthor(Author partialAuthor) {
            return new Author();
        }

        public virtual async Task LoadAuthorQuotes() { }

        public bool AuthorHasQuotes() {
            return AuthorQuotesURL.Length > 0;
        }
        #endregion authors

        #region favorites
        public virtual async Task InitializeFavorites() {
            if (FavoritesList != null && FavoritesList.Count > 0) return;
            await LoadFavorites();
            NotifyPropertyChanged("FavoritesList");
        }
        

        public virtual async Task LoadFavorites() {
            FavoritesList = await Settings.LoadFavoritesAsync(Language);
            if (FavoritesList == null) FavoritesList = new ObservableKeyedCollection();

            FavoritesList.CollectionChanged += FavoritesList_CollectionChanged;
        }

        public virtual async Task SaveFavorites() {
            await Settings.SaveFavoritesAsync(FavoritesList, Language);
        }

        public virtual async Task<bool> IsFavoritesEmpty() {
            await InitializeFavorites();
            return FavoritesList.Count == 0;
        }

        public virtual void FavoritesList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
        }

        public virtual bool IsFavorite(Quote quote) {
            if (quote == null || FavoritesList == null) return false;

            if (FavoritesList.Contains(quote.Link)) {
                return true;
            }

            return false;
        }

        public async virtual Task<bool> IsFavorite(string key) {
            await InitializeFavorites();
            if (string.IsNullOrEmpty(key)/* || FavoritesList == null*/) return false;
            if (FavoritesList.Contains(key)) {
                return true;
            }

            return false;
        }

        public virtual async void AddToFavorites(Quote quote) {
            if (quote == null) return;
            if (FavoritesList.Contains(quote.Link)) return;

            quote.IsFavorite = true;
            FavoritesList.Add(quote);
            //await Settings.SaveFavoritesAsync(FavoritesList, Name);
            await SaveFavorites();
        }

        public virtual async void RemoveFromFavorites(Quote quote) {
            if (quote == null) return;
            if (!FavoritesList.Contains(quote.Link)) return;

            FavoritesList.Remove(quote.Link);
            UpdateAllFavorites(quote);

            //await Settings.SaveFavoritesAsync(FavoritesList, Name);
            await SaveFavorites();
        }

        public virtual void UpdateAllFavorites(Quote quote) {
            var key = quote.Link;

            UpdateRecentFavorites(key);
            UpdateResultsFavorites(key);
            UpdateAuthorFavorites(key);
        }

        public async virtual void UpdateRecentFavorites(string key) {
            if (RecentList == null) return;
            if (RecentList.Contains(key)) {
                Quote quote = RecentList[key];
                quote.IsFavorite = await IsFavorite(key);
            }
        }

        public async virtual void UpdateResultsFavorites(string key) {
            if (ResultsList == null) return;
            if (ResultsList.Contains(key)) {
                Quote quote = ResultsList[key];
                quote.IsFavorite = await IsFavorite(key);
            }
        }

        public async virtual void UpdateAuthorFavorites(string key) {
            if (AuthorQuotesList == null) return;
            if (AuthorQuotesList.Contains(key)) {
                Quote quote = AuthorQuotesList[key];
                quote.IsFavorite = await IsFavorite(key);
            }
        }
        #endregion favorites
    }
}
