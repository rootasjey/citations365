using BackgroundTasks.Controllers;
using BackgroundTasks.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// Pour plus d'informations sur le modèle d'élément Page vierge, voir la page http://go.microsoft.com/fwlink/?LinkId=234238

namespace BackgroundTasks.Views
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class FavoritesPage : Page
    {
        private static FavoritesController _FController;
        public static FavoritesController FController {
            get {
                if (_FController == null) {
                    _FController = new FavoritesController();
                }
                return _FController;
            }
        }

        public FavoritesPage() {
            InitializeComponent();
            Populate();
        }

        private async void Populate() {
            await FavoritesController.Initialize();
            BindCollectionToView();
        }

        private void BindCollectionToView() {
            if (FavoritesController.IsDataLoaded()) {
                ListQuotes.ItemsSource = FavoritesController.FavoritesCollection;
                NoContentView.Visibility = Visibility.Collapsed;
                ListQuotes.Visibility = Visibility.Visible;
            }
        }

        private async void Favorite_Tapped(object sender, TappedRoutedEventArgs e) {
            FontIcon icon = (FontIcon)sender;
            Quote quote = (Quote)icon.DataContext;

            bool result = await FavoritesController.RemoveFavorite(quote, FavoritesController.CollectionType.favorites);
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
