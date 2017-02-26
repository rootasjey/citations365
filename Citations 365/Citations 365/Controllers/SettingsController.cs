using citations365.Helpers;
using citations365.Models;
using System;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.System.UserProfile;
using Windows.UI.Xaml;

namespace citations365.Controllers {
    public class SettingsController
    {
        /*
         * ***********
         * VARIABLES
         * ***********
         */
        private static UserSettings _userSettings = null;

        public static UserSettings userSettings {
            get {
                if (_userSettings == null) {
                    _userSettings = new UserSettings();
                }
                return _userSettings;
            }
            set {
                _userSettings = value;
            }
        }

        string _taskName = "UpdateTodayQuoteTask";
        string _entryPoint = "Tasks.UpdateTodayQuote";

        //string _backgroundTaskName = "UpdateBackground";
        //string _backgroundEntryPoint = "Tasks.UpdateBackground";

        string _backgroundTaskName = "LockScreenUpdater";
        string _backgroundEntryPoint = "OptimizedTasks.LockScreenUpdater";

        /*
         * ************
         * CONSTRUCTOR
         * ************
         */
        /// <summary>
        /// Initialize the controller
        /// </summary>
        public SettingsController() {

        }

        /*
         * ********
         * METHODS
         * ********
         */
        public async Task<bool> Update(UserSettings settings) {
            _userSettings = settings;
            return await SaveSettings();
        }

        /// <summary>
        /// Clear data and save settings
        /// </summary>
        /// <returns>True if data has been cleared and saved</returns>
        public async Task<bool> Reset() {
            _userSettings = null;
            _userSettings = new UserSettings();
            return await SaveSettings();
        }

        /// <summary>
        /// Save user's settings (background color, background task, etc.)
        /// </summary>
        /// <returns>True if the settings has been correctly saved. False if there was an error</returns>
        public static async Task<bool> SaveSettings() {
            try {
                await DataSerializer<UserSettings>.SaveObjectsAsync(userSettings, "userSettings.xml");
                return true;
            } catch (IsolatedStorageException exception) {
                return false; // error
            }
        }

        /// <summary>
        /// Load user's settings (background color, background task, etc.)
        /// </summary>
        /// <returns>True if the settings has been correctly loaded. False if there was an error</returns>
        public static async Task<bool> LoadSettings() {
            try {
                UserSettings settings = await DataSerializer<UserSettings>.RestoreObjectsAsync("userSettings.xml");
                if (settings != null) {
                    userSettings = settings;
                    return true;
                }
                return false;

            } catch (IsolatedStorageException exception) {
                return false; // error
            }
        }

        public bool IsQuoteTaskActivated() {
            foreach (var task in BackgroundTaskRegistration.AllTasks) {
                if (task.Value.Name == GetTaskQuoteName()) {
                    return true;
                }
            }
            return false;
        }

        public bool IsWallTaskActivated() {
            foreach (var task in BackgroundTaskRegistration.AllTasks) {
                if (task.Value.Name == GetTaskBackgroundName()) {
                    return true;
                }
            }
            return false;
        }

        public async void RegisterBackgroundTask(string taskName, string entryPoint) {
            foreach (var task in BackgroundTaskRegistration.AllTasks) {
                if (task.Value.Name == taskName) {
                    return;
                }
            }

            BackgroundAccessStatus status = await BackgroundExecutionManager.RequestAccessAsync();
            if (status == BackgroundAccessStatus.DeniedBySystemPolicy ||
                status == BackgroundAccessStatus.DeniedByUser) {
                return; // show message that task couldn't be registered
            }

            var builder = new BackgroundTaskBuilder();

            builder.Name = taskName;
            builder.TaskEntryPoint = entryPoint;
            builder.SetTrigger(new TimeTrigger(60, false));
            BackgroundTaskRegistration taskRegistered = builder.Register();
        }

        public void UnregisterBackgroundTask(string taskName) {
            foreach (var task in BackgroundTaskRegistration.AllTasks) {
                if (task.Value.Name == taskName) {
                    BackgroundExecutionManager.RemoveAccess();
                    task.Value.Unregister(false);
                    break;
                }
            }
        }

        public string GetTaskQuoteName() {
            return _taskName;
        }

        public string GetTaskQuoteEntryPoint() {
            return _entryPoint;
        }

        public string GetTaskBackgroundName() {
            return _backgroundTaskName;
        }

        public string GetTaskBackgroundEntryPoint() {
            return _backgroundEntryPoint;
        }

        public void UpdateAppTheme(ApplicationTheme theme) {
            userSettings.applicationTheme = theme;
            SaveSettings();

            // TODO: change theme dynamically
            // update listview styles
            //App._shell.RequestedTheme = (ElementTheme)theme;
        }

        public bool IsApplicationThemeLight() {
            return ApplicationTheme.Light == userSettings.applicationTheme;
        }

        public ApplicationTheme GetAppTheme() {
            return userSettings.applicationTheme;
        }

        public bool IsAppBackgroundDynamic() {
            return !string.IsNullOrEmpty(userSettings.AppBackground);
        }

        public static string GetAppBackground() {
            return userSettings.AppBackground;
        }

        public static string GetAppBackgroundURL() {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            string path = (string)localSettings.Values["LockscreenBackgroundPath"];
            path = path == null ? (string)localSettings.Values["AppBackgroundPath"] : path;
            return path;
        }

        public async void UpdateAppBackground(string background) {
            if (userSettings.AppBackground == background) {
                return;
            }

            TodayController.backgroundChanged = true;
            userSettings.AppBackground = background;
            SaveSettings();
        }

        public static async void UpdateAppBackgroundURL(string url) {
            if (userSettings.AppBackgroundURL == url) {
                return;
            }

            userSettings.AppBackgroundURL = url;
            SaveSettings();
            
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["AppBackgroundPath"] = url;
        }

        public static void UpdateAppBackgroundType(string type) {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["AppBackgroundType"] = type;
        }

        public static async void UpdateAppBackgroundName(string name) {
            if (userSettings.AppBackgroundName == name) {
                return;
            }

            userSettings.AppBackgroundName = name;
            SaveSettings();

            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["AppBackgroundName"] = name;
        }

        public static string GetAppBackgroundName() {
            return userSettings.AppBackgroundName;
        }

        public static string GenerateAppBackgroundName() {
            string previousName = userSettings.AppBackgroundName;

            string name1 = "wall1.png";
            string name2 = "wall2.png";

            if (previousName == name1) {
                return name2;
            }
            return name1;
        }

        // Pass in a relative path to a file inside the local appdata folder 
        public async Task<bool> SetWallpaperAsync() {
            bool success = false;

            if (UserProfilePersonalizationSettings.IsSupported()) {
                StorageFile file = await ImageHelper.GetLockscreenImage(GetAppBackgroundName());
                UserProfilePersonalizationSettings profileSettings = UserProfilePersonalizationSettings.Current;
                success = await profileSettings.TrySetLockScreenImageAsync(file);
            }
            return success;
        }
    }
}
