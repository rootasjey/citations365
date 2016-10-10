using citations365.Controllers;
using System.Threading.Tasks;

namespace citations365.Models {
    public class TodayCollection : ObservableKeyedCollection {
        /// <summary>
        /// Collection's name 
        /// (used to save the collection as a file in the IO)
        /// </summary>
        public override string Name {
            get {
                return "TodayCollection.xml";
            }
        }

        public TodayCollection() {
            HasMoreItems = true; // initially to false
        }

        /// <summary>
        /// Build the url and run the fetch method
        /// </summary>
        /// <returns></returns>
        public override async Task<int> BuildAndFetch(string query = "") {
            if (query.Length > 0) {
                return await Fetch(query);
            }

            string url = "http://evene.lefigaro.fr/citations/citation-jour.php?page=";

            if (Page < 2) {
                url = url.Substring(0, (url.Length - 6));
            } else {
                url = url + Page;
            }

            return await Fetch(url);
        }

        public override bool IsFavorite(Quote quote) {
            return FavoritesController.IsFavorite(quote);
        }

        /// <summary>
        /// Get the collection from the IO if the fetch failed
        /// </summary>
        /// <returns></returns>
        public override Task<bool> handleFailedFetch() {
            return base.handleFailedFetch();
        }
    }
}
