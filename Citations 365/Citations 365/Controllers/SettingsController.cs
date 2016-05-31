using citations365.Models;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

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

        public bool IsLiveTaskActivated() {
            foreach (var task in BackgroundTaskRegistration.AllTasks) {
                if (task.Value.Name == _taskName) {
                    return true;
                }
            }
            return false;
        }

        public void RegisterBackgroundTask() {
            foreach (var task in BackgroundTaskRegistration.AllTasks) {
                if (task.Value.Name == _taskName) {
                    return;
                }
            }
            var builder = new BackgroundTaskBuilder();

            builder.Name = _taskName;
            builder.TaskEntryPoint = _entryPoint;
            builder.SetTrigger(new TimeTrigger(15, false));
            BackgroundTaskRegistration taskRegistered = builder.Register();
        }

        public void UnregisterBackgroundTask() {
            foreach (var task in BackgroundTaskRegistration.AllTasks) {
                if (task.Value.Name == _taskName) {
                    task.Value.Unregister(false);
                    break;
                }
            }
        }
    }
}
