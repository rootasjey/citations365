using citations365.Models;
using citations365.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace citations365.Data {
    public class SourceModel {
        #region variables
        public virtual ObservableKeyedCollection RecentList { get; set; }
        public virtual ObservableKeyedCollection FavoritesList { get; set; }
        public virtual ObservableKeyedCollection ResultsList { get; set; }
        public virtual ObservableCollection<Author> AuthorsList { get; set; }
        public virtual ObservableKeyedCollection AuthorQuotesList { get; set; }

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
        #endregion variables

        public virtual async Task LoadRecent() { }

        #region search

        public virtual async void Search(string query = "") {
            int found = await ResultsList.BuildAndFetch(query);
        }

        #endregion search

        #region authors
        public virtual async Task LoadAuthors() { }

        public virtual async Task FetchAuthors() { }

        // TODO: replace with LoadAuthors & FetchAuthors
        public virtual async Task GetAuthors() { }

        public virtual async Task<Author> LoadAuthor(Author partialAuthor) { return new Author(); }

        public virtual async void LoadAuthorQuotes() { }

        public bool AuthorHasQuotes() {
            return AuthorQuotesURL.Length > 0;
        }
        #endregion authors

        #region favorites
        public virtual async Task InitializeFavorites() {
            if (FavoritesList != null && FavoritesList.Count > 0) return;
            await LoadFavorites();
        }
        

        public virtual async Task LoadFavorites() {
            FavoritesList = await Settings.LoadFavoritesAsync(Name);
            if (FavoritesList == null) FavoritesList = new ObservableKeyedCollection();

            FavoritesList.CollectionChanged += FavoritesList_CollectionChanged;
        }

        public virtual void SaveFavorites() {
            Settings.SaveFavoritesAsync(FavoritesList, Name);
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

        public virtual bool IsFavorite(string key) {
            if (string.IsNullOrEmpty(key) || FavoritesList == null) return false;
            if (FavoritesList.Contains(key)) {
                return true;
            }

            return false;
        }

        public virtual async void AddFavorites(Quote quote) {
            if (quote == null) return;
            if (FavoritesList.Contains(quote.Link)) return;

            quote.IsFavorite = true;
            FavoritesList.Add(quote);
            await Settings.SaveFavoritesAsync(FavoritesList, Name);
        }

        public virtual async void RemoveFavorites(Quote quote) {
            if (quote == null) return;

            if (FavoritesList.Contains(quote.Link)) {
                FavoritesList.Remove(quote.Link);
                UpdateAllFavorites(quote);

                await Settings.SaveFavoritesAsync(FavoritesList, Name);
            }
        }

        public virtual void UpdateAllFavorites(Quote quote) {
            var key = quote.Link;

            UpdateRecentFavorites(key);
            UpdateResultsFavorites(key);
            UpdateAuthorFavorites(key);
        }

        public virtual void UpdateRecentFavorites(string key) {
            if (RecentList.Contains(key)) {
                Quote quote = RecentList[key];
                quote.IsFavorite = IsFavorite(key);
            }
        }

        public virtual void UpdateResultsFavorites(string key) {
            if (ResultsList.Contains(key)) {
                Quote quote = ResultsList[key];
                quote.IsFavorite = IsFavorite(key);
            }
        }

        public virtual void UpdateAuthorFavorites(string key) {
            if (AuthorQuotesList.Contains(key)) {
                Quote quote = AuthorQuotesList[key];
                quote.IsFavorite = IsFavorite(key);
            }
        }
        #endregion favorites
    }
}
