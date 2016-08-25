using citations365.Controllers;
using citations365.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;

// Pour plus d'informations sur le modèle d'élément Page vierge, voir la page http://go.microsoft.com/fwlink/?LinkId=234238
namespace citations365.Views {
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class TodayPage : Page
    {
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
            InitializeComponent();
            PopulatePage();
        }
        
        public async void PopulatePage() {
            ShowLoadingQuotesIndicator();

            await Tcontroller.LoadData();
            BindCollectionToView();

            HideLoadingQuotesIndicator();

            RefreshBackground();

            //PopulateHeroQuote();
        }

        private void BindCollectionToView() {
            if (Tcontroller.IsDataLoaded()) {
                ListQuotes.ItemsSource = TodayController.TodayCollection;
                ShowListQuotes();
                Controller.UpdateTile(TodayController.TodayCollection[0]);

            } else {
                ShowNoQuotesView();
            }
        }

        private void ShowLoadingQuotesIndicator() {
            ViewLoadingQuote.Visibility = Visibility.Visible;
            RingLoadingQuotes.IsActive = true;
            RingLoadingQuotes.Visibility = Visibility.Visible;
        }

        private void HideLoadingQuotesIndicator() {
            ViewLoadingQuote.Visibility = Visibility.Collapsed;
            RingLoadingQuotes.IsActive = false;
            RingLoadingQuotes.Visibility = Visibility.Collapsed;
        }

        private void ShowNoQuotesView() {
            ListQuotes.Visibility = Visibility.Collapsed;
            NoContentView.Visibility = Visibility.Visible;
        }

        private void ShowListQuotes() {
            ListQuotes.Visibility = Visibility.Visible;
            NoContentView.Visibility = Visibility.Collapsed;
        }

        private async void RefreshBackground() {
            string url = await Tcontroller.GetAppBackgroundURL();

            if (!string.IsNullOrEmpty(url)) {
                var bitmap = new BitmapImage(new System.Uri(url));
                //PageBackground.UriSource = new System.Uri(url);
                ParallaxImage.Source = bitmap;
            }
        }

        private void PopulateHeroQuote() {
            var item = ListQuotes.Items[0];
        }

        /* ***************
         * EVENTS HANDLERS
         * ***************
         */
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

        private void Quote_Tapped(object sender, TappedRoutedEventArgs e) {
            StackPanel panel = (StackPanel)sender;
            Quote quote = (Quote)panel.DataContext;

            if (quote.AuthorLink != null && quote.AuthorLink.Length > 0) {
                Frame.Navigate(typeof(DetailAuthorPage), quote, new DrillInNavigationTransitionInfo());
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
            Quote quote = (Quote)e.ClickedItem;

            if (quote.AuthorLink != null && quote.AuthorLink.Length > 0) {
                Frame.Navigate(typeof(DetailAuthorPage), quote, new DrillInNavigationTransitionInfo());
            }
        }
    }
}
