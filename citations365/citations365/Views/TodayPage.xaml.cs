﻿using citations365.Controllers;
using citations365.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

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

        /// <summary>
        /// Page Constructor
        /// </summary>
        public TodayPage() {
            this.InitializeComponent();
            Populate();
        }

        /// <summary>
        /// Fill the page with data
        /// </summary>
        public async void Populate() {
            await Tcontroller.LoadData();
            BindCollectionToView();
        }

        private void BindCollectionToView() {
            // Set the binding, show the list
            if (Tcontroller.IsDataLoaded()) {
                ListQuotes.ItemsSource = TodayController.TodayCollection;

                NoContentView.Visibility = Visibility.Collapsed;
                ListQuotes.Visibility = Visibility.Visible;
            }
        }

        private async void Favorite_Tapped(object sender, TappedRoutedEventArgs e) {
            FontIcon icon = (FontIcon)sender;
            Quote quote = (Quote)icon.DataContext;

            if (FavoritesController.IsFavorite(quote.Link)) {
                // Remove from favorites
                bool result = await FavoritesController.RemoveFavorite(quote);
                if (result) {
                    quote.IsFavorite = Quote.UnFavoriteIcon;
                }
            } else {
                // Add to favorites
                bool result = await FavoritesController.AddFavorite(quote);
                if (result) {
                    quote.IsFavorite = Quote.FavoriteIcon;
                }
            }
        }

        private void Quote_Tapped(object sender, TappedRoutedEventArgs e) {
            StackPanel panel = (StackPanel)sender;
            Quote quote = (Quote)panel.DataContext;

            if (quote.AuthorLink != null && quote.AuthorLink.Length > 0) {
                Frame.Navigate(typeof(DetailAuthorPage), quote, new DrillInNavigationTransitionInfo());
            }
        }
    }
}
