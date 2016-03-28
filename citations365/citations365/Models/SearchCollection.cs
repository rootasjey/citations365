using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace citations365.Models {
    public class SearchCollection : ObservableKeyedCollection{
        /// <summary>
        /// Collection's name 
        /// (used to save the collection as a file in the IO)
        /// </summary>
        public override string Name {
            get {
                return "SearchCollection.xml";
            }
        }

        private string _query = "";
        public string Query {
            get {
                return _query;
            }
            set {
                if (_query != value) {
                    _query = value;
                }
            }
        }

        public SearchCollection() {
            HasMoreItems = true; // initially to false
        }

        /// <summary>
        /// Build the url and run the fetch method
        /// </summary>
        /// <returns></returns>
        public override async Task<int> BuildAndFetch(string query = "") {
            string url = "http://evene.lefigaro.fr/citations/mot.php?mot=";
            string _pageQuery = "&p=";

            // Checks if this is a new search
            if (query != string.Empty && query != Query) {
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
                url =  RedirectedURL + _pageQuery + Page;

            } else {
                url = url + query + _pageQuery + Page;
            }

            return await Fetch(url);
        }
    }
}
