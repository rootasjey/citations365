using citations365.Controllers;
using System;
using Windows.ApplicationModel.Email;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// Pour plus d'informations sur le modèle d'élément Page vierge, voir la page http://go.microsoft.com/fwlink/?LinkId=234238

namespace citations365.Views {
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        private static SettingsController _Scontroller;

        public static SettingsController Scontroller {
            get {
                if (_Scontroller == null) {
                    _Scontroller = new SettingsController();
                }
                return _Scontroller;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public SettingsPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);
            UpdateTaskSwitcher();
            UpdateThemeSwitcher();
            UpdateAppBackgroundSwitcher();
        }

        private void UpdateTaskSwitcher() {
            TaskSwitch.IsOn = Scontroller.IsLiveTaskActivated();
        }

        private void UpdateThemeSwitcher() {
            ThemeSwitch.IsOn = Scontroller.IsApplicationThemeLight();
        }

        /// <summary>
        /// Add or remove background task when the toggle changes state
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TaskSwitch_Toggled(object sender, RoutedEventArgs e) {
            var toggle = (ToggleSwitch)sender;
            if (toggle.IsOn) {
                Scontroller.RegisterBackgroundTask();
            } else {
                Scontroller.UnregisterBackgroundTask();
            }
        }

        private void LockscreenSwitch_Toggled(object sender, RoutedEventArgs e) {

        }

        private void FeedbackButton_Click(object sender, RoutedEventArgs e) {
            EmailMessage email = new EmailMessage();
            email.Subject = "[Citations 365] Feedback";
            email.Body = "send this email to metrodevapp@outlook.com";
            // TODO : add app infos
            EmailManager.ShowComposeNewEmailAsync(email);
        }

        private async void NoteButton_Click(object sender, RoutedEventArgs e) {
            string appID = "9wzdncrcwfqr";
            var op = await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store://review/?ProductId=" + appID));
        }

        private async void LockscreenButton_Click(object sender, RoutedEventArgs e) {
            // Launch URI for the lock screen settings screen. 
            var op = await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:lockscreen"));
        }

        private void ChangeTheme(ApplicationTheme theme) {
            if (theme != Scontroller.GetAppTheme()) {
                _Scontroller.UpdateAppTheme(theme);
            }
        }

        private void ThemeSwitch_Toggled(object sender, RoutedEventArgs e) {
            var toggle = (ToggleSwitch)sender;
            if (toggle.IsOn) {
                ChangeTheme(ApplicationTheme.Light);
            } else {
                ChangeTheme(ApplicationTheme.Dark);
            }
        }

        private void UpdateAppBackgroundSwitcher() {
            AppBackgroundgSwitch.IsOn = Scontroller.IsAppBackgroundDynamic();

            if (AppBackgroundgSwitch.IsOn) {
                UpdateAppBackgroundChooser();
            }
        }

        private void UpdateAppBackgroundChooser() {
            string background = SettingsController.GetAppBackground();
            switch (background) {
                case "nasa":
                    nasa.IsChecked = true;
                    break;
                case "unsplash":
                    unsplash.IsChecked = true;
                    break;
                default:
                    break;
            }
        }

        private void AppBgSwitch_Toggled(object sender, RoutedEventArgs e) {
            var toggle = (ToggleSwitch)sender;

            if (toggle.IsOn) {
                BackgroundChooser.Visibility = Visibility.Visible;
            } else {
                BackgroundChooser.Visibility = Visibility.Collapsed;
                Scontroller.UpdateAppBackground("");
            }
        }

        private void Background_Choosed(object sender, RoutedEventArgs e) {
            var radioButton = (RadioButton)sender;
            string background = radioButton.Name;

            Scontroller.UpdateAppBackground(background);
        }

        private void SetLockscreen_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e) {
            Scontroller.SetWallpaperAsync();
        }
    }
}
