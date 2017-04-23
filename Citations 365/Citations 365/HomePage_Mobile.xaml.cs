using citations365.Controllers;
using citations365.Helpers;
using citations365.Models;
using citations365.Services;
using citations365.Views;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Toolkit.Uwp.UI.Animations;
using System;
using System.Numerics;
using System.Threading.Tasks;
using Windows.UI.Composition;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace citations365 {
    public sealed partial class HomePage_Mobile : Page {
        public HomePage_Mobile() {
            InitializeComponent();
            HandleConnectedAnimation();
            HandleWindowVisibility();
        }

        void InitializeVariables() {
            _LastSelectedPivotItem = 0;
            _LastQuoteTapped = null;
            _PerformingSearch = false;
            _AnimationDelay = 0;
        }

        static int _LastSelectedPivotItem { get; set; }
        static Quote _LastQuoteTapped { get; set; }
        static bool _PerformingSearch { get; set; }
        static int _AnimationDelay { get; set; }

        private Visual _backgroundVisual;
        private Compositor _backgroundCompositor;
        private ScrollViewer _ListQuotesScrollViewer;
        private CompositionPropertySet _ListQuotesScrollerPropertySet;

        #region navigation

        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            CoreWindow.GetForCurrentThread().KeyDown -= Page_KeyDown;
            
            _LastSelectedPivotItem = HomePivot.SelectedIndex;

            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            CoreWindow.GetForCurrentThread().KeyDown += Page_KeyDown;

            RestorePivotPosition();

            base.OnNavigatedTo(e);
        }

        private void Page_KeyDown(CoreWindow sender, KeyEventArgs args) {
            if (InputSearch != null &&
                InputSearch.FocusState != Windows.UI.Xaml.FocusState.Unfocused) return;

            if (Events.IsBackOrEscapeKey(args.VirtualKey) && Frame.CanGoBack) {
                Frame.GoBack();
            }
        }


        #endregion navigation

        void HandleConnectedAnimation() {
            var animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("EllipseAuthor");
            if (animation != null) {
                animation.Cancel();
            }
        }

        void HandleWindowVisibility() {
            Window.Current.VisibilityChanged -= WindowVisibilityChangedEventHandler;
            Window.Current.VisibilityChanged += WindowVisibilityChangedEventHandler;
        }

        /// <summary>
        ///  Perform operations that should take place when the application becomes visible rather than
        ///  when it is prelaunched, such as building a what's new feed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void WindowVisibilityChangedEventHandler(object sender, VisibilityChangedEventArgs e) {
            if (!e.Visible) { // app losing focus
                return;
            }

            TodayController.CheckHeroQuote();
        }

        private void HomePivot_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var pivot = (Pivot)sender;

            switch (pivot.SelectedIndex) {
                case 0:
                    LoadRecentView();
                    LoadRecentBackground();
                    break;
                case 1:
                    LoadFavoritesView();
                    LoadFavoritesBackground();
                    break;
                case 2:
                    LoadSearchView();
                    LoadSearchBackground();
                    break;
                default:
                    break;
            }
        }
        

        void LoadRecentBackground() {
            if (ParallaxImage.Opacity != 0) return;
            AnimateBackgroundEnter();
        }

        void LoadFavoritesBackground() {
            AnimateBackgroundExit();
        }

        void LoadSearchBackground() {
            AnimateBackgroundExit();
        }

        private async void RecentPivotContent_Loaded(object sender, RoutedEventArgs e) {
            ShowLoadingView();

            await TodayController.LoadData();

            if (!TodayController.IsDataLoaded()) { ShowEmptyView(); return; }

            HideLoadingView();
            ListRecent.ItemsSource = TodayController.TodayCollection;

            RestoreListPosition(ListRecent);
            //LoadHeroSection();

            void ShowLoadingView() {
                ListRecent.Visibility = Visibility.Collapsed;
                AnimateLoadingViewEnter();

                async void AnimateLoadingViewEnter() {
                    RecentLoadingView.Visibility = Visibility.Visible;
                    await RecentLoadingText.Fade(0, 0).Offset(0, 30, 0).StartAsync();
                    await RecentProgress.Fade(0, 0).Offset(0, 30, 0).StartAsync();
                    RecentLoadingText.Fade(1, 500).Offset(0, 0, 500).Start();
                    RecentProgress.Fade(1, 500, 500).Offset(0, 0, 500, 500).Start();
                }
            }

            async Task HideLoadingView()
            {
                ListRecent.Visibility = Visibility.Visible;
                await AnimateLoadingViewExit();

                async Task AnimateLoadingViewExit()
                {
                    RecentProgress.Fade(0, 500).Offset(0, 30, 500).Start();
                    await RecentLoadingText.Fade(0, 500, 500).Offset(0, 30, 500, 500).StartAsync();
                    RecentLoadingView.Visibility = Visibility.Collapsed;
                }
            }

            void ShowEmptyView()
            {
                ListRecent.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                RecentEmptyView.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }

        private void SearchPivotContent_Loaded(object sender, RoutedEventArgs e) {
            // check results available
        }

        private async void FavoritesPivotContent_Loaded(object sender, RoutedEventArgs e) {
            await FavoritesController.Initialize();

            if (FavoritesController.IsDataLoaded()) {
                ListFavorites.ItemsSource = FavoritesController.FavoritesCollection;
            }

            if (FavoritesController.HasItems()) {
                FavoritesEmptyView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                ListFavorites.Visibility = Windows.UI.Xaml.Visibility.Visible;
                return;
            }

            ShowEmptyView();
            void ShowEmptyView()
            {
                FavoritesEmptyView.Visibility = Windows.UI.Xaml.Visibility.Visible;
                ListFavorites.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
        }

        void LoadRecentView() {
            FindName("RecentPivotContent");
        }

        void LoadFavoritesView() {
            FindName("FavoritesPivotContent");
        }

        void LoadSearchView() {
            FindName("SearchPivotContent");
        }

        private void ShowNoQuotesView() {
            ListRecent.Visibility = Visibility.Collapsed;
            RecentEmptyView.Visibility = Visibility.Visible;
        }

        private void ListQuotes_Loaded(object sender, RoutedEventArgs e) {
            _ListQuotesScrollViewer = ListRecent.GetChildOfType<ScrollViewer>();
            _ListQuotesScrollerPropertySet = ElementCompositionPreview.
                    GetScrollViewerManipulationPropertySet(_ListQuotesScrollViewer);
        }

        private async void Quote_Loaded(object sender, RoutedEventArgs e) {
            var panel = (Grid)sender;

            await panel.Fade(0, 0).Offset(0, 40, 0).StartAsync();
            panel.Fade(1, 500, _AnimationDelay)
                 .Offset(0, 0, 500, _AnimationDelay, EasingType.Quintic)
                 .Start();

            _AnimationDelay += 100;
        }

        private void Quote_Tapped(object sender, TappedRoutedEventArgs e) {
            var grid = (StackPanel)sender;
            var quote = (Quote)grid.DataContext;

            if (quote.AuthorLink != null && quote.AuthorLink.Length > 0) {
                UpdateLastSelectedRecent(quote);

                var AuthorName = (TextBlock)grid.FindName("AuthorName");
                if (AuthorName != null) {
                    ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("AuthorName", AuthorName);
                }

                Frame.Navigate(typeof(AuthorPage_Mobile), quote);
            }
        }

        void UpdateLastSelectedRecent(Quote quote) {
            _LastQuoteTapped = quote;
        }

        void RestorePivotPosition() {
            if (_LastSelectedPivotItem == 0) return;
            HomePivot.SelectedIndex = _LastSelectedPivotItem;
        }

        void RestoreListPosition(ListView listView) {
            if (_LastQuoteTapped == null) return;

            VisualTreeExtensions.ScrollIntoViewAsync(listView, _LastQuoteTapped);
            listView.UpdateLayout();

            var animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("AuthorName");

            if (animation == null) return;

            var container = (ListViewItem)listView.ContainerFromItem(_LastQuoteTapped);
            var root = (Grid)container.ContentTemplateRoot;
            var textAnimation = (TextBlock)root.FindName("AuthorName");

            animation.TryStart(textAnimation);
        }

        private void InputSearch_KeyDown(object sender, KeyRoutedEventArgs e) {
            if (e.Key == Windows.System.VirtualKey.Enter && !_PerformingSearch) {
                _PerformingSearch = true;

                string query = InputSearch.Text;
                RunSearch(query);
            }
        }

        private async void RunSearch(string query) {
            //ShowLoadingSearchScreen();
            bool result = await SearchController.Search(query);

            if (result) {
                ShowSearchResults();

            } else {
                SearchEmptyView.Visibility = Visibility.Visible;
                ListSearch.Visibility = Visibility.Collapsed;
            }

            _PerformingSearch = false;
        }

        private void ShowSearchResults() {
            bool alreadyVisible =
                (SearchEmptyView.Visibility == Visibility.Collapsed) &&
                (ListSearch.Visibility == Visibility.Visible);

            bool noResults = SearchController.SearchCollection.Count < 1;

            if (alreadyVisible || noResults) {
                return;
            }

            SearchEmptyView.Visibility = Visibility.Collapsed;
            ListSearch.Visibility = Visibility.Visible;
        }

        async void ToggleFavorite(Quote quote) {
            if (FavoritesController.IsFavorite(quote.Link)) {
                bool result = await FavoritesController.RemoveFavorite(quote);
                if (result) quote.IsFavorite = false;

                CheckFavoritesEmptyView();

            } else {
                bool result = await FavoritesController.AddFavorite(quote);
                if (result) quote.IsFavorite = true;

                CheckFavoritesEmptyView();
            }
        }

        private void CheckFavoritesEmptyView() {
            if (FavoritesController.HasItems()) {
                FavoritesEmptyView.Visibility = Visibility.Collapsed;
                ListFavorites.Visibility = Visibility.Visible;
                return;
            }

            FavoritesEmptyView.Visibility = Visibility.Visible;
            ListFavorites.Visibility = Visibility.Collapsed;
        }

        #region Commands
        private void ShareCommand_Tapped(object sender, TappedRoutedEventArgs e) {
            DataTransfer.Share((Quote)((MenuFlyoutItem)sender).DataContext);
        }

        private void FavCommand_Tapped(object sender, TappedRoutedEventArgs e) {
            ToggleFavorite((Quote)((MenuFlyoutItem)sender).DataContext);
        }

        private void CopyCommand_Tapped(object sender, TappedRoutedEventArgs e) {
            DataTransfer.Copy((Quote)((MenuFlyoutItem)sender).DataContext);
        }
        #endregion Commands

        #region appbar
        private void SettingsButton_Tapped(object sender, TappedRoutedEventArgs e) {
            Frame.Navigate(typeof(SettingsPage_Mobile));
        }

        private void AuthorsButton_Tapped(object sender, TappedRoutedEventArgs e) {
            Frame.Navigate(typeof(ListAuthorsPage));
        }
        #endregion appbar

        private async void ParallaxImage_Loaded(object sender, RoutedEventArgs e) {
            string path = Wallpaper.GetPath();
            path = string.IsNullOrEmpty(path) ? await Wallpaper.GetNew() : path;
            ParallaxImage.Source = new BitmapImage(new Uri(path));

            _backgroundVisual = ElementCompositionPreview.GetElementVisual(ParallaxImage);
            _backgroundCompositor = _backgroundVisual.Compositor;
        }

        private async void ParallaxImage_ImageOpened(object sender, RoutedEventArgs e) {
            await AnimateBackgroundEnter();
            AttachBackgroundBlurAnimation();
            AttachBackgroundParallax();
        }

        async Task AnimateBackgroundEnter() {
            await ParallaxImage
                .Scale(.9f, .9f, 0, 0, 0)
                .StartAsync();

            ParallaxImage.Visibility = Visibility.Visible;

            ParallaxImage
                .Fade(.5f, 1000)
                .Scale(1f, 1f, (float)ParallaxImage.Width / 2, (float)ParallaxImage.Height / 2, 1000)
                .Start();
        }

        void AnimateBackgroundExit() {
            if (ParallaxImage.Opacity == 0) return;

            ParallaxImage
                .Fade(0, 1000)
                .Scale(.8f, .8f, (float)ParallaxImage.Width / 2, (float)ParallaxImage.Height / 2)
                .Start();
        }

        private void AttachBackgroundParallax() {
            double backgroundOffset = Math.Round(ParallaxImage.ActualHeight - ParallaxCanvas.ActualHeight);
            string maxOffsetBottomToUp = backgroundOffset.ToString();

            var expression = _backgroundCompositor.CreateExpressionAnimation();
            expression.Expression = string.Format("Clamp(scroller.Translation.Y * parallaxFactor, -{0}, 999)", maxOffsetBottomToUp);
            expression.SetScalarParameter("parallaxFactor", 0.03f);
            expression.SetReferenceParameter("scroller", _ListQuotesScrollerPropertySet);
            _backgroundVisual.StartAnimation("Offset.Y", expression);

            //var expr = _backgroundCompositor.CreateExpressionAnimation();
            //expr.SetReferenceParameter("ScrollManipulation", _ListQuotesScrollerPropertySet);
            //expr.Expression = "1 / ScrollManipulation.Translation.Y";
            //_backgroundVisual.StartAnimation("Opacity", expr);

            //expr.Expression = "lerp(start, end, scroller.Translation.Y)";
            //expr.SetReferenceParameter("scroller", _ListQuotesScrollerPropertySet);
            //expr.SetScalarParameter("start", 0.0f);
            //expr.SetScalarParameter("end", 1f);
            //_backgroundVisual.StartAnimation("Opacity", expr);
        }

        private void AttachBackgroundBlurAnimation() {
            GaussianBlurEffect blurEffect = new GaussianBlurEffect() {
                Name = "Blur",
                BlurAmount = 20.0f,
                BorderMode = EffectBorderMode.Hard,
                Optimization = EffectOptimization.Speed,
                Source = new CompositionEffectSourceParameter("Backdrop")
            };

            var effectFactory = _backgroundCompositor.CreateEffectFactory(blurEffect, new[] { "Blur.BlurAmount" });
            var effectBrush = effectFactory.CreateBrush();

            var destinationBrush = _backgroundCompositor.CreateBackdropBrush();
            effectBrush.SetSourceParameter("Backdrop", destinationBrush);

            var blurSprite = _backgroundCompositor.CreateSpriteVisual();
            blurSprite.Size = new Vector2((float)ParallaxCanvas.ActualWidth, (float)ParallaxCanvas.ActualHeight);
            blurSprite.Brush = effectBrush;
            ElementCompositionPreview.SetElementChildVisual(ParallaxCanvas, blurSprite);

            ExpressionAnimation backgroundBlurAnimation = 
                _backgroundCompositor.CreateExpressionAnimation("Clamp(-scroller.Translation.Y / 10,0,20)");
            backgroundBlurAnimation.SetReferenceParameter("scroller", _ListQuotesScrollerPropertySet);

            blurSprite.Brush.Properties.StartAnimation("Blur.BlurAmount", backgroundBlurAnimation);
        }

        private void ListRecent_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args) {
            if (args.ItemIndex == 0) {
                InitializeHeroQuote(args);
                ListRecent.ContainerContentChanging -= ListRecent_ContainerContentChanging;
            }
        }

        private void InitializeHeroQuote(ContainerContentChangingEventArgs visual) {
            if (Application.Current.Resources.ContainsKey("HeroQuoteTemplate")) {
                //visual.ItemContainer.LayoutUpdated += ItemContainer_LayoutUpdated;
                //visual.ItemContainer.Opacity = 0;

                DataTemplate heroTemplate = (DataTemplate)Application.Current.Resources["HeroQuoteTemplate"];
                visual.ItemContainer.ContentTemplate = heroTemplate;                
            }
            
            //async void ItemContainer_LayoutUpdated(object sender, object e)
            //{
            //    visual.ItemContainer.LayoutUpdated -= ItemContainer_LayoutUpdated;

            //    await visual.ItemContainer.Offset(0, -20, 0).StartAsync();
            //    visual.ItemContainer
            //        .Fade(1, 1000, 1000)
            //        .Offset(0,0, 1000, 1000)
            //        .Start();
            //}
        }

        private void RecentPivotHeader_Tapped(object sender, TappedRoutedEventArgs e) {
            ListRecent.ScrollToIndex(0);
        }

        private void FavoritesPivotHeader_Tapped(object sender, TappedRoutedEventArgs e) {
            ListFavorites.ScrollToIndex(0);
        }

        private void SearchPivotHeader_Tapped(object sender, TappedRoutedEventArgs e) {
            ListSearch.ScrollToIndex(0);
        }
    }
}
