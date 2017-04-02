using citations365.Controllers;
using citations365.Models;
using Microsoft.Toolkit.Uwp.UI.Animations;
using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.Foundation;
using Windows.UI.Composition;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// Pour plus d'informations sur le modèle d'élément Page vierge, voir la page http://go.microsoft.com/fwlink/?LinkId=234238

namespace citations365.Views {
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

        private int _animationDelay = 500;

        public DetailAuthorPage() {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            CoreWindow.GetForCurrentThread().KeyDown += DetailAuthorPage_KeyDown;

            string name = "",
                   url = "",
                   imageLink = "";

            base.OnNavigatedTo(e);

            if (e.Parameter.GetType() == typeof(Author)) {
                Author author = (Author)e.Parameter;
                name = author.Name;
                url = author.Link;
                imageLink = author.ImageLink;

                GetPageData(name, url);
            } else if (e.Parameter.GetType() == typeof(Quote)) {
                Quote quote = (Quote)e.Parameter;
                name = quote.Author;
                url = quote.AuthorLink;

                GetPageData(name, url);
            } else if (e.Parameter.GetType() == typeof(Dictionary<string, object>)) {
                var payload = (Dictionary<string, object>)e.Parameter;
                if (payload["AuthorPayload"].GetType() == typeof(Author)) {
                    Author author = (Author)payload["AuthorPayload"];
                    name = author.Name;
                    url = author.Link;
                    //imageLink = author.ImageLink;

                    GetPageData(name, url);
                }

                var EllipseAuthorCoords = (Point)payload["EllipseAuthorCoords"];
                EllipseAuthor.Offset((float)EllipseAuthorCoords.X, (float)EllipseAuthorCoords.Y, 0, 0).Start();
            }

            //var animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("EllipseAuthor");
            //if (animation != null) {
            //    //EllipseAuthor.Opacity = 0;
            //    EllipseAuthor.Loaded += (sender, ev) => {
            //        //EllipseAuthor.Opacity = 1;
            //        animation.TryStart(EllipseAuthor);
            //    };
            //}
        }

        private void DetailAuthorPage_KeyDown(CoreWindow sender, KeyEventArgs args) {
            if (Controller.IsBackOrEscapeKey(args.VirtualKey) && Frame.CanGoBack) {
                Frame.GoBack();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            base.OnNavigatedFrom(e);
            CoreWindow.GetForCurrentThread().KeyDown -= DetailAuthorPage_KeyDown;
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
            StartAuthorAnimation();
        }

        void StartAuthorAnimation() {
            Job.Offset(offsetX: 0, offsetY: 20, duration: 300, delay: 0).Start();
            LifeTime.Offset(offsetX: 0, offsetY: 20, duration: 500, delay: 0).Start();
            MainQuote.Offset(offsetX: 0, offsetY: 20, duration: 800, delay: 0).Start();
            Biography.Offset(offsetX: 0, offsetY: 50, duration: 1000, delay: 0).Start();
            EllipseAuthor.Offset(offsetX: 0, offsetY: 0, duration: 1000, delay: 0).Start();
        }

        private void BindCollectionToView() {
            _isQuotesLoaded = true;

            NoContentViewQuotes.Visibility = Visibility.Collapsed;
            ListQuotes.Visibility = Visibility.Visible;
            ListQuotes.ItemsSource = DAuthorController.AuthorQuotesCollection;
        }

        private void PagePivot_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (PagePivot.SelectedIndex == 1) {
                PopulateQuotes();
            }
        }

        private void PopulateQuotes() {
            if (_isQuotesLoaded) return;

            if (DAuthorController.QuotesLoaded() && DAuthorController.isSameRequest()) {
                BindCollectionToView();
                return;
            }

            if (!DAuthorController.HasQuotes()) return;

            var resAsync = Dispatcher.RunAsync(CoreDispatcherPriority.Low, async () => {
                ShowQuotesLoadingIndicator();
                bool result = await DAuthorController.FetchQuotes();
                HideQuotesLoadingIndicator();

                if (result) BindCollectionToView();
                else ShowNoQuotesView();
            });
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
