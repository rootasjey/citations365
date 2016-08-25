using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace citations365.Helpers {
    public class ImageHelper {
        private static async Task<StorageFile> DownloadImagefromServer(string URI, string filename) {
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

        public static async Task<StorageFile> SaveFile(string name, Uri uri) {
            StorageFile file = await StorageFile.CreateStreamedFileFromUriAsync(name, uri, RandomAccessStreamReference.CreateFromUri(uri));
            StorageFile wall = await file.CopyAsync(ApplicationData.Current.LocalFolder, name, NameCollisionOption.ReplaceExisting);
            return wall;
        }

        public static async Task<StorageFile> GetFile(string name) {
            return await ApplicationData.Current.LocalFolder.GetFileAsync(name);
        }

        public static async Task<StorageFile> SaveLockscreenImage(string name, string uri) {
            //return await SaveFile(name, uri);
            return await DownloadImagefromServer(uri, name);
        }

        public static async Task<StorageFile> GetLockscreenImage(string name) {
            return await GetFile(name);
        }

        public static async Task<StorageFile> DeleteWallpaper(string name) {
            try {
                StorageFile file = await GetLockscreenImage(name);
                if (file != null) {
                    await file.DeleteAsync();
                }
                return file;
            } catch (FileNotFoundException) {
                return null;
            }
        }

        public static async Task<StorageFile> TakeScreenshot() {
            StackPanel sp = new StackPanel() {
                Background = new SolidColorBrush(Windows.UI.Colors.Teal),
                Width = 200,
                Height = 600,
                Orientation = Orientation.Vertical
            };

            TextBlock txtLine1 = new TextBlock() {
                Text = "Text Line 1",
                Foreground = new SolidColorBrush(Windows.UI.Colors.White),
                Width = 200,
                Height = 30,
                FontSize = 32,
                TextAlignment = TextAlignment.Left,
                Margin = new Thickness(9, 3, 0, 3)
            };

            TextBlock txtLine2 = new TextBlock {
                Text = "Text Line 2",
                Foreground = new SolidColorBrush(Windows.UI.Colors.White),
                Width = 200,
                Height = 30,
                FontSize = 32,
                TextAlignment = TextAlignment.Left,
                Margin = new Thickness(9, 3, 0, 3)
            };

            sp.Children.Add(txtLine1);
            sp.Children.Add(txtLine2);

            sp.UpdateLayout();
            sp.Measure(new Size(200, 200));
            sp.Arrange(new Rect(0, 0, 200, 200));
            sp.UpdateLayout();

            //now lets render this stackpanel on image
            try {
                RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap();
                await renderTargetBitmap.RenderAsync(sp, 200, 200);
                var pixelBuffer = await renderTargetBitmap.GetPixelsAsync();

                StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
                var tileFile = await storageFolder.CreateFileAsync("image.png", CreationCollisionOption.ReplaceExisting);

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
