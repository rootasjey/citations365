using citations365.Data;
using citations365.Helpers;
using citations365.Models;
using citations365.Services;
using Microsoft.Toolkit.Uwp.UI.Animations;
using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

namespace citations365.Views {
    public sealed partial class HomePage_Desktop : Page {
        #region variables
        private static Author _LastSelectedAuthor { get; set; }

        float _animationDelayAuthors { get; set; }

        int _animationDelay { get; set; }

        int _animationDelayFavorites { get; set; }

        int _animationDelaySearch { get; set; }

        bool _performingSearch { get; set; }

        TextBox _inputSearch = null;
        TextBox _InputSearch {
            get {
                if (_inputSearch == null) {
                    _inputSearch = (TextBox)UI.FindChildControl<TextBox>(SearchSection, "InputSearch");
                }
                return _inputSearch;
            }
        }

        static string _LastSelectedSection { get; set; }

        bool _OverrideFocusedSection { get; set; }

        static Quote _LastSelectedRecent { get; set; }
        static Quote _LastSelectedFavorite { get; set; }
        static Quote _LastSelectedSearch { get; set; }

        private SourceModel PageDataSource { get; set; }

        /// <summary>
        /// Get a random wallpaper every app new launch
        /// </summary>
        static string TodayWallpaperPath { get; set; }
        #endregion variables

        public HomePage_Desktop() {
            InitializeComponent();
            InitializeLocalVariables();
            PageDataSource = App.DataSource;

            var animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("EllipseAuthor");
            if (animation != null && _LastSelectedSection != "AuthorsSection") {
                animation.Cancel();
            }
        }

        void InitializeLocalVariables() {
            _animationDelay = 500;
            _animationDelayFavorites = 500;
            _animationDelayAuthors = 0.5f;
            _performingSearch = false;
            _OverrideFocusedSection = false;
        }

        private async void WindowsStates_CurrentStateChanged(object sender, VisualStateChangedEventArgs e) {
            if (e.NewState.Name == "WideState") {
                LoadHeroSection();
            }
        }


        #region loading

        private void RecentSection_Loaded(object sender, RoutedEventArgs e) {
            LoadRecentView();
        }

        private void FavoritesSection_Loaded(object sender, RoutedEventArgs e) {
            LoadFavoritesView();
        }

        async void LoadRecentView() {
            await App.DataSource.LoadRecent();

            if (App.DataSource.RecentList.Count == 0) { ShowNoQuotesView(); return; }

            var ListQuotes = (ListView)UI.FindChildControl<ListView>(RecentSection, "ListQuotes");
            ListQuotes.ItemsSource = App.DataSource.RecentList;

            LoadHeroSection();
        }

        async void LoadHeroSection() {
            if (App.DataSource.RecentList.Count == 0) return;

            var heroContent = (StackPanel)UI.FindChildControl<StackPanel>(HeroSection, "HeroContent");

            if (heroContent == null) return;

            var content = (TextBlock)heroContent.Children[0];
            var author = (TextBlock)heroContent.Children[1];
            var reference = (TextBlock)heroContent.Children[2];

            var firstQuote = App.DataSource.RecentList[0];

            await InitializeAnimation();
            StartAnimation();

            async Task InitializeAnimation() {
                await content.Fade(0, 0).Offset(0, 50, 0).StartAsync();
                await author.Fade(0, 0).Offset(0, 50, 0).StartAsync();
                await reference.Fade(0, 0).Offset(0, 50, 0).StartAsync();

                HeroSection.DataContext = firstQuote;
            }

            void StartAnimation() {
                content.Fade(1, 1000, 0).Offset(0, 0, 1000, 0).Start();
                author.Fade(1, 1000, 200).Offset(0, 0, 1000, 200).Start();
                reference.Fade(1, 1000, 500).Offset(0, 0, 1000, 500).Start();
            }
        }

        async void LoadFavoritesView() {
            await PageDataSource.InitializeFavorites();

            var ListQuotes = (ListView)UI.FindChildControl<ListView>(FavoritesSection, "ListQuotes");
            var NoContentView = (StackPanel)UI.FindChildControl<StackPanel>(FavoritesSection, "EmptyView");

            if (await PageDataSource.IsFavoritesEmpty()) {
                return;
            }

            NoContentView.Visibility = Visibility.Collapsed;
            ListQuotes.Visibility = Visibility.Visible;
            ListQuotes.ItemsSource = PageDataSource.FavoritesList;
        }

        private async void Quote_Loaded(object sender, RoutedEventArgs e) {
            var panel = (Grid)sender;

            await panel.Fade(0, 0).Offset(0, 40, 0).StartAsync();
            panel.Fade(1, 1000, _animationDelay)
                 .Offset(0, 0, 1000, _animationDelay, EasingType.Quintic)
                 .Start();

            _animationDelay += 100;
        }

        private void FavoritesQuote_Loaded(object sender, RoutedEventArgs e) {
            var panel = (StackPanel)sender;

            var visual = ElementCompositionPreview.GetElementVisual(panel);
            var compositor = visual.Compositor;

            var slideUpAnimation = compositor.CreateVector2KeyFrameAnimation();
            slideUpAnimation.InsertKeyFrame(0.0f, new Vector2(0f, 100f));
            slideUpAnimation.InsertKeyFrame(1.0f, new Vector2(0, 0));
            slideUpAnimation.Duration = TimeSpan.FromMilliseconds(_animationDelay);
            visual.StartAnimation("Offset.xy", slideUpAnimation);

            _animationDelayFavorites += 200;
        }

        private void ListQuotes_Loaded(object sender, RoutedEventArgs ev) {
            //_ListQuotesScrollViewer = ListQuotes.GetChildOfType<ScrollViewer>();
            //_ListQuotesScrollerPropertySet = ElementCompositionPreview.
            //        GetScrollViewerManipulationPropertySet(_ListQuotesScrollViewer);

            //AnimateHeroQuote();
            RestoreListPosition((ListView)sender);
        }


        private void ShowNoQuotesView() {
            var ListQuotes = (ListView)UI.FindChildControl<ListView>(RecentSection, "ListQuotes");
            var EmptyView = (StackPanel)UI.FindChildControl<StackPanel>(RecentSection, "EmptyView");

            ListQuotes.Visibility = Visibility.Collapsed;
            EmptyView.Visibility = Visibility.Visible;
        }

        #endregion loading

        #region navigation

        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            CoreWindow.GetForCurrentThread().KeyDown -= HubPage_KeyDown;

            if (!_OverrideFocusedSection)
                _LastSelectedSection = HomeHub.SectionsInView.First().Name;

            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            CoreWindow.GetForCurrentThread().KeyDown += HubPage_KeyDown;
            
            RestoreHubSectionPosition();

            base.OnNavigatedTo(e);
        }

        #endregion navigation

        #region Events
        /// <summary>
        ///  Perform operations that should take place when the application becomes visible rather than
        ///  when it is prelaunched, such as building a what's new feed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void WindowVisibilityChangedEventHandler(object sender, Windows.UI.Core.VisibilityChangedEventArgs e) {
            if (!e.Visible) { // app losing focus
                return;
            }
            App.DataSource.CheckHeroQuote();
        }

        private void HubPage_KeyDown(CoreWindow sender, KeyEventArgs args) {
            if (_InputSearch != null && 
                _InputSearch.FocusState != FocusState.Unfocused) return;

            if (Events.IsBackOrEscapeKey(args.VirtualKey) && Frame.CanGoBack) {
                Frame.GoBack();
            }
        }
        
        private void ListQuotes_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args) {
            if (args.ItemIndex == 0) { // handle only the 1st item
                InitializeFirstQuote(args);

                var ListQuotes = (ListView)UI.FindChildControl<ListView>(RecentSection, "ListQuotes");
                ListQuotes.ContainerContentChanging -= ListQuotes_ContainerContentChanging;
            }
        }

        private void InitializeFirstQuote(ContainerContentChangingEventArgs visual) {
            if (Resources.ContainsKey("HeroQuoteTemplate")) {
                DataTemplate heroTemplate = (DataTemplate)Resources["HeroQuoteTemplate"];
                visual.ItemContainer.ContentTemplate = heroTemplate;
            }
        }

        private void BackToTop_Tapped(object sender, TappedRoutedEventArgs e) {
            var sectionName = (string)((StackPanel)sender).Tag;
            var section = (HubSection)HomeHub.FindName(sectionName);
            var list = (ListView)UI.FindChildControl<ListView>(section, "ListQuotes");

            if (list == null) return;

            VisualTreeExtensions.ScrollToIndex(list, 0);
        }

        #endregion Events

        #region background
        private async void HeroBackground_Loaded(object sender, RoutedEventArgs e) {
            var img = (Image)sender;
            img.ImageOpened += Img_ImageOpened;

            string path = null;
            if (string.IsNullOrEmpty(TodayWallpaperPath)) {
                path = await Wallpaper.GetNew();
                if (string.IsNullOrEmpty(path)) return;
            }

            TodayWallpaperPath = path ?? TodayWallpaperPath;

            var bmp = new BitmapImage(new Uri(TodayWallpaperPath));
            img.Source = bmp;

            async void Img_ImageOpened(object _s, RoutedEventArgs _e)
            {
                await img.Scale(1.1f, 1.1f, 0, 0, 0).StartAsync();

                var bounds = ApplicationView.GetForCurrentView().VisibleBounds;
                var scaleFactor = DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
                var size = new Size(bounds.Width * scaleFactor, bounds.Height * scaleFactor);

                img.Fade(.2f, 1000, 500)
                   .Scale(1f, 1f, (float)size.Width / 2, (float)size.Height / 2, 1000, 500)
                   .Blur(30, 1000, 500)
                   .Start();
            }
        }

        private void BorderBackground_Loaded(object sender, RoutedEventArgs e) {
            var border = (Border)sender;

            var bounds = ApplicationView.GetForCurrentView().VisibleBounds;
            var scaleFactor = DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
            var size = new Size(bounds.Width * scaleFactor, bounds.Height * scaleFactor);

            border.Clip = new RectangleGeometry() { Rect = new Rect(0, 0, 780, size.Height) };
        }

        private void HeroSectionContent_PointerEntered(object sender, PointerRoutedEventArgs e) {
            var HeroSectionContent = (Grid)sender;
            var border = (Border)HeroSectionContent.Children[0];
            var image = (Image)border.Child;

            image.Scale(1.1f, 1.1f, 0, 0)
                .Blur(0)
                .Fade(.4f)
                .Start();
        }

        private void HeroSectionContent_PointerExited(object sender, PointerRoutedEventArgs e) {
            var HeroSectionContent = (Grid)sender;
            var border = (Border)HeroSectionContent.Children[0];
            var image = (Image)border.Child;

            image.Scale(1f, 1f, 0, 0)
                .Blur(30)
                .Fade(.2f)
                .Start();
        }

        #endregion background

        #region favorites
        private async void CheckFavoritesEmptyView() {
            var ListQuotes = (ListView)UI.FindChildControl<ListView>(FavoritesSection, "ListQuotes");
            var EmptyView = (StackPanel)UI.FindChildControl<StackPanel>(FavoritesSection, "EmptyView");
            
            if (await PageDataSource.IsFavoritesEmpty()) {
                EmptyView.Visibility = Visibility.Visible;
                ListQuotes.Visibility = Visibility.Collapsed;
                return;
            }

            EmptyView.Visibility = Visibility.Collapsed;
            ListQuotes.Visibility = Visibility.Visible;

            if (PageDataSource.FavoritesList.Count == 1) {
                ListQuotes.ItemsSource = PageDataSource.FavoritesList;
            }
        }

        async void ToggleFavorite(Quote quote) {
            if (await PageDataSource.IsFavorite(quote.Link)) {
                PageDataSource.RemoveFromFavorites(quote);
                quote.IsFavorite = false;
                CheckFavoritesEmptyView();

            } else {
                PageDataSource.AddToFavorites(quote);
                quote.IsFavorite = true;
                CheckFavoritesEmptyView();
            }
        }
        #endregion favorites

        #region search
        private void SearchSection_Loaded(object sender, RoutedEventArgs e) {
            if (!App.DataSource.HasSearch) {
                SearchSection.Visibility = Visibility.Collapsed;
                return;
            }

            var ListQuotes = (ListView)UI.FindChildControl<ListView>(SearchSection, "ListQuotes");
            ListQuotes.ItemsSource = PageDataSource.ResultsList;
            ShowSearchResults();
        }

        private void SearchQuote_Loaded(object sender, RoutedEventArgs e) {
            var grid = (StackPanel)sender;

            var visual = ElementCompositionPreview.GetElementVisual(grid);
            var compositor = visual.Compositor;

            var slideUpAnimation = compositor.CreateVector2KeyFrameAnimation();
            slideUpAnimation.InsertKeyFrame(0.0f, new Vector2(0f, 100f));
            slideUpAnimation.InsertKeyFrame(1.0f, new Vector2(0, 0));
            slideUpAnimation.Duration = TimeSpan.FromMilliseconds(_animationDelay);
            visual.StartAnimation("Offset.xy", slideUpAnimation);

            _animationDelaySearch += 200;
        }

        private void InputSearch_GotFocus(object sender, RoutedEventArgs e) {
            TextBox inputSearch = (TextBox)sender;
            if (inputSearch.FocusState == FocusState.Unfocused) {
                FocusSearchInput();
                //System.Diagnostics.Debug.Write("input focused");
            }
        }

        private void InputSearch_Loaded(object sender, RoutedEventArgs e) {
            //FocusSearchInput();
        }

        private void InputSearch_KeyDown(object sender, KeyRoutedEventArgs e) {
            if (e.Key == Windows.System.VirtualKey.Enter && !_performingSearch) {
                _performingSearch = true;
                
                string query = _InputSearch.Text;
                RunSearch(query);
            }
        }

        private void FocusSearchInput() {
            _InputSearch.Focus(FocusState.Programmatic);
        }

        private async void RunSearch(string query) {
            //ShowLoadingSearchScreen();
            var result = await PageDataSource.Search(query);

            var ListQuotes = (ListView)UI.FindChildControl<ListView>(SearchSection, "ListQuotes");
            var EmptyView = (StackPanel)UI.FindChildControl<StackPanel>(SearchSection, "EmptyView");

            if (result > 0) { ShowSearchResults(); } 
            else {
                EmptyView.Visibility = Visibility.Visible;
                ListQuotes.Visibility = Visibility.Collapsed;
            }

            _performingSearch = false;
        }

        private void ShowSearchResults() {
            var ListQuotes = (ListView)UI.FindChildControl<ListView>(SearchSection, "ListQuotes");
            var EmptyView = (StackPanel)UI.FindChildControl<StackPanel>(SearchSection, "EmptyView");

            bool alreadyVisible =
                (EmptyView.Visibility == Visibility.Collapsed) &&
                (ListQuotes.Visibility == Visibility.Visible);
            
            bool noResults = (
                PageDataSource.ResultsList == null ||
                PageDataSource.ResultsList.Count < 1);

            if (alreadyVisible || noResults) {
                return;
            }

            EmptyView.Visibility = Visibility.Collapsed;
            ListQuotes.Visibility = Visibility.Visible;
            //AdaptCommandBar();
        }


        private void ClearSearchInput() {
            _InputSearch.Text = "";
        }

        #endregion search


        #region authors

        private void HideLoading(GridView AuthorsGrid) {
            var LoadingView = (StackPanel)UI.FindChildControl<StackPanel>(AuthorsSection, "LoadingView");

            AuthorsGrid.Visibility = Visibility.Visible;
            LoadingView.Visibility = Visibility.Collapsed;
        }

        async Task CheckAuthorsDataLoaded() {
            if (PageDataSource.AuthorsList == null || 
                PageDataSource.AuthorsList.Count == 0) {
                await PageDataSource.LoadAuthors();
            }
        }

        private async void AuthorsGrid_Loaded(object sender, RoutedEventArgs e) {
            if (!App.DataSource.HasAuthors) {
                AuthorsSection.Visibility = Visibility.Collapsed;
                return;
            }

            await CheckAuthorsDataLoaded();

            var AuthorsGrid = (GridView)sender;

            HideLoading(AuthorsGrid);
        }

        private void AuthorsKeys_Loaded(object sender, RoutedEventArgs ev) {
            if (!App.DataSource.HasAuthors) { return; }

            var AuthorsGrid = (GridView)UI.FindChildControl<GridView>(AuthorsSection, "AuthorsGrid");

            if (PageDataSource.AuthorsList.Count > 0) {
                BindAuthorsKeyData((GridView)sender);
                RestorAuthorsListPosition(AuthorsGrid);
                return;
            }

            PageDataSource.PropertyChanged += (s, e) => {
                if (e.PropertyName != "AuthorsListLoaded") return;
                BindAuthorsKeyData((GridView)sender);
                RestorAuthorsListPosition(AuthorsGrid);
            };
        }

        void BindAuthorsKeyData(GridView AuthorsKeys) {
            var groupedAuthors = from author in PageDataSource.AuthorsList
                                 group author by author.Name.First() into firstLetter
                                 orderby firstLetter.Key
                                 select firstLetter;

            this.groupedAuthors.Source = groupedAuthors;
            AuthorsKeys.ItemsSource = this.groupedAuthors.View.CollectionGroups;
        }

        void RestorAuthorsListPosition(GridView AuthorsGrid) {
            if (_LastSelectedAuthor == null) return;

            AuthorsGrid.ScrollIntoView(_LastSelectedAuthor);
            AuthorsGrid.UpdateLayout();

            // TODO: hub's load is too slow - the animation get dismissed
            var animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("EllipseAuthor");

            if (animation == null) return;

            var container = (GridViewItem)AuthorsGrid.ContainerFromItem(_LastSelectedAuthor);
            var root = (StackPanel)container.ContentTemplateRoot;
            var ellipse = (Ellipse)root.FindName("EllipseAuthor");

            animation.TryStart(ellipse);
        }

        private void Authors_Tapped(object sender, TappedRoutedEventArgs e) {
            StackPanel panel = (StackPanel)sender;
            Author author = (Author)panel.DataContext;

            var EllipseAuthor = (UIElement)panel.FindName("EllipseAuthor");

            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("EllipseAuthor", EllipseAuthor);

            UpdateLastSelectedAuthor(author);
            ForceUpdateLastSelectedSection(AuthorsSection.Name);

            Frame.Navigate(typeof(AuthorPage_Desktop), author);
        }

        private void Author_Loaded(object sender, RoutedEventArgs e) {
            var panel = (StackPanel)sender;

            var visual = ElementCompositionPreview.GetElementVisual(panel);
            var compositor = visual.Compositor;

            var offsetAnimation = compositor.CreateScalarKeyFrameAnimation();
            offsetAnimation.InsertKeyFrame(0.0f, 100f);
            offsetAnimation.InsertKeyFrame(1.0f, 0f);
            offsetAnimation.Duration = TimeSpan.FromSeconds(_animationDelayAuthors);

            visual.StartAnimation("Offset.y", offsetAnimation);

            _animationDelayAuthors += 0.3f;
        }


        #endregion authors


        #region List Position
        void RestoreHubSectionPosition() {
            if (_LastSelectedSection == null) return;

            var sections = from s in HomeHub.Sections
                          where s.Name == _LastSelectedSection
                          select s;

            if (sections != null && sections.Count() > 0) {
                HomeHub.ScrollToSection(sections.First());
            }
        }

        void UpdateLastSelectedRecent(Quote quote) {
            _LastSelectedRecent = quote;
        }

        void RestoreListPosition(ListView listView) {
            if (_LastSelectedRecent == null) return;
            
            listView.ScrollIntoView(_LastSelectedRecent);
            listView.UpdateLayout();

            var animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("QuoteAuthor");

            if (animation == null) return;

            var container = (ListViewItem)listView.ContainerFromItem(_LastSelectedRecent);
            var root = (Grid)container.ContentTemplateRoot;
            var textAnimation = (TextBlock)root.FindName("QuoteAuthor");

            animation.TryStart(textAnimation);
        }

        void UpdateLastSelectedAuthor(Author author) {
            _LastSelectedAuthor = author;
        }

        void ForceUpdateLastSelectedSection(string name) {
            _LastSelectedSection = name;
            _OverrideFocusedSection = true;
        }
        #endregion List Position

        #region Quote Events
        private void Quote_Tapped(object sender, TappedRoutedEventArgs e) {
            var panel = (StackPanel)sender;
            var quote = (Quote)panel.DataContext;

            if (quote.AuthorLink != null && quote.AuthorLink.Length > 0) {
                UpdateLastSelectedRecent(quote);
                ForceUpdateLastSelectedSection(RecentSection.Name);
                Frame.Navigate(typeof(AuthorPage_Desktop), quote);
            }
        }

        private void HeroContent_Tapped(object sender, TappedRoutedEventArgs e) {
            var quote = (Quote)HeroSection.DataContext;
            if (string.IsNullOrEmpty(quote.AuthorLink)) return;
            Frame.Navigate(typeof(AuthorPage_Desktop), quote);
        }

        #endregion Quote Events

        #region appbar
        private void AuthorsButton_Tapped(object sender, TappedRoutedEventArgs e) {
            Frame.Navigate(typeof(ListAuthorsPage));
        }

        private void SettingsButton_Tapped(object sender, TappedRoutedEventArgs e) {
            Frame.Navigate(typeof(SettingsPage));
        }
        #endregion appbar

        #region Quote Actions
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
        #endregion QuoteActions
        
    }
}
