using System;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Tasks.Models;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.UserProfile;

namespace Tasks {
    public sealed class UpdateBackground : IBackgroundTask {
        BackgroundTaskDeferral _deferral;
        volatile bool _cancelRequested = false;
        private string unsplashURL = "https://unsplash.it/1080?random";

        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason) {
            // Indicate that the background task is canceled.
            _cancelRequested = true;
        }

        public async void Run(IBackgroundTaskInstance taskInstance) {
            // 1.Get userSettings to generate the right file's name
            // 2.Download the background and save it to IO with the generated file's name
            // 3.Return the file saved
            // 4.Save the new file's name and path to userSettings
            // 5.Set the new wallpaper
            _deferral = taskInstance.GetDeferral();

            string appBackgroundName = RetrieveAppBackgroundName();
            string newFilesName = GenerateAppBackgroundName(appBackgroundName);

            StorageFile wall = await DownloadImagefromServer(unsplashURL, newFilesName);
            await SetWallpaperAsync(wall);
            SaveBackground(wall);

            _deferral.Complete();
        }

        public string RetrieveAppBackgroundName() {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            return (string)localSettings.Values["AppBackgroundName"];
        }

        public void SaveBackground(StorageFile wall) {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["AppBackgroundName"] = wall.Name;
            localSettings.Values["AppBackgroundPath"] = wall.Path;
        }

        // Pass in a relative path to a file inside the local appdata folder 
        private async Task<bool> SetWallpaperAsync(StorageFile file) {
            bool success = false;

            if (UserProfilePersonalizationSettings.IsSupported()) {
                UserProfilePersonalizationSettings profileSettings = UserProfilePersonalizationSettings.Current;
                success = await profileSettings.TrySetLockScreenImageAsync(file);
            }
            return success;
        }

        public static string GenerateAppBackgroundName(string previousName) {
            string name1 = "wall1.png";
            string name2 = "wall2.png";

            if (previousName == name1) {
                return name2;
            }
            return name1;
        }


        private async Task<UserSettings> RestoreSettings() {
            try {
                StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync("userSettings.xml");

                var inStream = await file.OpenStreamForReadAsync();

                //Deserialize the objetcs
                DataContractSerializer serializer = new DataContractSerializer(typeof(UserSettings));
                UserSettings data = (UserSettings)serializer.ReadObject(inStream);
                inStream.Dispose();

                return data;

            } catch (FileNotFoundException exception) {
                UserSettings data = default(UserSettings);
                return data;
            }
        }

        private async Task SaveSettingsAsync(UserSettings sourceData, string targetFileName) {
            StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(targetFileName, CreationCollisionOption.ReplaceExisting);
            var outStream = await file.OpenStreamForWriteAsync(); // ERREUR NON GEREE ICI?

            DataContractSerializer serializer = new DataContractSerializer(typeof(UserSettings));
            serializer.WriteObject(outStream, sourceData);
            await outStream.FlushAsync();
            outStream.Dispose();
        }

        private async Task<StorageFile> DownloadImagefromServer(string URI, string filename) {
            var rootFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Citations365\\CoverPics", CreationCollisionOption.OpenIfExists);
            var coverpic = await rootFolder.CreateFileAsync(filename, CreationCollisionOption.FailIfExists);

            try {
                HttpClient client = new HttpClient();
                byte[] buffer = await client.GetByteArrayAsync(URI); // Download file
                using (Stream stream = await coverpic.OpenStreamForWriteAsync())
                    stream.Write(buffer, 0, buffer.Length); // Save

                return coverpic;
            } catch {
                return null;
            }
        }
    }
}
