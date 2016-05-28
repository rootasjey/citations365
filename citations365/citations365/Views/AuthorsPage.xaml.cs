using BackgroundTasks.Controllers;
using BackgroundTasks.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// Pour plus d'informations sur le modèle d'élément Page vierge, voir la page http://go.microsoft.com/fwlink/?LinkId=234238

namespace BackgroundTasks.Views
{
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

        public AuthorsPage()
        {
            InitializeComponent();
            Populate();
        }

        private async void Populate() {
            bool result = await AuthorsController.LoadData();

            if (result) {
                HideLoading();
                BindCollectionToView();
            }
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
            AuthorsGrid.ItemsSource = AuthorsController.AuthorsCollection;
        }

        private void Authors_Tapped(object sender, TappedRoutedEventArgs e) {
            StackPanel panel = (StackPanel)sender;
            Author author = (Author)panel.DataContext;

            Frame.Navigate(typeof(DetailAuthorPage), author, new DrillInNavigationTransitionInfo());
        }
    }
}
