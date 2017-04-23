using citations365.Models;
using System.Collections.ObjectModel;

namespace citations365.Data {
    public static class Quotes {
        public static ObservableCollection<Quote> Recent { get; set; }
        public static ObservableCollection<Quote> Favorites { get; set; }
        public static ObservableCollection<Quote> Search { get; set; }
        
        public static bool IsFavorite(string quoteLink) {
            return false;
        }

        public static bool IsFavorite(Quote quote) {
            return false;
        }

        public static void SaveRecentOFfline() {

        }

        public static void LoadRecentOffline() {

        }

        public static void SaveFavorites() {

        }

        public static void LoadFavorites() {

        }
    }
}
