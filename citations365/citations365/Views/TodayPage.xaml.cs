using citations365.Controllers;
using citations365.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// Pour plus d'informations sur le modèle d'élément Page vierge, voir la page http://go.microsoft.com/fwlink/?LinkId=234238
namespace citations365.Views {
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class TodayPage : Page
    {
        /// <summary>
        /// Today controller
        /// </summary>
        private static TodayController _Tcontroller;

        public static TodayController Tcontroller {
            get {
                if (_Tcontroller == null) {
                    _Tcontroller = new TodayController();
                }
                return _Tcontroller;
            }
        }

        public TodayPage() {
            this.InitializeComponent();
            populate();
        }

        /// <summary>
        /// Fill the page with data
        /// </summary>
        public async void populate() {
            await Tcontroller.LoadData();

            if (Tcontroller.IsDataLoaded()) {
                // Set the binding, show the list
                ListQuotes.ItemsSource = TodayController.TodayCollection;

                NoContentView.Visibility    = Visibility.Collapsed;
                ListQuotes.Visibility       = Visibility.Visible;
            } else {
                // Hide the list, show the empty view
            }
        }

        private async void Favorite_Tapped(object sender, TappedRoutedEventArgs e) {
            FontIcon icon = (FontIcon)sender;
            Quote quote = (Quote)icon.DataContext;

            bool result = await FavoritesController.AddFavorite(quote);
            if (result) {
                quote.IsFavorite = Quote.FavoriteIcon;
            }
        }
    }
}
