using citations365.Controllers;
using System.Threading.Tasks;

namespace citations365.Models {
    public class AuthorQuotesCollection : ObservableKeyedCollection {
        private string _query { get; set; }

        public AuthorQuotesCollection() {
            HasMoreItems = true;
        }

        public override async Task<int> BuildAndFetch(string query = "") {
            // Checks if this is a new search
            if (query != string.Empty && query != _query) {
                Page = 1;
                HasMoreItems = true;
                RedirectedURL = "";
                Clear();
            }

            _query = query.Length > 0 ? query : _query;

            var url = _query + "?page=" + Page;
            return await Fetch(url);
        }

        public override bool IsFavorite(Quote quote) {
            return FavoritesController.IsFavorite(quote);
        }
    }
}
