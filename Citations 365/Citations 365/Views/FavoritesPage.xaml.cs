using citations365.Controllers;
using citations365.Models;
using System;
using System.Numerics;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace citations365.Views {
    public sealed partial class FavoritesPage : Page {
        private static FavoritesController _FController;
        public static FavoritesController FController {
            get {
                if (_FController == null) {
                    _FController = new FavoritesController();
                }
                return _FController;
            }
        }

        private int _animationDelay = 500;

        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            CoreWindow.GetForCurrentThread().KeyDown -= FavoritesPage_KeyDown;
            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            CoreWindow.GetForCurrentThread().KeyDown += FavoritesPage_KeyDown;
            base.OnNavigatedTo(e);
        }

        private void FavoritesPage_KeyDown(CoreWindow sender, KeyEventArgs args) {
            if (Controller.IsBackOrEscapeKey(args.VirtualKey) && Frame.CanGoBack) {
                Frame.GoBack();
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
            }

            if (FavoritesController.HasItems()) {
                NoContentView.Visibility = Visibility.Collapsed;
                ListQuotes.Visibility = Visibility.Visible;
            }
        }

        private void CheckEmptyView() {
            if (FavoritesController.HasItems()) {
                NoContentView.Visibility = Visibility.Collapsed;
                ListQuotes.Visibility = Visibility.Visible;

            } else {
                NoContentView.Visibility = Visibility.Visible;
                ListQuotes.Visibility = Visibility.Collapsed;
            }
        }

        /* ***************
         * EVENTS HANDLERS
         * ***************
         */
        private async void Favorite_Tapped(object sender, TappedRoutedEventArgs e) {
            FontIcon icon = (FontIcon)sender;
            Quote quote = (Quote)icon.DataContext;

            bool result = await FavoritesController.RemoveFavorite(quote, FavoritesController.CollectionType.favorites);
        }

        private async void ItemSwipeTriggerComplete(object sender, LLM.SwipeCompleteEventArgs args) {
            LLM.LLMListViewItem item = (LLM.LLMListViewItem)sender;
            Quote quote = (Quote)item.Content;

            if (args.SwipeDirection == LLM.SwipeDirection.Left) {
                quote.IsShared = false;
                Controller.share(quote);

            } else {
                if (FavoritesController.IsFavorite(quote.Link)) {
                    bool result = await FavoritesController.RemoveFavorite(quote);
                    if (result) {
                        quote.IsFavorite = false;
                    }
                    CheckEmptyView();

                } else {
                    bool result = await FavoritesController.AddFavorite(quote);
                    if (result) {
                        quote.IsFavorite = true;
                    }
                    CheckEmptyView();
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
