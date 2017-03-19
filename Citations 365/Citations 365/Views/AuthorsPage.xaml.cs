using citations365.Controllers;
using citations365.Models;
using System.Linq;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// Pour plus d'informations sur le modèle d'élément Page vierge, voir la page http://go.microsoft.com/fwlink/?LinkId=234238

namespace citations365.Views {
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class AuthorsPage : Page
    {
        private static AuthorsController _authorController;

        public static AuthorsController AuthorsController {
            get {
                if (_authorController == null) {
                    _authorController = new AuthorsController();
                }
                return _authorController;
            }
        }

        public AuthorsPage() {
            InitializeComponent();
            Populate();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            CoreWindow.GetForCurrentThread().KeyDown -= AuthorsPage_KeyDown;
            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            CoreWindow.GetForCurrentThread().KeyDown += AuthorsPage_KeyDown;
            base.OnNavigatedTo(e);
        }

        private void AuthorsPage_KeyDown(CoreWindow sender, KeyEventArgs args) {
            if (Controller.IsBackOrEscapeKey(args.VirtualKey) && Frame.CanGoBack) {
                Frame.GoBack();
            }
        }

        private async void Populate() {
            bool loaded = await AuthorsController.LoadData();
            if (!loaded) return;

            HideLoading();
            BindCollectionToView();
        }

        private void ShowLoading() {
            AuthorsGrid.Visibility = Visibility.Collapsed;
            LoadingView.Visibility = Visibility.Visible;
        }

        private void HideLoading() {
            AuthorsGrid.Visibility = Visibility.Visible;
            LoadingView.Visibility = Visibility.Collapsed;
        }

        private void BindCollectionToView() {
            var groupedAuthors = from author in AuthorsController.AuthorsCollection
                                 group author by author.Name.First() into firstLetter
                                 orderby firstLetter.Key
                                 select firstLetter;

            this.groupedAuthors.Source = groupedAuthors;
            AuthorsKeys.ItemsSource = this.groupedAuthors.View.CollectionGroups;
        }

        private void Authors_Tapped(object sender, TappedRoutedEventArgs e) {
            StackPanel panel = (StackPanel)sender;
            Author author = (Author)panel.DataContext;

            Frame.Navigate(typeof(DetailAuthorPage), author, new DrillInNavigationTransitionInfo());
        }
    }
}
