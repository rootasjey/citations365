using HtmlAgilityPack;
using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tasks.Models;
using Windows.ApplicationModel.Background;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.System.UserProfile;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Tasks {
    public sealed class UpdateBackground : XamlRenderingBackgroundTask {
        BackgroundTaskDeferral _deferral;
        volatile bool _cancelRequested = false;
        private string unsplashURL = "https://unsplash.it/1500?random";
        private string nasaURL = "http://apod.nasa.gov/apod/";

        private string _appBackgroundName = "AppBackgroundName";
        private string _appBackgroundPath = "AppBackgroundPath";
        private string _appBackgroundType = "AppBackgroundType";
        private string _lockscreenBackgroundName = "LockscreenBackgroundName";
        private string _lockscreenBackgroundPath = "LockscreenBackgroundPath";

        private string _dailyQuoteContent = "dailyQuoteContent";
        private string _dailyQuoteAuthor = "dailyQuoteAuthor";

        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason) {
            // Indicate that the background task is canceled.
            _cancelRequested = true;
        }

        public async void Run(IBackgroundTaskInstance taskInstance) {
            _deferral = taskInstance.GetDeferral();

            //string appBackgroundName = RetrieveAppBackgroundName();
            //string appBackgroundType = RetrieveAppBackgroundType();

            //string newFilesName = GenerateAppBackgroundName(appBackgroundName);
            //string backgroundURL = await GetBackgroundURL(appBackgroundType);

            //StorageFile wall = await DownloadImagefromServer(backgroundURL, newFilesName);

            //await SetWallpaperAsync(wall);
            //SaveAppBackground(wall);

            _deferral.Complete();
        }

        protected override async void OnRun(IBackgroundTaskInstance taskInstance) {
            taskInstance.Canceled += OnCanceled;
            var deferral = taskInstance.GetDeferral();
            //string appBackgroundName = RetrieveLockscreenBackgroundName();
            //StorageFile wall = await DownloadImagefromServer(_lockscreenURL, newFilesName);
            //SaveLockscreenBackground(wall);
            var prevName = RetrieveLockscreenBackgroundName();
            var newName = GenerateAppBackgroundName(prevName);
            //var path = RetrieveAppBackgroundPath();
            //var dailyQuote = RetrieveDailyQuote();

            StorageFile wall = await DownloadImagefromServer("https://unsplash.it/720/1280?random", _appBackgroundName);
            //StorageFile wall = await ApplicationData.Current.LocalFolder.GetFileAsync(prevName); ;

            //StorageFile lockImage = await TakeScreenshot(wall.Path, newName, null);
            await SetWallpaperAsync(wall);
            //SaveLockscreenBackgroundName(lockImage.Name);
            //SaveAppBackground(lockImage);
            deferral.Complete();
        }

        private async Task<string> GetBackgroundURL(string type) {
            switch (type) {
                case "unsplash":
                    return unsplashURL;
                case "nasa":
                    //return nasaURL;
                    return await GetNasaImage();
                default:
                    return "";
            }
        }

        public string RetrieveAppBackgroundName() {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            return (string)localSettings.Values[_appBackgroundName];
        }

        public string RetrieveAppBackgroundType() {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            return (string)localSettings.Values[_appBackgroundType];
        }

        public string RetrieveLockscreenBackgroundName() {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            return (string)localSettings.Values[_lockscreenBackgroundName];
        }

        public string RetrieveAppBackgroundPath() {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            return(string)localSettings.Values[_appBackgroundPath];
        }

        public Quote RetrieveDailyQuote() {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            var content = (string)localSettings.Values[_dailyQuoteContent];
            var author = (string)localSettings.Values[_dailyQuoteAuthor];

            return new Quote() {
                Content = content,
                Author = author
            };
        }

        public void SaveAppBackground(StorageFile wall) {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values[_appBackgroundName] = wall.Name;
            localSettings.Values[_appBackgroundPath] = wall.Path;
        }

        public void SaveLockscreenBackground(StorageFile background) {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values[_lockscreenBackgroundName] = background.Name;
            localSettings.Values[_lockscreenBackgroundPath] = background.Path;
        }

        public void SaveLockscreenBackgroundName(string name) {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values[_lockscreenBackgroundName] = name;
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
            filename += ".png";
            var rootFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Citations365\\CoverPics", CreationCollisionOption.OpenIfExists);
            var coverpic = await rootFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);

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

        private async Task<string> GetNasaImage() {
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = null;
            try {
                response = await httpClient.GetAsync(nasaURL);
                response.EnsureSuccessStatusCode();
                string responseBodyAsText = await response.Content.ReadAsStringAsync();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(responseBodyAsText);

                string start = "<a";
                string end = ".jpg";

                Regex regex = new Regex(start + "(.*?)" + end);
                MatchCollection matches = regex.Matches(responseBodyAsText);

                if (matches.Count > 0) {
                    return "http://apod.nasa.gov/apod/" + matches[0].ToString().Substring(9);
                }

                return GetDefaultNasaImage();
            } catch {
                return GetDefaultNasaImage();
            }
        }

        private string GetDefaultNasaImage() {
            return "/Assets/Backgrounds/nasa.jpg";
        }

        private async Task<StorageFile> TakeScreenshot(string url, string name, Quote quote) {
            var bold = new FontWeight();
            bold.Weight = 700;
            TextBlock txtLine1 = new TextBlock() {
                Text = "totooooooo",
                Foreground = new SolidColorBrush(Windows.UI.Colors.White),
                Opacity = 0.6,
                FontSize = 32,
                FontWeight = bold,
                TextAlignment = TextAlignment.Left,
                Margin = new Thickness(48, 400, 12, 0),
                TextWrapping = TextWrapping.Wrap
            };

            StackPanel sp = new StackPanel() {
                Background = new SolidColorBrush(Windows.UI.Colors.Brown),
                Width = 720,
                Height = 1280,
                Orientation = Orientation.Vertical
            };
            var bitmap = new BitmapImage(new System.Uri(url));
            var brush = new ImageBrush();
            brush.ImageSource = bitmap;
            brush.Opacity = 0.5;
            sp.Background = brush;

            //TextBlock txtLine2 = new TextBlock {
            //    Text = "Text Line 2",
            //    Foreground = new SolidColorBrush(Windows.UI.Colors.White),
            //    Width = 200,
            //    Height = 30,
            //    FontSize = 32,
            //    TextAlignment = TextAlignment.Left,
            //    Margin = new Thickness(9, 3, 0, 3)
            //};

            sp.Children.Add(txtLine1);
            //sp.Children.Add(txtLine2);

            sp.UpdateLayout();
            //sp.Measure(new Size(200, 200));
            //sp.Arrange(new Rect(0, 0, 200, 200));
            //sp.UpdateLayout();

            //now lets render this stackpanel on image
            try {
                RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap();
                await renderTargetBitmap.RenderAsync(sp, 520, 1080);
                var pixelBuffer = await renderTargetBitmap.GetPixelsAsync();

                StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
                var tileFile = await storageFolder.CreateFileAsync(name, CreationCollisionOption.ReplaceExisting);

                // Encode the image to the selected file on disk
                using (var fileStream = await tileFile.OpenAsync(FileAccessMode.ReadWrite)) {
                    var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, fileStream);

                    encoder.SetPixelData(
                        BitmapPixelFormat.Bgra8,
                        BitmapAlphaMode.Ignore,
                        (uint)renderTargetBitmap.PixelWidth,
                        (uint)renderTargetBitmap.PixelHeight,
                        DisplayInformation.GetForCurrentView().LogicalDpi,
                        DisplayInformation.GetForCurrentView().LogicalDpi,
                        pixelBuffer.ToArray());

                    await encoder.FlushAsync();
                }
                return tileFile;
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                //new MessageDialog(ex.Message, "Error").ShowAsync();
                return null;
            }
        }
    }
}
