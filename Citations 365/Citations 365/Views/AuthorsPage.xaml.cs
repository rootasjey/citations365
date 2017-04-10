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

namespace citations365.Views {
    public sealed partial class AuthorsPage : Page {
        private static AuthorsController _authorController;

        private static Author _LastSelectedAuthor { get; set; }

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
            AuthorsGrid.Loaded += (s, v) => {
                RestorViewPosition();
            };

            await AuthorsController.LoadData();

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

        void RestorViewPosition() {
            var animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("EllipseAuthor");
            if (animation == null || _LastSelectedAuthor == null) return;

            AuthorsGrid.ScrollIntoView(_LastSelectedAuthor);
            AuthorsGrid.UpdateLayout();

            var container = (GridViewItem)AuthorsGrid.ContainerFromItem(_LastSelectedAuthor);
            var root = (StackPanel)container.ContentTemplateRoot;
            var ellipse = (Ellipse)root.FindName("EllipseAuthor");

            animation.TryStart(ellipse);
        }

        private void Authors_Tapped(object sender, TappedRoutedEventArgs e) {
            StackPanel panel = (StackPanel)sender;
            Author author = (Author)panel.DataContext;

            _LastSelectedAuthor = author;

            var EllipseAuthor = (UIElement)panel.FindName("EllipseAuthor");

            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("EllipseAuthor", EllipseAuthor);

            Frame.Navigate(typeof(DetailAuthorPage), author);
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
