using citations365.Models;
using HtmlAgilityPack;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace citations365.Controllers {
    public class DetailAuthorController {
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

        /*
         * ***********
         * VARIABLES
         * ***********
         */
        private Author _author { get; set; }

        private string _quotesLink { get; set; }

        private string _lastURL { get; set; }

        private AuthorInfos _lastAuthor { get; set; }

        private bool _isSameRequest { get; set; }
        /*
         * ***********
         * CONSTRUCTOR
         * ***********
         */
        /// <summary>
        /// Initialize the controller
        /// </summary>
        public DetailAuthorController() {

        }

        /*
         * ********
         * METHODS
         * ********
         */
        public async Task<AuthorInfos> LoadData(string url) {
            if (isSameRequest(url)) {
                return _lastAuthor;
            }

            SaveURL(url);
            return _lastAuthor = await FetchBio(url);
        }

        private void SaveURL(string url) {
            _lastURL = url;
        }

        public bool isSameRequest(string url) {
            _isSameRequest = _lastURL == url;
            return _isSameRequest;
        }

        public bool isSameRequest() {
            return _isSameRequest;
        }

        public async Task<bool> Reload() {
            return false;
        }

        public async Task<AuthorInfos> FetchBio(string url) {
            // If there's no internet connection
            if (!NetworkInterface.GetIsNetworkAvailable()) {
                return null;
            }

            // Fetch the content from a web source
            HttpClient http = new HttpClient();
            string responseBodyAsText;

            try {
                HttpResponseMessage message = await http.GetAsync(url);
                responseBodyAsText = await message.Content.ReadAsStringAsync();

                // HTML Document building
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(responseBodyAsText);

                string job = doc.DocumentNode.Descendants("div").Where(x => x.GetAttributeValue("itemprop", "") == "jobTitle").Select(y => y.InnerHtml).ToArray().FirstOrDefault();
                string bio = doc.DocumentNode.Descendants("div").Where(x => x.GetAttributeValue("itemprop", "") == "description").Select(y => y.InnerHtml).ToArray().FirstOrDefault();
                string birth = doc.DocumentNode.Descendants("time").Where(x => x.GetAttributeValue("property", "") == "birthDate").Select(y => y.InnerHtml).ToArray().FirstOrDefault();
                string death = doc.DocumentNode.Descendants("time").Where(x => x.GetAttributeValue("property", "") == "deathDate").Select(y => y.InnerHtml).ToArray().FirstOrDefault();
                string quote = doc.DocumentNode.Descendants("p").Where(x => x.GetAttributeValue("class", "") == "figsco__artist__quote").Select(y => y.InnerHtml).ToArray().FirstOrDefault();
                _quotesLink = doc.DocumentNode.Descendants("a").Where(x => x.GetAttributeValue("class", "") == "figsco__quote__link").Select(y => y.GetAttributeValue("href", "")).ToArray().FirstOrDefault();

                job         = job   != null ? job   : "";
                bio         = bio   != null ? bio   : "";
                birth       = birth != null ? birth : "";
                death       = death != null ? death : "";
                quote       = quote != null ? quote : "";

                _quotesLink = _quotesLink != null ? _quotesLink : "";

                AuthorInfos infos = new AuthorInfos() {
                    job = job,
                    bio = Controller.DeleteHTMLTags(bio),
                    birth = birth,
                    death = death,
                    quote = quote
                };

                return infos;

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
    }

    public class AuthorInfos {
        public string bio { get; set; }
        public string birth { get; set; }
        public string death { get; set; }
        public string job { get; set; }
        public string quote { get; set; }
    }
}
