using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Tasks.Models;
using Windows.Foundation;
using Windows.Storage;

namespace Tasks.Factory {
    public sealed class Settings {
        private const string DAILY_QUOTE = "DailyQuote"; // composite value
        private const string DAILY_QUOTE_CONTENT = "DailyQuoteContent";
        private const string DAILY_QUOTE_AUTHOR = "DailyQuoteAuthor";
        private const string DAILY_QUOTE_AUTHOR_LINK = "DailyQuoteAuthorLink";
        private const string DAILY_QUOTE_REFERENCE = "DailyQuoteReference";
        private const string DAILY_QUOTE_LINK = "DailyQuoteLink";

        private const string DAILY_LIST_FILENAME = "dailyList.txt";
        private const string LAST_REFRESHED_DATA = "LastRefreshedData";

        public static IAsyncOperation<bool> SaveConfigAsync(IList<Quote> quotes, int pick, bool isOutdated) {
            return SaveConfig(quotes, pick, isOutdated).AsAsyncOperation();
        }

        private static async Task<bool> SaveConfig(IList<Quote> quotes, int pick, bool isOutdated) {
            SaveQuoteToStorage(quotes[pick]);
            await SaveQuotesToStorage(quotes);

            if (isOutdated) {
                SaveCurrentTime();
            }

            return true;
        }

        private static void SaveCurrentTime() {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values[LAST_REFRESHED_DATA] = DateTime.Now.ToString();
        }

        public static DateTimeOffset GetLastTimeFetch() {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values.TryGetValue(LAST_REFRESHED_DATA, out object last);

            if (last != null) {
                DateTime time = DateTime.Parse((string)last);
                return time;
            }

            return new DateTime(TimeSpan.FromHours(7).Ticks);
        }

        private static Quote RetrieveDailyQuote() {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            ApplicationDataCompositeValue compositeQuote = (ApplicationDataCompositeValue)localSettings.Values[DAILY_QUOTE];

            Quote quote = new Quote() {
                Content     = (string)compositeQuote[DAILY_QUOTE_CONTENT],
                Author      = (string)compositeQuote[DAILY_QUOTE_AUTHOR],
                AuthorLink  = (string)compositeQuote[DAILY_QUOTE_AUTHOR_LINK],
                Date        = null,
                Reference   = (string)compositeQuote[DAILY_QUOTE_REFERENCE],
                Link        = (string)compositeQuote[DAILY_QUOTE_LINK]
            };

            return quote;
        }

        private static void SaveQuoteToStorage(Quote dailyQuote) {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            ApplicationDataCompositeValue compositeQuote = new ApplicationDataCompositeValue {
                [DAILY_QUOTE_CONTENT] = dailyQuote.Content,
                [DAILY_QUOTE_AUTHOR] = dailyQuote.Author,
                [DAILY_QUOTE_AUTHOR_LINK] = dailyQuote.AuthorLink,
                [DAILY_QUOTE_REFERENCE] = dailyQuote.Reference,
                [DAILY_QUOTE_LINK] = dailyQuote.Link
            };

            localSettings.Values[DAILY_QUOTE] = compositeQuote;
        }
        
        private static async Task SaveQuotesToStorage(IList<Quote> quotesList) {
            try {
                StorageFile savedFile =
                    await ApplicationData.Current.LocalFolder.CreateFileAsync(DAILY_LIST_FILENAME, CreationCollisionOption.ReplaceExisting);

                using (Stream writeStream = await savedFile.OpenStreamForWriteAsync()) {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(IList<Quote>));
                    serializer.WriteObject(writeStream, quotesList);
                    await writeStream.FlushAsync();
                    writeStream.Dispose();
                }

            } catch (Exception e) {
                return;
            }
        }

        public static IAsyncOperation<IList<Quote>> ExtractListAsyncFrom(StorageFile file) {
            return ExtractListFrom(file).AsAsyncOperation();
        }

        private static async Task<IList<Quote>> ExtractListFrom(StorageFile file) {
            try {
                IList<Quote> results = null;

                using (Stream readStream = await file.OpenStreamForReadAsync()) {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(IList<Quote>));
                    results = (IList<Quote>)serializer.ReadObject(readStream);
                    await readStream.FlushAsync();
                    readStream.Dispose();
                }
                return results;
            } catch {
                await file.DeleteAsync();
                return null;
            }
        }
        public static IAsyncOperation<StorageFile> RestoreQuotesFromStorageAsync() {
            return RestoreQuotesFromStorage().AsAsyncOperation();
        }

        private static async Task<StorageFile> RestoreQuotesFromStorage() {
            try {
                return await ApplicationData.Current.LocalFolder.GetFileAsync(DAILY_LIST_FILENAME);
            } catch {
                return null;
            }
        }
    }
}
