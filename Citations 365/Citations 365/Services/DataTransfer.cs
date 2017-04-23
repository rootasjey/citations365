using citations365.Models;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.ApplicationModel.DataTransfer;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.UI.Notifications;

namespace citations365.Services {
    public static class DataTransfer {

        static DataTransferManager _dataTransferManager;

        private static Quote _sharingQuote;
        
        public static void Share(Quote quote) {
            _sharingQuote = quote;
            DataTransferManager.ShowShareUI();
        }

        public static void RegisterForShare() {
            _dataTransferManager = DataTransferManager.GetForCurrentView();
            _dataTransferManager.DataRequested += new TypedEventHandler<DataTransferManager,
                DataRequestedEventArgs>(ShareTextHandler);
        }

        public static void ShareTextHandler(DataTransferManager sender, DataRequestedEventArgs e) {
            string text = _sharingQuote.Content + " - " + _sharingQuote.Author;
            if (!string.IsNullOrWhiteSpace(_sharingQuote.Reference)) {
                text += " (" + _sharingQuote.Reference + ")";
            }

            DataRequest request = e.Request;
            request.Data.Properties.Title = "Citations 365";
            request.Data.Properties.Description = "Share a quote";
            request.Data.SetText(text);
        }

        public static void Copy(Quote quote) {
            DataPackage dataPackage = new DataPackage() {
                RequestedOperation = DataPackageOperation.Copy
            };

            dataPackage.SetText(quote.Content + " - " + quote.Author);
            Clipboard.SetContent(dataPackage);

            ShowShareCompleted();
        }

        public static void ShowShareCompleted() {
            ToastContent content = new ToastContent() {
                Visual = new ToastVisual() {
                    BindingGeneric = new ToastBindingGeneric() {
                        Children = {
                            new AdaptiveText() {
                                Text = "Citations 365"
                            },
                            new AdaptiveText() {
                                Text = "Citation copiée!"
                            }
                        }
                    }
                }
            };

            XmlDocument xmlContent = content.GetXml();
            ToastNotification notification = new ToastNotification(xmlContent);
            ToastNotificationManager.CreateToastNotifier().Show(notification);
        }
    }
}
