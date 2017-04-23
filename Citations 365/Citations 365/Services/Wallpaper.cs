using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System.UserProfile;

namespace citations365.Services {
    public static class Wallpaper {
        private static string LockscreenPath = "LockscreenBackgroundPath";
        private static string AppPath = "AppBackgroundPath";

        private static string UnsplashURL {
            get {
                return "https://unsplash.it/1500?random";
            }
        }

        public static void SavePath(string path) {
            var localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values[LockscreenPath] = path;
        }

        public static string GetPath() {
            var settingsValues = ApplicationData.Current.LocalSettings.Values;
            return settingsValues.ContainsKey(LockscreenPath) ? (string)settingsValues[LockscreenPath] : null;
        }

        public static async Task<string> GetNew() {
            string name = GenerateName();
            var wallpaper = await Download(UnsplashURL, name);

            SavePath(wallpaper.Path);

            return wallpaper.Path;
        }

        public static string GenerateName() {
            var random = new Random();
            return "wall-" + random.Next();
        }

        private static async Task<StorageFile> Download(string URI, string filename) {
            var rootFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Citations365\\CoverPics", CreationCollisionOption.OpenIfExists);
            var coverpic = await rootFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);

            try {
                var client = new HttpClient();
                byte[] buffer = await client.GetByteArrayAsync(URI); // Download file
                using (Stream stream = await coverpic.OpenStreamForWriteAsync())
                    stream.Write(buffer, 0, buffer.Length); // Save

                return coverpic;
            } catch {
                return null;
            }
        }

        public static async Task<bool> SetWallpaperAsync() {
            bool success = false;

            if (!UserProfilePersonalizationSettings.IsSupported()) {
                return false;
            }

            var file = await ApplicationData.Current.LocalFolder.GetFileAsync(GetPath());
            UserProfilePersonalizationSettings profileSettings = UserProfilePersonalizationSettings.Current;
            success = await profileSettings.TrySetLockScreenImageAsync(file);
            return success;
        }
    }
}
