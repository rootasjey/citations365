using citations365.Models;
using citations365.Services;
using HtmlAgilityPack;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace citations365.Controllers {
    public class AuthorsController : INotifyPropertyChanged {
        private static string _url {
            get {
                return "http://www.evene.fr/citations/dictionnaire-citations-auteurs.php";
            }
        }

        private static bool _IsLoading { get; set; }
        
        /*
         * ************
         * COLLECTIONS
         * ************
         */
        private static ObservableCollection<Author> _authorsCollection { get; set; }
        
        public static ObservableCollection<Author> AuthorsCollection {
            get {
                if (_authorsCollection == null) {
                    _authorsCollection = new ObservableCollection<Author>();
                }   return _authorsCollection;
            }
            set { _authorsCollection = value; }
        }

        public AuthorsController() {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        public async Task<bool> LoadData() {
            if (!IsDataLoaded()) {
                _IsLoading = true;

                bool result = await GetAuthors();

                _IsLoading = false;

                return result;
            } 
            else {
                _IsLoading = false;
                return true;
            }            
        }
        
        public async Task<bool> Reload() {
            _IsLoading = true;

            if (IsDataLoaded()) {
                AuthorsCollection.Clear();
            }

            bool result = await LoadAuthors();
            _IsLoading = false;

            return result;
        }
        
        public static bool IsDataLoaded() {
            return AuthorsCollection.Count > 0 || _IsLoading;
        }

        public async Task<bool> GetAuthors() {
            AuthorsCollection = await Settings.LoadAuthorsAsync();

            if (AuthorsCollection.Count > 0) {
                NotifyPropertyChanged("Loaded");
                return true;
            }

            return await LoadAuthors();
        }
        
        public async Task<bool> LoadAuthors() {
            if (NetworkInterface.GetIsNetworkAvailable()) {
                HttpClient http = new HttpClient();

                try {
                    string responseBodyAsText = await http.GetStringAsync(_url);
                    // Create a html document to parse the data
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(responseBodyAsText);

                    string[] authorsNames = doc.DocumentNode.Descendants("a").Where(x => (string)x.GetAttributeValue("class", "") == "N11 txtC30").Select(y => (string)y.InnerText).ToArray();
                    string[] authorsLinks = doc.DocumentNode.Descendants("a").Where(x => (string)x.GetAttributeValue("class", "") == "N11 txtC30").Select(y => (string)y.GetAttributeValue("href", "")).ToArray();

                    for (int i = 0; i < authorsNames.Length; i++) {
                        Author author = new Author() {
                            Name = authorsNames[i],
                            Link = authorsLinks[i],
                            ImageLink = "ms-appx:///Assets/Icons/gray.png"
                        };
                        AuthorsCollection.Add(author);
                    }

                    Settings.SaveAuthorsAsync(AuthorsCollection);

                    NotifyPropertyChanged("Loaded");
                    return true;

                } catch (HttpRequestException hre) {
                    NotifyPropertyChanged("Loaded");
                    return false;
                }

            } else {
                return false;
            }
        }
        
    }
}
