using citations365.Controllers;
using citations365.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// Pour plus d'informations sur le modèle d'élément Page vierge, voir la page http://go.microsoft.com/fwlink/?LinkId=234238

namespace citations365.Views {
    public sealed partial class AuthorsPage : Page {
        private static AuthorsController _authorController;

        public static AuthorsController AuthorsController {
            get {
                if (_authorController == null) {
                    _authorController = new AuthorsController();
                }
                return _authorController;
            }
        }

        private float _animationDelay = 0.5f;

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

            var EllipseAuthor = (UIElement)panel.FindName("EllipseAuthor");

            var matrice = EllipseAuthor.TransformToVisual(Window.Current.Content);
            Point EllipseAuthorCoords = matrice.TransformPoint(new Point(0, 0));

            //ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("EllipseAuthor", EllipseAuthor);
            //Frame.Navigate(typeof(DetailAuthorPage), author, new DrillInNavigationTransitionInfo());
            var payload = new Dictionary<string, object>() {
                { "AuthorPayload", author },
                { "EllipseAuthorCoords", EllipseAuthorCoords }
            };
            Frame.Navigate(typeof(DetailAuthorPage), payload, new DrillInNavigationTransitionInfo());
        }

        private void Author_Loaded(object sender, RoutedEventArgs e) {
            var panel = (StackPanel)sender;

            var visual = ElementCompositionPreview.GetElementVisual(panel);
            var compositor = visual.Compositor;

            var offsetAnimation = compositor.CreateScalarKeyFrameAnimation();
            offsetAnimation.InsertKeyFrame(0.0f, 100f);
            offsetAnimation.InsertKeyFrame(1.0f, 0f);
            offsetAnimation.Duration = TimeSpan.FromSeconds(_animationDelay);

            visual.StartAnimation("Offset.y", offsetAnimation);

            _animationDelay += 0.3f;
        }
    }
}
