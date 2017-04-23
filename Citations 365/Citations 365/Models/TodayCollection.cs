using citations365.Controllers;
using System.Threading.Tasks;

namespace citations365.Models {
    public class TodayCollection : ObservableKeyedCollection {
        public override string Name {
            get {
                return "TodayCollection.xml";
            }
        }

        public TodayCollection() {
            HasMoreItems = true; // initially to false
        }
        
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
        
        public override Task<int> HandleFailedFetch() {
            return base.HandleFailedFetch();
        }
    }
}
