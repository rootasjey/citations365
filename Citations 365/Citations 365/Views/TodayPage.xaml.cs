using citations365.Controllers;
using citations365.Models;
using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Numerics;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using citations365.Helpers;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Core;

// Pour plus d'informations sur le modèle d'élément Page vierge, 
// voir la page http://go.microsoft.com/fwlink/?LinkId=234238
namespace citations365.Views {
    public sealed partial class TodayPage : Page
    {
        private static TodayController _Tcontroller;

        public static TodayController Tcontroller {
            get {
                if (_Tcontroller == null) {
                    _Tcontroller = new TodayController();

                    // Event will be added once
                    Window.Current.VisibilityChanged += WindowVisibilityChangedEventHandler;
                }
                return _Tcontroller;
            }
        }

        private Visual _backgroundVisual;
        private Compositor _backgroundCompositor;
        private ScrollViewer _ListQuotesScrollViewer;
        private CompositionPropertySet _ListQuotesScrollerPropertySet;

        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            CoreWindow.GetForCurrentThread().KeyDown -= TodayPage_KeyDown;
            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            CoreWindow.GetForCurrentThread().KeyDown += TodayPage_KeyDown;
            base.OnNavigatedTo(e);
        }

        private void TodayPage_KeyDown(CoreWindow sender, KeyEventArgs args) {
            if (Controller.IsBackOrEscapeKey(args.VirtualKey) && Frame.CanGoBack) {
                Frame.GoBack();
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
        }

        private void BindCollectionToView() {
            if (Tcontroller.IsDataLoaded()) {
                ListQuotes.ItemsSource = TodayController.TodayCollection;
                ShowListQuotes();
                //Controller.UpdateTile(TodayController.TodayCollection[0]);

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
                var bitmap = new BitmapImage(new Uri(url));
                ParallaxImage.Source = bitmap;
            }
        }

        /* ***************
         * EVENTS HANDLERS
         * ***************
         */
        private void ParallaxImage_Loaded(object sender, RoutedEventArgs e) {
            _backgroundVisual = ElementCompositionPreview.GetElementVisual(ParallaxImage);
            _backgroundCompositor = _backgroundVisual.Compositor;
        }

        private void ListQuotes_Loaded(object sender, RoutedEventArgs e) {
            _ListQuotesScrollViewer = ListQuotes.GetChildOfType<ScrollViewer>();
            _ListQuotesScrollerPropertySet = ElementCompositionPreview.
                    GetScrollViewerManipulationPropertySet(_ListQuotesScrollViewer);
        }

        /// <summary>
        ///  Perform operations that should take place when the application becomes visible rather than
        ///  when it is prelaunched, such as building a what's new feed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void WindowVisibilityChangedEventHandler(object sender, Windows.UI.Core.VisibilityChangedEventArgs e) {
            if (!e.Visible) { // app losing focus
                return;
            }

            TodayController.CheckHeroQuote();
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

        private void ListQuotes_ItemClick(object sender, ItemClickEventArgs e) {
            Quote quote = (Quote)e.ClickedItem;

            if (quote.AuthorLink != null && quote.AuthorLink.Length > 0) {
                Frame.Navigate(typeof(DetailAuthorPage), quote, new DrillInNavigationTransitionInfo());
            }
        }

        /// <summary>
        /// Initialize the Hero Quote
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ListQuotes_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args) {
            if (args.ItemIndex == 0) { // handle only the 1st item
                InitializeHeroQuote(args);
                ListQuotes.ContainerContentChanging -= ListQuotes_ContainerContentChanging;
            }
        }

        /// <summary>
        /// Attach animations to the background when it's been loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ParallaxImage_ImageOpened(object sender, RoutedEventArgs e) {
            var animation = _backgroundCompositor.CreateScalarKeyFrameAnimation();
            var easing = _backgroundCompositor.CreateLinearEasingFunction();
            animation.InsertKeyFrame(0.0f, 0.0f);
            animation.InsertKeyFrame(1.0f, 0.5f, easing);
            animation.Duration = TimeSpan.FromSeconds(3);

            ParallaxImage.Opacity = 1; // to see the animation
            _backgroundVisual.StartAnimation(nameof(_backgroundVisual.Opacity), animation);

            AttachBackgroundBlurAnimation();
            AttachBackgroundParallax();
        }


        /* ************************
         * VISUAL SWYPE (ITEM MOVE)
         * ************************
         */
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

        private void InitializeHeroQuote(ContainerContentChangingEventArgs visual) {
            if (Application.Current.Resources.ContainsKey("HeroQuoteTemplate")) {
                DataTemplate heroTemplate = (DataTemplate)Application.Current.Resources["HeroQuoteTemplate"];
                visual.ItemContainer.ContentTemplate = heroTemplate;
            }
        }

        /* ************
         * Composition
         * ************
         */
        private void AttachBackgroundParallax() {
            double backgroundOffset = Math.Round(ParallaxImage.ActualHeight - ParallaxCanvas.ActualHeight);
            string maxOffsetBottomToUp = backgroundOffset.ToString();

            var expression = _backgroundCompositor.CreateExpressionAnimation();
            expression.Expression = string.Format("Clamp(scroller.Translation.Y * parallaxFactor, -{0}, 999)", maxOffsetBottomToUp);
            expression.SetScalarParameter("parallaxFactor", 0.03f);
            expression.SetReferenceParameter("scroller", _ListQuotesScrollerPropertySet);

            _backgroundVisual.StartAnimation("Offset.Y", expression);
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

            ExpressionAnimation backgroundBlurAnimation = _backgroundCompositor.CreateExpressionAnimation(
                "Clamp(-scroller.Translation.Y / 10,0,20)");
            backgroundBlurAnimation.SetReferenceParameter("scroller", _ListQuotesScrollerPropertySet);

            blurSprite.Brush.Properties.StartAnimation("Blur.BlurAmount", backgroundBlurAnimation);
        }
    }
}
