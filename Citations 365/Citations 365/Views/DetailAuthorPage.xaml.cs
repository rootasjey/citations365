using citations365.Controllers;
using citations365.Models;
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
using Windows.UI.Xaml.Navigation;

// Pour plus d'informations sur le modèle d'élément Page vierge, voir la page http://go.microsoft.com/fwlink/?LinkId=234238

namespace citations365.Views {
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class DetailAuthorPage : Page {
        private static DetailAuthorController _dAuthorController;
        public static DetailAuthorController DAuthorController {
            get {
                if (_dAuthorController == null) {
                    _dAuthorController = new DetailAuthorController();
                }
                return _dAuthorController;
            }
        }

        public DetailAuthorPage() {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            string name      = "", 
                   url       = "", 
                   imageLink = "";

            base.OnNavigatedTo(e);

            if (e.Parameter.GetType() == typeof(Author)) {
                Author author = (Author)e.Parameter;
                name = author.Name;
                url = author.Link;
                imageLink = author.ImageLink;

                Populate(name, url, imageLink);
            } 
            else if (e.Parameter.GetType() == typeof(Quote)) {
                Quote quote = (Quote)e.Parameter;
                name = quote.Author;
                url = quote.AuthorLink;

                Populate(name, url);
            }
            
        }

        private async void Populate(string name, string url, 
            string imageLink = "ms-appx:///Assets/Icons/gray.png") {

            PopulateHeader(name);
            AuthorInfos infos = await DAuthorController.LoadData(url);

            if (infos != null) {
                PopulateBio(infos, imageLink);
                ShowBio();
            }
        }

        private void PopulateHeader(string name) {
            //HeaderContent.Text = name;
        }

        private void PopulateBio(AuthorInfos infos, string imageLink) {
            ContentBio.Text = infos.bio;
            LifeTime.Text = infos.birth + " - " + infos.death;
            Job.Text = infos.job;
            MainQuote.Text = infos.quote;
            AuthorImage.UriSource = new Uri(imageLink);
        }

        private void ShowBio() {
            if (ContentBio.Text.Length < 1) {
                return;
            }
            ViewBio.Visibility = Visibility.Visible;
            NoContentViewBio.Visibility = Visibility.Collapsed;
        }

        private void HideBio() {
            ViewBio.Visibility = Visibility.Collapsed;
            NoContentViewBio.Visibility = Visibility.Visible;
        }

        private void ShowQuotesLoading() {
        }

        private void HideQuotesLoading() {
        }

        private void BindCollectionToView() {
            NoContentViewQuotes.Visibility = Visibility.Collapsed;
            ListQuotes.Visibility = Visibility.Visible;
            ListQuotes.ItemsSource = DAuthorController.AuthorQuotesCollection;
        }

        private async void PagePivot_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (PagePivot.SelectedIndex == 1) { // quotes pivot item
                PopulateQuotes();
            }
        }

        private async void PopulateQuotes() {
            if (DAuthorController.QuotesLoaded()) {
                return;
            }

            // Load quotes if there aren't
            if (DAuthorController.HasQuotes()) {
                ShowQuotesLoading();
                bool result = await DAuthorController.FetchQuotes();

                if (result) {
                    BindCollectionToView();
                }
                HideQuotesLoading();
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
    }
}
