using citations365.Controllers;
using citations365.Models;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace citations365.Views {
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class SearchPage : Page
    {
        /// <summary>
        /// Search controller
        /// </summary>
        private static SearchController _Scontroller;

        /// <summary>
        /// Search controller
        /// </summary>
        public static SearchController Scontroller {
            get {
                if (_Scontroller == null) {
                    _Scontroller = new SearchController();
                }
                return _Scontroller;
            }
        }
        
        /// <summary>
        /// Contains the text infos contents
        /// </summary>
        private static IDictionary<string, string> _textInfosContent =
            new Dictionary<string, string>();

        /// <summary>
        /// Avoid running multiple search calls
        /// </summary>
        private bool _performingSearch = false;

        /// <summary>
        /// Page Constructor
        /// </summary>
        public SearchPage()
        {
            this.InitializeComponent();
            Populate();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);
            // Set the input focus to ensure that keyboard events are raised.
            //this.Loaded += delegate { this.Focus(FocusState.Programmatic); };
            //InputSearch.Loaded += delegate { InputSearch.Focus(FocusState.Programmatic); };
        }

        /// <summary>
        /// Populate the page
        /// </summary>
        public async void Populate() {
            await Scontroller.LoadData();
            BindCollectionToView();
            PopulateTextInfos();            
        }

        /// <summary>
        /// Populate the text infos dictionary
        /// </summary>
        private void PopulateTextInfos() {
            if (_textInfosContent.Count > 0) {
                return;
            }

            _textInfosContent["default"] = "Recherchez des citations par mot-clés";
            _textInfosContent["searchFailed"] = "Désolé, nous n'avons trouvé aucune citation comportement ce mot clé :/";
            _textInfosContent["searching"] = "Patientez quelques instants, nous recherchons dans le Cyber-Espace...";
        }

        /// <summary>
        /// Set the binding
        /// </summary>
        private void BindCollectionToView() {
            ListQuotes.ItemsSource = SearchController.SearchCollection;
        }

        /// <summary>
        /// Add/Remove a favorite
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Favorite_Tapped(object sender, TappedRoutedEventArgs e) {
            FontIcon icon = (FontIcon)sender;
            Quote quote = (Quote)icon.DataContext;

            if (FavoritesController.IsFavorite(quote.Link)) {
                // Remove from favorites
                bool result = await FavoritesController.RemoveFavorite(quote);
                if (result) {
                    quote.IsFavorite = Quote.UnFavoriteIcon;
                }
            } else {
                // Add to favorites
                bool result = await FavoritesController.AddFavorite(quote);
                if (result) {
                    quote.IsFavorite = Quote.FavoriteIcon;
                }
            }
        }

        /// <summary>
        /// Search for quotes containing the query string
        /// </summary>
        /// <param name="query"></param>
        private async void RunSearch(string query) {
            bool result;

            // Show loading screen
            ShowLoading();

            // Get the quotes
            result = await Scontroller.GetQuotes(query);

            // Hide load scren
            if (result) {
                NoContentView.Visibility = Visibility.Collapsed;
                ListQuotes.Visibility = Visibility.Visible;
            } else {
                NoContentView.Visibility = Visibility.Visible;
                ListQuotes.Visibility = Visibility.Collapsed;
                TextInfos.Text = _textInfosContent["searchFailed"];
            }

            _performingSearch = false;
        }

        /// <summary>
        /// Erase the text input
        /// </summary>
        private void ClearSearch() {
            InputSearch.Text = "";
        }

        private void ShowLoading() {
            ListQuotes.Visibility = Visibility.Collapsed;
            InputSearch.Visibility = Visibility.Collapsed;
            TextInfos.Text = _textInfosContent["searching"];
        }

        private void HideLoading() {
            ListQuotes.Visibility = Visibility.Visible;
            InputSearch.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Display the text input and hide the quotes list
        /// </summary>
        private void ShowInput() {
            if (NoContentView.Visibility == Visibility.Collapsed) {
                ListQuotes.Visibility = Visibility.Collapsed;
                NoContentView.Visibility = Visibility.Visible;
                TextInfos.Text = _textInfosContent["default"];
                InputSearch.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Display the quotes list and hide the text input 
        /// </summary>
        private void ShowResults() {
            if (NoContentView.Visibility == Visibility.Visible) {
                NoContentView.Visibility = Visibility.Collapsed;
                ListQuotes.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Watch for keypressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InputSearch_KeyDown(object sender, KeyRoutedEventArgs e) {
            if (e.Key == Windows.System.VirtualKey.Enter && !_performingSearch) {
                string query = InputSearch.Text;
                RunSearch(query);
            }
        }
        
        /// <summary>
        /// Show search result
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResultsButton_Click(object sender, RoutedEventArgs e) {
            ShowResults();
        }

        /// <summary>
        ///  Clear input, show input and Start a new search
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewSearchButton_Click(object sender, RoutedEventArgs e) {
            ClearSearch();
            ShowInput();
        }

        private void InputSearch_GotFocus(object sender, RoutedEventArgs e) {
            TextBox inputSearch = (TextBox)sender;
            if (inputSearch.FocusState == FocusState.Unfocused) {
                InputFocus();
                System.Diagnostics.Debug.Write("input focused");
            }   
        }

        private void InputFocus() {
            bool result = InputSearch.Focus(FocusState.Programmatic);
        }

        private void InputSearch_Loaded(object sender, RoutedEventArgs e) {
            InputFocus();
        }

        private void InputSearch_LostFocus(object sender, RoutedEventArgs e) {
            //InputFocus();
        }

        private void Page_KeyDown(object sender, KeyRoutedEventArgs e) {

        }
    }
}
