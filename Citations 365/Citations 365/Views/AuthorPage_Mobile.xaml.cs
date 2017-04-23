using citations365.Controllers;
using citations365.Helpers;
using citations365.Models;
using citations365.Services;
using Microsoft.Toolkit.Uwp.UI.Animations;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

namespace citations365.Views {
    public sealed partial class AuthorPage_Mobile : Page {
        private static DetailAuthorController _dAuthorController;
        public static DetailAuthorController DAuthorController {
            get {
                if (_dAuthorController == null) {
                    _dAuthorController = new DetailAuthorController();
                }
                return _dAuthorController;
            }
        }

        private Author _Author { get; set; }

        private bool _isQuotesLoaded { get; set; }

        private int _animationDelay = 500;


        public AuthorPage_Mobile() {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            CoreWindow.GetForCurrentThread().KeyDown += AuthorPage_KeyDown;

            Author author = null;

            if (e.Parameter.GetType() == typeof(Author)) {
                author = (Author)e.Parameter;
            } else if (e.Parameter.GetType() == typeof(Quote)) {

                Quote quote = (Quote)e.Parameter;

                author = new Author() {
                    Name = quote.Author.Replace("De ", ""),
                    Link = quote.AuthorLink
                };
            }

            _Author = author;
            //PopulatePage(author);

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            CoreWindow.GetForCurrentThread().KeyDown -= AuthorPage_KeyDown;
            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e) {
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("EllipseAuthor", EllipseAuthor);
            //ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("AuthorName", AuthorName);

            base.OnNavigatingFrom(e);
        }

        private void AuthorPage_KeyDown(CoreWindow sender, KeyEventArgs args) {
            if (Events.IsBackOrEscapeKey(args.VirtualKey) && Frame.CanGoBack) {
                Frame.GoBack();
            }
        }

        private void AuthorPivot_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var index = ((Pivot)sender).SelectedIndex;

            switch (index) {
                case 0:
                    LoadBiographyView();
                    break;
                case 1:
                    LoadQuotesView();
                    break;
                default:
                    break;
            }
        }

        void LoadBiographyView() {
            FindName("BiographyPivotContent");
        }

        void LoadQuotesView() {
            FindName("QuotesPivotContent");
        }

        private void BiographyPivotContent_Loaded(object sender, RoutedEventArgs e) {
            LoadAuthor(_Author);
        }

        private void QuotesPivotContent_Loaded(object sender, RoutedEventArgs e) {
            LoadAuthorQuotes();
        }

        void LoadAuthorQuotes() {
            if (!DAuthorController.HasQuotes()) return;

            var resAsync = Dispatcher.RunAsync(CoreDispatcherPriority.Low, async () => {
                await DAuthorController.FetchQuotes();

                HideLoadingView();

                if (DAuthorController.AuthorQuotesCollection.Count > 0) {
                    _isQuotesLoaded = true;
                    ListAuthorQuotes.ItemsSource = DAuthorController.AuthorQuotesCollection;
                } 
                else { ShowEmptyView(); }
            });

            void HideLoadingView()
            {
                QuotesLoadingView.Visibility = Visibility.Collapsed;
            }

            void ShowEmptyView()
            {
                QuotesEmptyView.Visibility = Visibility.Visible;
            }
        }

        private void LoadAuthor(Author author) {
            author.IsLoading = true;
            author.Biography = ""; // TODO: Language

            BindAuthorDataContext();
            InitializeAnimation();
            FetchOtherInformationAsync();

            async void FetchOtherInformationAsync()
            {
                Author authorFilled = await DAuthorController.LoadData(author.Link);
                authorFilled.IsLoading = false;

                author.Biography = authorFilled.Biography;
                author.Birth = authorFilled.Birth;
                author.Death = authorFilled.Death;
                author.LifeTime = authorFilled.LifeTime;
                author.Quote = authorFilled.Quote;
                author.IsLoading = authorFilled.IsLoading;

                HideLoadingView();
                StartAnimation();

                if(string.IsNullOrEmpty(author.Biography)) {
                    ShowEmptyView();
                }
            }

            void BindAuthorDataContext()
            {
                BiographyPivotContent.DataContext = author;
            }

            void HideLoadingView()
            {
                LoadingView.Visibility = Visibility.Collapsed;
            }

            void InitializeAnimation()
            {
                Job.Fade(0,0).Offset(0, 20, 0).Start();
                LifeTime.Fade(0, 0).Offset(0, 20, 0).Start();
                MainQuote.Fade(0, 0).Offset(0, 20, 0).Start();
                Biography.Fade(0, 0).Offset(0, 20, 0).Start();
            }

            void StartAnimation()
            {
                Job.Fade(1, 1000).Offset(0, 0, 1000).Start();
                LifeTime.Fade(1, 1000, 100).Offset(0, 0, 1000, 100).Start();
                MainQuote.Fade(1, 1000, 200).Offset(0, 0, 1000, 200).Start();
                Biography.Fade(1, 1000, 300).Offset(0, 0, 1000, 300).Start();
            }

            void ShowEmptyView()
            {
                EmptyView.Visibility = Visibility.Visible;
            }
        }

        private async void Quote_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e) {
            var panel = (Grid)sender;

            await panel.Fade(0, 0).Offset(0, 40, 0).StartAsync();
            panel.Fade(1, 1000, _animationDelay)
                 .Offset(0, 0, 1000, _animationDelay, EasingType.Quintic)
                 .Start();

            _animationDelay += 100;
        }

        private void EllipseAuthor_Loaded(object sender, RoutedEventArgs e) {
            var EllipseAuthor = (Ellipse)sender;
            var animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("EllipseAuthor");

            if (animation != null) animation.TryStart(EllipseAuthor);
        }

        private void AuthorName_Loaded(object sender, RoutedEventArgs e) {
            var animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("AuthorName");

            if (animation != null) animation.TryStart(AuthorName);
        }

        private void BackToTop_Tapped(object sender, TappedRoutedEventArgs e) {
            if (ListAuthorQuotes != null) VisualTreeExtensions.ScrollToIndex(ListAuthorQuotes, 0);
        }

        #region Commands
        private void ShareCommand_Tapped(object sender, TappedRoutedEventArgs e) {
            DataTransfer.Share((Quote)((MenuFlyoutItem)sender).DataContext);
        }

        private void FavCommand_Tapped(object sender, TappedRoutedEventArgs e) {
            var quote = (Quote)((MenuFlyoutItem)sender).DataContext;
            ToggleFavorite(quote);
        }

        private void CopyCommand_Tapped(object sender, TappedRoutedEventArgs e) {
            DataTransfer.Copy((Quote)((MenuFlyoutItem)sender).DataContext);
        }
        #endregion Commands


        async void ToggleFavorite(Quote quote) {
            if (FavoritesController.IsFavorite(quote.Link)) {
                bool result = await FavoritesController.RemoveFavorite(quote);
                if (result) {
                    quote.IsFavorite = false;
                }

            } else {
                bool result = await FavoritesController.AddFavorite(quote);
                if (result) {
                    quote.IsFavorite = true;
                }
            }
        }

    }
}
