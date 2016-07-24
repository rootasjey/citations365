using citations365.Controllers;
using citations365.Models;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
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

        private bool _isQuotesLoaded { get; set; }

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

                GetPageData(name, url);
            } 
            else if (e.Parameter.GetType() == typeof(Quote)) {
                Quote quote = (Quote)e.Parameter;
                name = quote.Author;
                url = quote.AuthorLink;

                GetPageData(name, url);
            }
        }

        private async void GetPageData(string name, string url) {
            ShowAuthorBioLoadingIndicator();

            PopulateHeader(name);
            AuthorInfos infos = await DAuthorController.LoadData(url);
            BindAuthorDataContext(infos);

            HideAuthorBioLoadingIndicator();
        }

        private void PopulateHeader(string name) {
            var _name = name.Replace("De ", "").ToUpper();
            App._shell.SetHeaderTitle(_name);
        }

        private void BindAuthorDataContext(AuthorInfos authorInfos) {
            AuthorInfos.DataContext = authorInfos;
        }

        private void BindCollectionToView() {
            _isQuotesLoaded = true;

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
            if (_isQuotesLoaded) {
                return;
            }

            if (DAuthorController.QuotesLoaded() && DAuthorController.isSameRequest()) {
                BindCollectionToView();
                return;
            }

            if (DAuthorController.HasQuotes()) {
                ShowQuotesLoadingIndicator();
                bool result = await DAuthorController.FetchQuotes();

                HideQuotesLoadingIndicator();
                if (result) {
                    BindCollectionToView();

                } else {
                    ShowNoQuotesView();
                }
            }
        }

        private void ShowQuotesLoadingIndicator() {
            NoContentViewQuotes.Visibility = Visibility.Collapsed;
            RingLoadingQuotes.IsActive = true;
            RingLoadingQuotes.Visibility = Visibility.Visible;
        }

        private void HideQuotesLoadingIndicator() {
            RingLoadingQuotes.IsActive = false;
            RingLoadingQuotes.Visibility = Visibility.Collapsed;
        }

        private void ShowNoQuotesView() {
            NoContentViewQuotes.Visibility = Visibility.Visible;
            ListQuotes.Visibility = Visibility.Collapsed;
        }

        private void ShowAuthorBioLoadingIndicator() {
            RingLoadingAuthorBio.IsActive = true;
            RingLoadingAuthorBio.Visibility = Visibility.Visible;
        }

        private void HideAuthorBioLoadingIndicator() {
            RingLoadingAuthorBio.IsActive = false;
            RingLoadingAuthorBio.Visibility = Visibility.Collapsed;
        }


        private async void Favorite_Tapped(object sender, TappedRoutedEventArgs e) {
            FontIcon icon = (FontIcon)sender;
            Quote quote = (Quote)icon.DataContext;

            if (FavoritesController.IsFavorite(quote.Link)) {
                // Remove from favorites
                bool result = await FavoritesController.RemoveFavorite(quote);
                if (result) {
                    quote.IsFavorite = false;
                }
            } else {
                // Add to favorites
                bool result = await FavoritesController.AddFavorite(quote);
                if (result) {
                    quote.IsFavorite = true;
                }
            }
        }

        private async void ItemSwipeTriggerComplete(object sender, LLM.SwipeCompleteEventArgs args) {
            LLM.LLMListViewItem item = (LLM.LLMListViewItem)sender;
            Quote quote = (Quote)item.Content;

            if (args.SwipeDirection == LLM.SwipeDirection.Left) {
                quote.IsShared = false;
                Controller.share(quote);

            } else {
                // Favorite/Un-Favorite
                if (FavoritesController.IsFavorite(quote.Link)) {
                    // Remove from favorites
                    bool result = await FavoritesController.RemoveFavorite(quote);
                    if (result) {
                        quote.IsFavorite = false;
                    }
                } else {
                    // Add to favorites
                    bool result = await FavoritesController.AddFavorite(quote);
                    if (result) {
                        quote.IsFavorite = true;
                    }
                }
            }
        }

        private void ItemSwipeTriggerInTouch(object sender, LLM.SwipeTriggerEventArgs args) {
            var quote = (sender as LLM.LLMListViewItem).Content as Quote;

            if (args.SwipeDirection == LLM.SwipeDirection.Left) {
                quote.IsShared = args.IsTrigger;

            } else {
                quote.IsFavorite = FavoritesController.IsFavorite(quote) ? !args.IsTrigger : args.IsTrigger;
            }
        }

        /* ************************
         * VISUAL SWYPE (ITEM MOVE)
         * ************************
         */
        private void ItemSwipeProgressInTouch(object sender, LLM.SwipeProgressEventArgs args) {
            if (args.SwipeDirection == LLM.SwipeDirection.None)
                return;

            var panel = Controller.Getpanel(sender, args.SwipeDirection);
            Controller.SwipeMovePanel(panel, args);
        }

        private void ItemSwipeBeginTrigger(object sender, LLM.SwipeReleaseEventArgs args) {
            if (args.SwipeDirection == LLM.SwipeDirection.None)
                return;

            var panel = Controller.Getpanel(sender, args.SwipeDirection);
            Controller.SwipeReleasePanel(panel, args);
        }

        private void ItemSwipeBeginRestore(object sender, LLM.SwipeReleaseEventArgs args) {
            if (args.SwipeDirection == LLM.SwipeDirection.None)
                return;

            var panel = Controller.Getpanel(sender, args.SwipeDirection);
            Controller.SwipeReleasePanel(panel, args);
        }

        private void ListQuotes_ItemClick(object sender, ItemClickEventArgs e) {

        }
    }
}
