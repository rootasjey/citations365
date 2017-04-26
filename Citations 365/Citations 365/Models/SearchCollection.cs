using citations365.Controllers;
using System.Threading.Tasks;

namespace citations365.Models {
    public class SearchCollection : ObservableKeyedCollection{
        public override string Name {
            get {
                return "SearchCollection.xml";
            }
        }
        
        public SearchCollection() {
            HasMoreItems = true; // initially to false
        }
        
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

        public override bool IsFavorite(Quote quote) {
            return FavoritesController.IsFavorite(quote);
        }
    }
}
