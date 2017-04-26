using System.Threading.Tasks;
using citations365.Models;
using citations365.Services;
using Windows.Storage;
using System.Net.NetworkInformation;
using System.Net.Http;
using HtmlAgilityPack;
using System.Linq;
using System.ComponentModel;

namespace citations365.Data {
    public class Evene : SourceModel, INotifyPropertyChanged {
        public override string Name => "Evene";
        public override string Language => "FR";
        public override bool HasAuthors { get => true; set => base.HasAuthors = value; }
        public override bool HasSearch { get => true; set => base.HasSearch = value; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region recent
        public override async Task LoadRecent() {
            await InitializeFavorites();

            if (RecentList.Count > 0) return;

            await RecoveryFetchRecent();

            SetLockscreenQuoteAsHero();
        }
        
        public async Task RecoveryFetchRecent() {
            int added = 0;

            await BuildAndFetchRecent(); // Initial fetch
            if (added > 0) return;

            RecentList.Page++;
            await BuildAndFetchRecent(); // If failed, fetch from page 2
            if (added > 0) return;

            // If failed, fetch a random category
            await BuildAndFetchRecent("http://evene.lefigaro.fr/citations/mot.php?mot=absurde");
        }

        private async Task BuildAndFetchRecent(string query = "") {
            if (query.Length > 0) await RecentList.Fetch(query);

            string url = "http://evene.lefigaro.fr/citations/citation-jour.php?page=";

            if (RecentList.Page < 2) {
                url = url.Substring(0, (url.Length - 6));
            } else {
                url = url + RecentList.Page;
            }

            await RecentList.Fetch(url);
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

        public void CheckHeroQuote() {
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
        public async Task BuildAndFetchSearch(string query = "") {
            string url = "http://evene.lefigaro.fr/citations/mot.php?mot=";
            string _pageQuery = "&p=";

            // Checks if this is a new search
            if (query != string.Empty && query != ResultsList.Query) {
                ResultsList.Page = 1;
                ResultsList.HasMoreItems = true;
                ResultsList.RedirectedURL = "";
                ResultsList.Clear();
            }

            // Save the last query (if it's not an empty string)
            ResultsList.Query = query.Length > 0 ? query : ResultsList.Query;

            if (ResultsList.RedirectedURL.Length > 0) {
                if (ResultsList.RedirectedURL.Contains(_pageQuery)) {
                    ResultsList.RedirectedURL = 
                        ResultsList.RedirectedURL.Substring(0, ResultsList.RedirectedURL.IndexOf(_pageQuery));
                }

                url = ResultsList.RedirectedURL + _pageQuery + ResultsList.Page;

            } else {
                url = url + query + _pageQuery + ResultsList.Page;
            }

            await ResultsList.Fetch(url);
        }
        #endregion search

        #region authors
        public override async Task GetAuthors() {
            AuthorsList = await Settings.LoadAuthorsAsync();

            if (AuthorsList.Count > 0) {
                NotifyPropertyChanged("AuthorsListLoaded");
                return;
            }

            await LoadAuthors();
        }

        public override async Task LoadAuthors() {
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

                    NotifyPropertyChanged("AuthorsListLoaded");
                    return;

                } catch {
                    NotifyPropertyChanged("AuthorsListLoaded");
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

        public override async void LoadAuthorQuotes() {
            await BuildAndFetchAuthorQuotes("http://evene.lefigaro.fr" + AuthorQuotesURL);
        }

        public async Task BuildAndFetchAuthorQuotes(string query = "") {
            // Checks if this is a new search
            if ((!string.IsNullOrEmpty(query)) && query != LastAuthorRequest) {
                AuthorQuotesList.Page = 1;
                AuthorQuotesList.HasMoreItems = true;
                AuthorQuotesList.RedirectedURL = "";
                AuthorQuotesList.Clear();
            }

            AuthorQuotesList.Query = query.Length > 0 ? query : AuthorQuotesList.Query;

            var url = AuthorQuotesList.Query + "?page=" + AuthorQuotesList.Page;
            await AuthorQuotesList.Fetch(url);
        }

        #endregion authors
    }
}
