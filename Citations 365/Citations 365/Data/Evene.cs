using System.Threading.Tasks;
using citations365.Models;
using citations365.Services;
using Windows.Storage;
using System.Net.NetworkInformation;
using System.Net.Http;
using HtmlAgilityPack;
using System.Linq;
using System.Collections.Generic;
using Windows.UI.Xaml.Data;
using System;

namespace citations365.Data {
    public class Evene : SourceModel {
        public override string Name => "Evene";

        public override string Language => "FR";

        public override bool HasAuthors { get => true; set => base.HasAuthors = value; }

        public override bool HasSearch { get => true; set => base.HasSearch = value; }

        #region recent
        public override async Task LoadRecent() {
            await InitializeFavorites();

            if (RecentList != null && RecentList.Count > 0) return;

            RecentList = new EveneRecentList() {
                Favorites = FavoritesList
            };

            await RecentList.LoadQuotes();

            SetLockscreenQuoteAsHero();
        }
        
        void SetFavoritesQuotes(ObservableKeyedCollection collection) {
            foreach (var favQuote in FavoritesList) {
                if (collection.Contains(favQuote.Link)) {
                    collection[favQuote.Link].IsFavorite = true;
                }
            }
        }

        public Quote GetLockScreenQuote() {
            string DAILY_QUOTE = "DailyQuote";
            string DAILY_QUOTE_CONTENT = "DailyQuoteContent";
            string DAILY_QUOTE_AUTHOR = "DailyQuoteAuthor";
            string DAILY_QUOTE_AUTHOR_LINK = "DailyQuoteAuthorLink";
            string DAILY_QUOTE_REFERENCE = "DailyQuoteReference";
            string DAILY_QUOTE_LINK = "DailyQuoteLink";

            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                ApplicationDataCompositeValue composite =
                    (ApplicationDataCompositeValue)localSettings.Values[DAILY_QUOTE];

            if (composite == null) return null;

            return new Quote() {
                Content = (string)composite[DAILY_QUOTE_CONTENT],
                Author = (string)composite[DAILY_QUOTE_AUTHOR],
                AuthorLink = (string)composite[DAILY_QUOTE_AUTHOR_LINK],
                Reference = (string)composite[DAILY_QUOTE_REFERENCE],
                Link = (string)composite[DAILY_QUOTE_LINK]
            };
        }

        public void SetLockscreenQuoteAsHero() {
            var hero = GetLockScreenQuote();
            if (hero == null) return;
            RecentList.Insert(0, hero);
        }

        public override void CheckHeroQuote() {
            if (RecentList.Count == 0) {
                return;
            }

            var lockscreenQuote = GetLockScreenQuote();
            var heroQuote = RecentList[0];

            if (lockscreenQuote == null) {
                return;
            }

            if (lockscreenQuote.Link == heroQuote.Link) {
                return; // the hero quote is the last fetched quote
            }

            // update the hero quote's value
            // (inserting a new quote at 0 won't set it as hero quote)
            heroQuote.Content = lockscreenQuote.Content;
            heroQuote.Author = lockscreenQuote.Author;
            heroQuote.AuthorLink = lockscreenQuote.AuthorLink;
            heroQuote.Date = lockscreenQuote.Date;
            heroQuote.IsFavorite = lockscreenQuote.IsFavorite;
            heroQuote.Reference = lockscreenQuote.Reference;
        }

        #endregion recent

        #region search
        public override async Task<int> Search(string query = "") {
            if (ResultsList == null) {
                ResultsList = new EveneResultsList() {
                    Favorites = FavoritesList
                };
            }

            if (!NetworkInterface.GetIsNetworkAvailable()) {
                return 0;
            }

            if (LastSearchQuery == query) {
                NotifyPropertyChanged("ResultsLoaded");
                return ResultsList.Count;
            }

            LastSearchQuery = query;

            var added = await ResultsList.Search(query);
            NotifyPropertyChanged("ResultsLoaded");
            return added;
        }
        #endregion search

        #region authors
        public override async Task LoadAuthors() {
            if (IsAuthorListFilled()) {
                NotifyPropertyChanged("AuthorsListLoaded");
                return;
            }

            AuthorsList = await Settings.LoadAuthorsAsync();

            if (IsAuthorListFilled()) {
                NotifyPropertyChanged("AuthorsListLoaded");
                return;
            }

            await FetchAuthorsList();
            NotifyPropertyChanged("AuthorsListLoaded");

            bool IsAuthorListFilled()
            {
                return AuthorsList != null && AuthorsList.Count > 0;
            }
        }

        async Task FetchAuthorsList() {
            if (AuthorsList == null) {
                AuthorsList = new System.Collections.ObjectModel.ObservableCollection<Author>();
            }

            if (NetworkInterface.GetIsNetworkAvailable()) {
                HttpClient http = new HttpClient();

                try {
                    string responseBodyAsText = await http.GetStringAsync("http://www.evene.fr/citations/dictionnaire-citations-auteurs.php");
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(responseBodyAsText);

                    string[] authorsNames = doc.DocumentNode
                        .Descendants("a")
                        .Where(x => x.GetAttributeValue("class", "") == "N11 txtC30")
                        .Select(y => y.InnerText)
                        .ToArray();

                    string[] authorsLinks = doc.DocumentNode
                        .Descendants("a")
                        .Where(x => x.GetAttributeValue("class", "") == "N11 txtC30")
                        .Select(y => y.GetAttributeValue("href", ""))
                        .ToArray();

                    for (int i = 0; i < authorsNames.Length; i++) {
                        Author author = new Author() {
                            Name = authorsNames[i],
                            Link = authorsLinks[i],
                            ImageLink = "ms-appx:///Assets/Icons/gray.png"
                        };

                        AuthorsList.Add(author);
                    }

                    Settings.SaveAuthorsAsync(AuthorsList);
                    return;

                } catch {
                    return;
                }
            }

        }

        public override async Task<Author> LoadAuthor(Author partialAuthor) {
            IsLoadingAuthor = true;

            var url = partialAuthor.Link;

            if (LastAuthorRequest == url) {
                IsLoadingAuthor = false;
                AuthorLoaded = true;
                NotifyPropertyChanged("AuthorLoaded");

                return LastAuthorFetched;
            }

            LastAuthorRequest = url;
            LastAuthorFetched = await FetchAuthorInformation(url);

            IsLoadingAuthor = false;
            AuthorLoaded = true;

            NotifyPropertyChanged("AuthorLoaded");

            return LastAuthorFetched;
        }

        public async Task<Author> FetchAuthorInformation(string url) {
            if (!NetworkInterface.GetIsNetworkAvailable()) {
                return null;
            }

            HttpClient http = new HttpClient();
            string responseBodyAsText;

            try {
                HttpResponseMessage message = await http.GetAsync(url);
                responseBodyAsText = await message.Content.ReadAsStringAsync();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(responseBodyAsText);

                string job = doc.DocumentNode
                                .Descendants("div")
                                .Where(x => x.GetAttributeValue("itemprop", "") == "jobTitle")
                                .Select(y => y.InnerHtml)
                                .ToArray()
                                .FirstOrDefault();

                string bio = doc.DocumentNode
                                .Descendants("div")
                                .Where(x => x.GetAttributeValue("itemprop", "") == "description")
                                .Select(y => y.InnerHtml)
                                .ToArray()
                                .FirstOrDefault();

                string birth = doc.DocumentNode
                                .Descendants("time")
                                .Where(x => x.GetAttributeValue("property", "") == "birthDate")
                                .Select(y => y.InnerHtml)
                                .ToArray()
                                .FirstOrDefault();

                string death = doc.DocumentNode
                                .Descendants("time")
                                .Where(x => x.GetAttributeValue("property", "") == "deathDate")
                                .Select(y => y.InnerHtml)
                                .ToArray()
                                .FirstOrDefault();

                string quote = doc.DocumentNode
                                .Descendants("p")
                                .Where(x => x.GetAttributeValue("class", "") == "figsco__artist__quote")
                                .Select(y => y.InnerHtml)
                                .ToArray()
                                .FirstOrDefault();

                AuthorQuotesURL = doc.DocumentNode
                                    .Descendants("a")
                                    .Where(x => x.GetAttributeValue("class", "") == "figsco__quote__link")
                                    .Select(y => y.GetAttributeValue("href", ""))
                                    .ToArray()
                                    .FirstOrDefault();


                job = job ?? "";
                bio = bio ?? "";
                birth = birth ?? "";
                death = death ?? "";
                quote = quote ?? "";

                AuthorQuotesURL = AuthorQuotesURL ?? "";

                var author = new Author() {
                    Biography = Formatter.DeleteHTMLTags(bio),
                    Birth = birth,
                    Death = death,
                    Picture = "ms-appx:///Assets/Icons/gray.png",
                    Job = job,
                    LifeTime = birth + " - " + death,
                    Quote = quote
                };

                return author;

            } catch { return null; }
        }

        public override async Task LoadAuthorQuotes() {
            if (AuthorQuotesList == null) {
                AuthorQuotesList = new EveneAuthorQuotesList() {
                    Favorites = FavoritesList
                };
            }

            if (string.IsNullOrEmpty(AuthorQuotesURL) || 
                AuthorQuotesURL == AuthorQuotesList.URL) {
                return; // same last author
            }

            AuthorQuotesList.Page = 1;
            AuthorQuotesList.HasMoreItems = true;
            AuthorQuotesList.RedirectedURL = "";
            AuthorQuotesList.Clear();

            AuthorQuotesList.URL = AuthorQuotesURL;
            await AuthorQuotesList.LoadQuotes();
        }

        // NOTE: Unused
        public void IsAWoman(Author author) {
            //if (author == null) return true;

            //var woman = CountWomanPronums(author.Biography);
            //var man = CountManPronums(author.Biography);

            //return woman > man;

            //int CountWomanPronums(string biography)
            //{
            //    var womanRegex = new Regex(@"(\s+)(elle)(\s+)");
            //    var womanCount = womanRegex.Matches(biography);

            //    return womanCount.Count;
            //}

            //int CountManPronums(string biography)
            //{
            //    var manRegex = new Regex(@"(\s+)(il)(\s+)");
            //    var manCount = manRegex.Matches(biography);

            //    return manCount.Count;
            //}
        }

        #endregion authors

        public static List<Quote> FindEveneQuotes(string response) {
            var results = new List<Quote>();

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(response);

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

                quote = Formatter.Normalize(quote);
                quote.IsFavorite = false;

                results.Add(quote);
            }

            return results;
        }

        public static int AddQuotesToCollection(ObservableKeyedCollection coll, List<Quote> list) {
            int added = 0;
            foreach (var quote in list) {
                if (!coll.Contains(quote.Link)) {
                    quote.IsFavorite = coll.Favorites != null ? 
                        coll.Favorites.Contains(quote.Link) : false;

                    coll.Add(quote);
                    added++;
                }
            }

            return added;
        }
    }

    public class EveneRecentList : ObservableKeyedCollection {
        public override string Name => "Evene";

        public EveneRecentList() { HasMoreItems = true; }
        
        string GetRandomCategory() {
            string[] categories = {
                "absurde",
                "réussite",
                "courage",
                "voyage",
                "sagesse",
                "art",
                "Espoir",
                "tendresse",
                "aimer"
            };

            var rand = new Random();
            var index = rand.Next(0, categories.Length - 1);
            return categories[index];
        }

        public override async Task LoadQuotes() {
            string[] endpoints = {
                "",
                "http://evene.lefigaro.fr/citations/citation-jour.php?page=3",
                "http://evene.lefigaro.fr/citations/citation-jour.php?page=20",
                "http://evene.lefigaro.fr/citations/mot.php?mot=" + GetRandomCategory()
            };

            List<Quote> results = null;

            foreach (string point in endpoints) {
                var url = BuildQuery(point);
                var response = await FetchAsync(url);

                var responseOK = await EnsureResponseOK(response);
                if (!responseOK) return;

                results = Evene.FindEveneQuotes(response);

                if (results != null && results.Count > 0) break;
                Page++;
            }

            if (results == null || results.Count == 0) 
                { HasMoreItems = false; return; }
            
            Evene.AddQuotesToCollection(this, results);
            SaveQuotesToIO();
        }

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

        private string BuildQuery(string query = "") {
            if (query.Length > 0) return query;
            return "http://evene.lefigaro.fr/citations/citation-jour.php?page=" + Page;
        }

        public override async Task LoadMoreQuotes() {
            Page++;
            var url = BuildQuery();
            var response = await FetchAsync(url);

            var responseOK = await EnsureResponseOK(response);
            if (!responseOK) return;

            var results = Evene.FindEveneQuotes(response);

            if (results == null || results.Count == 0) 
                { HasMoreItems = false; return; }
            
            Evene.AddQuotesToCollection(this, results);
            SaveQuotesToIO();
        }

        public override async Task<LoadMoreItemsResult> LoadMoreItemsAsync(uint count) {
            await LoadMoreQuotes();
            return new LoadMoreItemsResult { Count = count };
        }
    }

    public class EveneAuthorQuotesList : ObservableKeyedCollection {
        public EveneAuthorQuotesList() { HasMoreItems = true; }

        private string BuildQuery() {
            return "http://evene.lefigaro.fr" + URL;
        }

        public override async Task LoadQuotes() {
            var added = 0;
            var query = BuildQuery();

            Query = query.Length > 0 ? query : Query;

            var url = Query + "?page=" + Page;
            var response = await FetchAsync(url);
            var results = Evene.FindEveneQuotes(response);

            if (results == null || results.Count == 0) {
                HasMoreItems = false;
                return;
            }

            added = Evene.AddQuotesToCollection(this, results);
            if (added == 0) HasMoreItems = false;
        }

        public override async Task LoadMoreQuotes() {
            Page++;
            await LoadQuotes();
        }

        public override async Task<LoadMoreItemsResult> LoadMoreItemsAsync(uint count) {
            await LoadMoreQuotes();
            return new LoadMoreItemsResult { Count = count };
        }
    }

    public class EveneResultsList: ObservableKeyedCollection {
        public override string Name => "ResultsList";

        public override async Task<int> Search(string query="") {
            var url = BuildQuery(query);
            var response = await FetchAsync(url);
            var results = Evene.FindEveneQuotes(response);
            int added = Evene.AddQuotesToCollection(this, results);

            return added;
        }

        public override async Task<int> SearchMore(string query = "") {
            Page++;
            int added = await Search(query);
            if (added == 0) { HasMoreItems = false; }
            return added;
        }

        string BuildQuery(string query) {
            string url = "http://evene.lefigaro.fr/citations/mot.php?mot=";

            string _pageQuery = "&p=";

            // Checks if this is a new search
            if (!string.IsNullOrEmpty(query) && query != Query) {
                Page = 1;
                HasMoreItems = true;
                RedirectedURL = "";
                Clear();
            }

            // Save the last query (if it's not an empty string)
            Query = query.Length > 0 ? query : Query;

            if (RedirectedURL.Length > 0) {
                if (RedirectedURL.Contains(_pageQuery)) {
                    RedirectedURL = RedirectedURL.Substring(0, RedirectedURL.IndexOf(_pageQuery));
                }
                url = RedirectedURL + _pageQuery + Page;

            } else {
                url = url + query + _pageQuery + Page;
            }
            return url;
        }

        public override async Task<LoadMoreItemsResult> LoadMoreItemsAsync(uint count) {
            int added = await SearchMore();
            return new LoadMoreItemsResult { Count = count };
        }
    }
}
