using citations365.Controllers;
using citations365.Models;
using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace citations365.Views {
    public sealed partial class SearchPage : Page {
        private static SearchController _Scontroller;

        public static SearchController Scontroller {
            get {
                if (_Scontroller == null) {
                    _Scontroller = new SearchController();
                }
                return _Scontroller;
            }
        }

        private int _animationDelay = 500;

        private static IDictionary<string, string> _tips =
            new Dictionary<string, string>();

        private static IDictionary<string, string> _infos =
            new Dictionary<string, string>();

        /// <summary>
        /// Avoid running multiple search calls
        /// </summary>
        private bool _performingSearch = false;

        public SearchPage() {
            InitializeComponent();
            PopulatePage();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            ShowSearchResults();
            CoreWindow.GetForCurrentThread().KeyDown += SearchPage_KeyDown;
            base.OnNavigatedTo(e);
        }

        private void SearchPage_KeyDown(CoreWindow sender, KeyEventArgs args) {
            if (InputSearch.FocusState != FocusState.Unfocused) return;
            if (Controller.IsBackOrEscapeKey(args.VirtualKey) && Frame.CanGoBack) {
                Frame.GoBack();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            CoreWindow.GetForCurrentThread().KeyDown -= SearchPage_KeyDown;
            base.OnNavigatedFrom(e);
        }


        public async void PopulatePage() {
            await Scontroller.LoadData();
            BindCollectionToView();
            PopulateTextInfos();
        }

        private void PopulateTextInfos() {
            if (_tips.Count < 1) {
                _tips["default"] = "Recherchez des citations par mot-clés";
                _tips["resultats"] = "La dernière recherche est disponible en cliquant le bouton 'résultats' même après avoir changé de page";
                _tips["searchback"] = "Après avoir effectué une recherche, vous pouvez appuyer sur un des boutons retour pour afficher le champ de recherche à nouveau";
            }

            if (_infos.Count < 1) {
                _infos["searchFailed"] = "Désolé, nous n'avons trouvé aucune citation comportant ce mot clé :/";
                _infos["searching"] = "Patientez quelques instants...";
            }
        }

        private void BindCollectionToView() {
            ListQuotes.ItemsSource = SearchController.SearchCollection;
        }

        private async void RunSearch(string query) {
            ShowLoadingSearchScreen();
            bool result = await Scontroller.Search(query);

            if (result) {
                ShowSearchResults();
                ListQuotes.RefreshAreaHeight = 0; // HACK: hide refresh loading

            } else {
                NoContentView.Visibility = Visibility.Visible;
                ListQuotes.Visibility = Visibility.Collapsed;
                TextInfos.Text = _infos["searchFailed"];
            }

            _performingSearch = false;
        }

        private void ClearSearchInput() {
            InputSearch.Text = "";
        }

        private void ShowLoadingSearchScreen() {
            ListQuotes.Visibility = Visibility.Collapsed;
            InputSearch.Visibility = Visibility.Collapsed;
            TextInfos.Text = _infos["searching"];
        }

        private void HideLoadingSearchScreen() {
            ListQuotes.Visibility = Visibility.Visible;
            InputSearch.Visibility = Visibility.Visible;
        }

        private void ShowSearchInput() {
            if (NoContentView.Visibility == Visibility.Collapsed) {
                ListQuotes.Visibility = Visibility.Collapsed;
                NoContentView.Visibility = Visibility.Visible;
                TextInfos.Text = _tips["default"];
                InputSearch.Visibility = Visibility.Visible;

                AdaptCommandBar();

                // Auto focus
                FocusSearchInput();
            }
        }

        private void ShowSearchResults() {
            bool alreadyVisible =
                NoContentView.Visibility == Visibility.Collapsed &&
                ListQuotes.Visibility == Visibility.Visible;
            bool noResults = SearchController.SearchCollection.Count < 1;

            if (alreadyVisible || noResults) {
                return;
            }

            NoContentView.Visibility = Visibility.Collapsed;
            ListQuotes.Visibility = Visibility.Visible;

            AdaptCommandBar();
        }

        private void FocusSearchInput() {
            InputSearch.Focus(FocusState.Programmatic);
        }

        private void StarTipsSlideShow() {

        }

        private void StopTipsSlideShow() {

        }

        private void AdaptCommandBar() {
            bool resultsAreVisible = ListQuotes.Visibility == Visibility.Visible;
            if (resultsAreVisible) {
                CmdResults.Visibility = Visibility.Collapsed;
                CmdSearch.Visibility = Visibility.Visible;

            } else {
                CmdResults.Visibility = Visibility.Visible;
                CmdSearch.Visibility = Visibility.Collapsed;
            }
        }

        /* ******
         * EVENTS
         * ******
         */
        private void InputSearch_Loaded(object sender, RoutedEventArgs e) {
            FocusSearchInput();
        }

        private void InputSearch_KeyDown(object sender, KeyRoutedEventArgs e) {
            if (e.Key == Windows.System.VirtualKey.Enter && !_performingSearch) {
                _performingSearch = true;

                string query = InputSearch.Text;
                RunSearch(query);
            }
            //e.Handled = true;
        }

        private void ResultsButton_Click(object sender, RoutedEventArgs e) {
            ShowSearchResults();
        }

        private void NewSearchButton_Click(object sender, RoutedEventArgs e) {
            ClearSearchInput();
            ShowSearchInput();
        }

        private void InputSearch_GotFocus(object sender, RoutedEventArgs e) {
            TextBox inputSearch = (TextBox)sender;
            if (inputSearch.FocusState == FocusState.Unfocused) {
                FocusSearchInput();
                System.Diagnostics.Debug.Write("input focused");
            }
        }

        private void Quote_Tapped(object sender, TappedRoutedEventArgs e) {
            StackPanel panel = (StackPanel)sender;
            Quote quote = (Quote)panel.DataContext;

            if (quote.AuthorLink != null && quote.AuthorLink.Length > 0) {
                Frame.Navigate(typeof(DetailAuthorPage), quote, new DrillInNavigationTransitionInfo());
            }
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
            Quote quote = (Quote)e.ClickedItem;

            if (quote.AuthorLink != null && quote.AuthorLink.Length > 0) {
                Frame.Navigate(typeof(DetailAuthorPage), quote, new DrillInNavigationTransitionInfo());
            }
        }

        private void Quote_Loaded(object sender, RoutedEventArgs e) {
            var grid = (StackPanel)sender;

            var visual = ElementCompositionPreview.GetElementVisual(grid);
            var compositor = visual.Compositor;

            var slideUpAnimation = compositor.CreateVector2KeyFrameAnimation();
            slideUpAnimation.InsertKeyFrame(0.0f, new Vector2(0f, 100f));
            slideUpAnimation.InsertKeyFrame(1.0f, new Vector2(0, 0));
            slideUpAnimation.Duration = TimeSpan.FromMilliseconds(_animationDelay);
            visual.StartAnimation("Offset.xy", slideUpAnimation);

            _animationDelay += 200;
        }
    }
}
