using citations365.Services;
using System;
using Windows.ApplicationModel.Email;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace citations365.Views {
    public sealed partial class SettingsPage : Page {
        public SettingsPage() {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);
            CoreWindow.GetForCurrentThread().KeyDown += SettingsPage_KeyDown;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            CoreWindow.GetForCurrentThread().KeyDown -= SettingsPage_KeyDown;
            base.OnNavigatedFrom(e);
        }

        private void SettingsPage_KeyDown(CoreWindow sender, KeyEventArgs args) {
            if (Events.IsBackOrEscapeKey(args.VirtualKey) && Frame.CanGoBack) {
                Frame.GoBack();
            }
        }

        private void TasksSection_Loaded(object sender, RoutedEventArgs e) {
            UpdateQuoteTaskSwitcher();
            UpdateWallTaskSwitcher();
        }

        private void PersonalizationSection_Loaded(object sender, RoutedEventArgs e) {
            UpdateThemeSwitcher();
        }

        private void UpdateQuoteTaskSwitcher() {
            var TaskSwitch = (ToggleSwitch)UI.FindChildControl<ToggleSwitch>(TasksSection, "TaskSwitch");
            TaskSwitch.IsOn = BackgroundTasks.IsQuoteTaskActivated();
        }

        private void UpdateWallTaskSwitcher() {
            var LockscreenSwitch = (ToggleSwitch)UI.FindChildControl<ToggleSwitch>(TasksSection, "LockscreenSwitch");
            LockscreenSwitch.IsOn = BackgroundTasks.IsLockscreenTaskActivated();
        }

        private void UpdateThemeSwitcher() {
            var ThemeSwitch = (ToggleSwitch)UI.FindChildControl<ToggleSwitch>(PersonalizationSection, "ThemeSwitch");
            ThemeSwitch.IsOn = Settings.IsApplicationThemeLight();
        }

        /// <summary>
        /// Add or remove background task when the toggle changes state
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TaskSwitch_Toggled(object sender, RoutedEventArgs e) {
            var toggle = (ToggleSwitch)sender;

            if (toggle.IsOn) BackgroundTasks.RegisterQuoteTask();
            else BackgroundTasks.UnregisterQuoteTask();
        }

        private void LockscreenSwitch_Toggled(object sender, RoutedEventArgs e) {
            var toggle = (ToggleSwitch)sender;

            if (toggle.IsOn) BackgroundTasks.RegisterLockscreenTask();
            else BackgroundTasks.UnregisterLockscreenTask();
        }

        private void FeedbackButton_Click(object sender, RoutedEventArgs e) {
            EmailMessage email = new EmailMessage() {
                Subject = "[Citations 365] Feedback",
                Body = "send this email to metrodevapp@outlook.com"
            };

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
            Settings.UpdateAppTheme(theme);
        }

        private void ThemeSwitch_Toggled(object sender, RoutedEventArgs e) {
            var toggle = (ToggleSwitch)sender;
            if (toggle.IsOn) {
                ChangeTheme(ApplicationTheme.Light);
            } else {
                ChangeTheme(ApplicationTheme.Dark);
            }
        }

        private void Background_Choosed(object sender, RoutedEventArgs e) {
            var radioButton = (RadioButton)sender;
            string background = radioButton.Name;

            //Scontroller.UpdateAppBackground(background);
        }

        void UpdateSelectedLanguage() {
            var FrenchLanguageItem = 
                (ToggleMenuFlyoutItem)UI.FindChildControl<ToggleMenuFlyoutItem>(PersonalizationSection, "FrenchLanguageItem");
            FrenchLanguageItem.IsChecked = true;
        }

        private void SetLockscreen_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e) {
            Wallpaper.SetWallpaperAsync();
        }

        private void EnglishLanguage_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e) {
            var item = (ToggleMenuFlyoutItem)sender;
            UnselectOtherLanguages(item);
        }

        private void FrenchLanguage_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e) {
            var item = (ToggleMenuFlyoutItem)sender;
            UnselectOtherLanguages(item);
        }

        void UnselectOtherLanguages(ToggleMenuFlyoutItem selectedItem) {
            var LanguageButton = (Button)UI.FindChildControl<Button>(PersonalizationSection, "LanguageButton");
            var LanguageFlyout = (MenuFlyout)LanguageButton.Flyout;

            foreach (ToggleMenuFlyoutItem item in LanguageFlyout.Items) {
                item.IsChecked = false;
            }

            selectedItem.IsChecked = true;
        }
    }
}
