using System;
using citations365.Models;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Windows.Storage;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;

namespace citations365.Services {
    public class Settings {
        private static string ThemeKey {
            get {
                return "Theme";
            }
        }

        private static string LangKey {
            get {
                return "Language";
            }
        }

        public static void SaveLanguage(string language) {
            var settingsValues = ApplicationData.Current.LocalSettings.Values;
            settingsValues[LangKey] = language;
        }

        public static string GetLanguage() {
            string defaultLanguage = "FR";

            var settingsValues = ApplicationData.Current.LocalSettings.Values;
            return settingsValues.ContainsKey(LangKey) ? (string)settingsValues[LangKey] : defaultLanguage;
        }

        public static bool IsApplicationThemeLight() {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values.TryGetValue(ThemeKey, out var previousTheme);
            return ApplicationTheme.Light.ToString() == (string)previousTheme;
        }

        public static void UpdateAppTheme(ApplicationTheme theme) {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values.TryGetValue(ThemeKey, out var previousTheme);

            if ((string)previousTheme == theme.ToString()) return;

            localSettings.Values[ThemeKey] = theme.ToString();
            App.UpdateAppTheme();
        }

        public static async Task SaveFavoritesAsync(ObservableKeyedCollection favorites, string source) {
            string json = JsonConvert.SerializeObject(favorites);
            StorageFile file = 
                await ApplicationData
                        .Current
                        .LocalFolder
                        .CreateFileAsync("favorites-" + source + ".json", CreationCollisionOption.ReplaceExisting);

            await FileIO.WriteTextAsync(file, json);
        }

        public static async Task<ObservableKeyedCollection> LoadFavoritesAsync(string source) {
            StorageFile file = (StorageFile)await ApplicationData.Current.LocalFolder.TryGetItemAsync("favorites-" + source + ".json");
            if (file == null) return null;

            string json = await FileIO.ReadTextAsync(file);
            ObservableKeyedCollection favorites = JsonConvert.DeserializeObject<ObservableKeyedCollection>(json);
            return favorites;
        }

        public static async Task SaveAuthorsAsync(ObservableCollection<Author> authors) {
            string json = JsonConvert.SerializeObject(authors);
            StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync("authors.json", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, json);
        }

        public static async Task<ObservableCollection<Author>> LoadAuthorsAsync() {
            StorageFile file = (StorageFile)await ApplicationData.Current.LocalFolder.TryGetItemAsync("authors.json");
            if (file == null) return null;

            string json = await FileIO.ReadTextAsync(file);
            ObservableCollection<Author> authors = JsonConvert.DeserializeObject<ObservableCollection<Author>>(json);
            return authors;
        }

        public static async Task SaveQuotesAsync(ObservableKeyedCollection quotes) {
            string json = JsonConvert.SerializeObject(quotes);
            StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(quotes.Name + ".json", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, json);
        }

        public static async Task<ObservableKeyedCollection> LoadQuotesAsync(string name) {
            StorageFile file = (StorageFile)await ApplicationData.Current.LocalFolder.TryGetItemAsync(name);
            if (file == null) return null;

            string json = await FileIO.ReadTextAsync(file);
            ObservableKeyedCollection quotes = JsonConvert.DeserializeObject<ObservableKeyedCollection>(json);
            return quotes;
        }
    }
}
