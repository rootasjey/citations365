using citations365.Data;
using citations365.Helpers;
using citations365.Models;
using citations365.Services;
using Microsoft.Toolkit.Uwp.UI.Animations;
using System;
using System.ComponentModel;
using System.Numerics;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

namespace citations365.Views {
    public sealed partial class AuthorPage_Desktop : Page {
        private bool _isQuotesLoaded { get; set; }

        private int _animationDelay = 500;

        ListView _AuthorQuotes = null;
        ListView AuthorQuotes {
            get {
                if (_AuthorQuotes == null) {
                    _AuthorQuotes = (ListView)UI.FindChildControl<ListView>(QuotesSection, "ListQuotes");
                }
                return _AuthorQuotes;
            }
            set {
                if (_AuthorQuotes != value) {
                    _AuthorQuotes = value;
                }
            }
        }

        private SourceModel PageDataSource { get; set; }

        public AuthorPage_Desktop() {
            InitializeComponent();
            PageDataSource = App.DataSource;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            CoreWindow.GetForCurrentThread().KeyDown += DetailAuthorPage_KeyDown;

            Author author = null;

            if (e.Parameter.GetType() == typeof(Author)) {
                author = (Author)e.Parameter;
            } 
            else if (e.Parameter.GetType() == typeof(Quote)) {

                Quote quote = (Quote)e.Parameter;

                author = new Author() {
                    Name = quote.Author.Replace("De ", ""),
                    Link = quote.AuthorLink
                };
            }

            PageDataSource.AuthorLoaded = false;
            PopulatePage(author);

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            CoreWindow.GetForCurrentThread().KeyDown -= DetailAuthorPage_KeyDown;
            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e) {
            var EllipseAuthor = (Ellipse)UI.FindChildControl<Ellipse>(HeroSection, "EllipseAuthor");
            if (EllipseAuthor != null) {
                ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("EllipseAuthor", EllipseAuthor);
            }

            base.OnNavigatingFrom(e);
        }


        private void DetailAuthorPage_KeyDown(CoreWindow sender, KeyEventArgs args) {
            if (Events.IsBackOrEscapeKey(args.VirtualKey) && Frame.CanGoBack) {
                Frame.GoBack();
            }
        }

        #region LoadEvents
        private void Quote_Loaded(object sender, RoutedEventArgs e) {
            var grid = (Grid)sender;

            var visual = ElementCompositionPreview.GetElementVisual(grid);
            var compositor = visual.Compositor;

            var slideUpAnimation = compositor.CreateVector2KeyFrameAnimation();
            slideUpAnimation.InsertKeyFrame(0.0f, new Vector2(0f, 100f));
            slideUpAnimation.InsertKeyFrame(1.0f, new Vector2(0, 0));
            slideUpAnimation.Duration = TimeSpan.FromMilliseconds(_animationDelay);
            visual.StartAnimation("Offset.xy", slideUpAnimation);

            _animationDelay += 200;
        }

        private void EllipseAuthor_Loaded(object sender, RoutedEventArgs e) {
            var EllipseAuthor = (Ellipse)sender;
            var animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("EllipseAuthor");

            if (animation != null) animation.TryStart(EllipseAuthor);
        }

        private void AuthorHeader_Loaded(object sender, RoutedEventArgs e) {
            StartAuthorAnimation((StackPanel)sender);
        }

        private void ListQuotes_Loaded(object sender, RoutedEventArgs ev) {
            var ListQuotes = (ListView)sender;
            AuthorQuotes = ListQuotes;
            
            if (PageDataSource.AuthorLoaded) {
                GetQuotes(ListQuotes);
                return;
            }

            PropertyChangedEventHandler quotesLinkReady = null;
            quotesLinkReady = (s, e) => {
                GetQuotes(ListQuotes);
                PageDataSource.PropertyChanged -= quotesLinkReady;
            };

            PageDataSource.PropertyChanged += quotesLinkReady;
        }

        #endregion LoadEvents

        void GetQuotes(ListView ListQuotes) {
            var EmptyView = (StackPanel)UI.FindChildControl<StackPanel>(QuotesSection, "EmptyView");
            var ProgressQuotes = (ProgressBar)UI.FindChildControl<ProgressBar>(QuotesSection, "ProgressQuotes");

            var resAsync = Dispatcher.RunAsync(CoreDispatcherPriority.Low, async () => {
                await PageDataSource.LoadAuthorQuotes();

                HideQuotesLoadingIndicator(ProgressQuotes);

                if (PageDataSource.AuthorQuotesList.Count > 0) {
                    BindCollectionToView(ListQuotes, EmptyView);
                }                    
                else ShowEmptyViewQuotes(ListQuotes, EmptyView);
            });
        }

        private void PopulatePage(Author author) {
            author.IsLoading = true;
            author.Biography = "loading"; // TODO: Language

            BindAuthorDataContext(author);

            FetchOtherInformationAsync();

            async void FetchOtherInformationAsync() {
                Author authorFilled = await PageDataSource.LoadAuthor(author);
                authorFilled.IsLoading = false;

                author.Biography = authorFilled.Biography;
                author.Birth = authorFilled.Birth;
                author.Death = authorFilled.Death;
                author.LifeTime = authorFilled.LifeTime;
                author.Quote = authorFilled.Quote;
                author.IsLoading = authorFilled.IsLoading;
            }
        }

        private void BindAuthorDataContext(Author author) {
            HeroSection.DataContext = author;
        }

        void StartAuthorAnimation(StackPanel AuthorHeader) {
            var Job = (TextBlock)UI.FindChildControl<TextBlock>(AuthorHeader, "Job");
            var LifeTime = (TextBlock)UI.FindChildControl<TextBlock>(AuthorHeader, "LifeTime");
            var MainQuote = (TextBlock)UI.FindChildControl<TextBlock>(AuthorHeader, "MainQuote");
            var Biography = (RichTextBlock)UI.FindChildControl<RichTextBlock>(HeroSection, "Biography");

            Job.Offset(offsetX: 0, offsetY: 20, duration: 300, delay: 0).Start();
            LifeTime.Offset(offsetX: 0, offsetY: 20, duration: 500, delay: 0).Start();
            MainQuote.Offset(offsetX: 0, offsetY: 20, duration: 800, delay: 0).Start();
            Biography.Offset(offsetX: 0, offsetY: 50, duration: 1000, delay: 0).Start();
        }

        private void BindCollectionToView(ListView ListQuotes, StackPanel EmptyView) {
            _isQuotesLoaded = true;

            EmptyView.Visibility = Visibility.Collapsed;
            ListQuotes.Visibility = Visibility.Visible;
            ListQuotes.ItemsSource = PageDataSource.AuthorQuotesList;
        }
        

        private void ShowQuotesLoadingIndicator(StackPanel EmptyView, ProgressBar ProgressQuotes) {
            EmptyView.Visibility = Visibility.Collapsed;
            ProgressQuotes.Visibility = Visibility.Visible;
            ProgressQuotes.Visibility = Visibility.Visible;
        }

        private void HideQuotesLoadingIndicator(ProgressBar ProgressQuotes) {
            ProgressQuotes.Visibility = Visibility.Collapsed;
        }

        private void ShowEmptyViewQuotes(ListView ListQuotes, StackPanel EmptyView) {
            EmptyView.Visibility = Visibility.Visible;
            ListQuotes.Visibility = Visibility.Collapsed;
        }
        
        private void BackToTop_Tapped(object sender, TappedRoutedEventArgs e) {
            if (AuthorQuotes != null) VisualTreeExtensions.ScrollToIndex(AuthorQuotes, 0);
        }

        private void HeroBackground_Loaded(object sender, RoutedEventArgs ev) {
            var image = (Image)sender;

            if (PageDataSource.AuthorLoaded) {
                SetWallpaper();
                return;
            }

            PropertyChangedEventHandler biographyReady = null;
            biographyReady = (s, e) => {
                PageDataSource.PropertyChanged -= biographyReady;
                SetWallpaper();
            };

            PageDataSource.PropertyChanged += biographyReady;

            async void SetWallpaper()
            {
                await image.Scale(1.1f, 1.1f, 0, 0, 0).StartAsync();

                var womanPath = "ms-appx:///Assets/Backgrounds/woman.jpg";
                var IsAWoman = false;

                if (IsAWoman) image.Source = new BitmapImage(new Uri(womanPath));

                image.Fade(.1f, 1000)
                     .Scale(1f, 1f, 0, 0, 1000)
                     .Blur(20, 1000)
                     .Start();
            }
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
            if (await PageDataSource.IsFavorite(quote.Link)) {
                PageDataSource.RemoveFromFavorites(quote);
                quote.IsFavorite = false;

            } else {
                PageDataSource.AddToFavorites(quote);
                quote.IsFavorite = true;
            }
        }
    }
}
