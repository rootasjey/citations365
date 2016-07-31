using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;

namespace citations365.Helpers {
    public class ImageHelper {
        public async static Task<BitmapImage> LoadImage(Uri uri) {
            BitmapImage bitmapImage = new BitmapImage();

            try {
                using (HttpClient client = new HttpClient()) {
                    using (var response = await client.GetAsync(uri)) {
                        response.EnsureSuccessStatusCode();

                        using (IInputStream inputStream = await response.Content.ReadAsInputStreamAsync()) {
                            await bitmapImage.SetSourceAsync((IRandomAccessStream)inputStream.AsStreamForRead());
                        }
                    }
                }
                return bitmapImage;
            } catch (Exception ex) {
                //Debug.WriteLine("Failed to load the image: {0}", ex.Message);
            }

            return null;
        }

        public static async Task<StorageFile> SaveFile(string name, Uri uri) {
            StorageFile file = await StorageFile.CreateStreamedFileFromUriAsync(name, uri, RandomAccessStreamReference.CreateFromUri(uri));
            StorageFile wall = await file.CopyAsync(ApplicationData.Current.LocalFolder, name, NameCollisionOption.ReplaceExisting);
            return wall;
        }

        public static async Task<StorageFile> GetFile(string name) {
            return await ApplicationData.Current.LocalFolder.GetFileAsync(name);
        }

        public static async Task<StorageFile> SaveLockscreenImage(string name, Uri uri) {
            return await SaveFile(name, uri);
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
    }
}
