﻿using citations365.Data;
using citations365.Models;
using HtmlAgilityPack;
using System;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace citations365.Controllers {
    public class DetailAuthorController : INotifyPropertyChanged {
        /*
         * ************
         * COLLECTIONS
         * ************
         */
        private AuthorQuotesCollection _authorQuotesCollection { get; set; }
        public AuthorQuotesCollection AuthorQuotesCollection {
            get {
                if (_authorQuotesCollection == null) {
                    _authorQuotesCollection = new AuthorQuotesCollection();
                }
                return _authorQuotesCollection;
            }
        }

        #region variables
        private string _quotesLink { get; set; }

        private string _lastURL { get; set; }

        public Author _lastAuthor { get; set; }

        private bool _isSameRequest { get; set; }

        public static bool IsLoaded { get; set; }

        public static bool IsLoading { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion variables

        public async Task<Author> LoadData(string url) {
            IsLoading = true;

            if (IsSameRequest(url)) {
                IsLoading = false;
                IsLoaded = true;
                NotifyPropertyChanged("Loaded");

                return _lastAuthor;
            }

            SaveURL(url);
            _lastAuthor = await FetchInformation(url);

            IsLoading = false;
            IsLoaded = true;

            NotifyPropertyChanged("Loaded");

            return _lastAuthor;
        }

        private void SaveURL(string url) {
            _lastURL = url;
        }

        public bool IsSameRequest(string url) {
            _isSameRequest = _lastURL == url;
            return _isSameRequest;
        }

        public bool IsSameRequest() {
            return _isSameRequest;
        }

        public async Task<Author> FetchInformation(string url) {
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

                _quotesLink = doc.DocumentNode
                                .Descendants("a")
                                .Where(x => x.GetAttributeValue("class", "") == "figsco__quote__link")
                                .Select(y => y.GetAttributeValue("href", ""))
                                .ToArray()
                                .FirstOrDefault();


                job         = job ?? "";
                bio         = bio ?? "";
                birth       = birth ?? "";
                death       = death ?? "";
                quote       = quote ?? "";

                _quotesLink = _quotesLink ?? "";

                var author = new Author() {
                    Biography = Evene.DeleteHTMLTags(bio),
                    Birth = birth,
                    Death = death,
                    Picture = "ms-appx:///Assets/Icons/gray.png",
                    Job = job,
                    LifeTime = birth + " - " + death,
                    Quote = quote
                };

                return author;

            } catch (HttpRequestException hre) {
                return null;
            }
        }

        public async Task<bool> FetchQuotes() {
            int added = await AuthorQuotesCollection.BuildAndFetch("http://evene.lefigaro.fr" + _quotesLink);
            if (added > 0) {
                return true;
            }
            return false;
        }

        public bool HasQuotes() {
            return _quotesLink.Length > 0;
        }

        public bool QuotesLoaded() {
            return AuthorQuotesCollection.Count > 0;
        }

        public static bool IsAWoman(Author author) {
            if (author == null) return true;

            var woman = CountWomanPronums(author.Biography);
            var man = CountManPronums(author.Biography);

            return woman > man;

            int CountWomanPronums(string biography)
            {
                var womanRegex = new Regex(@"(\s+)(elle)(\s+)");
                var womanCount = womanRegex.Matches(biography);

                return womanCount.Count;
            }

            int CountManPronums(string biography)
            {
                var manRegex = new Regex(@"(\s+)(il)(\s+)");
                var manCount = manRegex.Matches(biography);

                return manCount.Count;
            }
        }
    }
}
