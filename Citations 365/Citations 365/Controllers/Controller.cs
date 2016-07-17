using citations365.Models;
using LLMListView;
using System;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.DataTransfer;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace citations365.Controllers {
    public class Controller {
        /*
         * ***********
         * VARIABLES
         * ***********
         */
        private static Controller _controller = null;

        public static Controller controller {
            get {
                if (_controller == null) {
                    _controller = new Controller();
                }
                return _controller;
            }
        }

        static DataTransferManager _dataTransferManager;

        private static Quote _sharedQuote;

        /*
         * ************
         * CONSTRUCTOR
         * ************
         */
        /// <summary>
        /// Initialize the controller
        /// </summary>
        public Controller() {

        }

        /*
         * ********
         * METHODS
         * ********
         */         
        #region quotes

        /// <summary>
        /// Open the Share UI with the quote's data (share on twitter, facebook, sms, ...)
        /// </summary>
        /// <param name="quote">The quote to share</param>
        public static void share(Quote quote) {
            // If the user clicks the share button, invoke the share flow programatically.
            _sharedQuote = quote; 
            DataTransferManager.ShowShareUI();
        }

        public static void RegisterForShare() {
            _dataTransferManager = DataTransferManager.GetForCurrentView();
            _dataTransferManager.DataRequested += new TypedEventHandler<DataTransferManager,
                DataRequestedEventArgs>(ShareTextHandler);
        }

        public static void ShareTextHandler(DataTransferManager sender, DataRequestedEventArgs e) {
            string text = _sharedQuote.Content + " - " + _sharedQuote.Author;
            if (!string.IsNullOrWhiteSpace(_sharedQuote.Reference)) {
                text += " (" + _sharedQuote.Reference + ")";
            }

            DataRequest request = e.Request;
            request.Data.Properties.Title = "Citations 365";
            request.Data.Properties.Description = "Share a quote";
            request.Data.SetText(text);
        }

        /// <summary>
        /// Copy the quote's content to the clipboard
        /// </summary>
        /// <param name="quote">The quote's content to copy</param>
        public void Copy(Quote quote) {
            DataPackage dataPackage = new DataPackage();
            dataPackage.RequestedOperation = DataPackageOperation.Copy;
            dataPackage.SetText(quote.Content + " - " + quote.Author);
            Clipboard.SetContent(dataPackage);
        }

        #endregion quotes

        /// <summary>
        /// Update the application's tile with the most recent quote
        /// </summary>
        /// <param name="quote">The quote's content to update the tile with</param>
        public static void UpdateTile(Quote quote) {
            XmlDocument tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWideText09);
            XmlNodeList tileTextAttributes = tileXml.GetElementsByTagName("text");

            tileTextAttributes[0].InnerText = quote.Author;
            tileTextAttributes[1].InnerText = quote.Content;

            // Square tile
            XmlDocument squareTileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150Text02);
            XmlNodeList squareTileTextAttributes = squareTileXml.GetElementsByTagName("text");
            //squareTileTextAttributes[0].AppendChild(squareTileXml.CreateTextNode("Hello World! My very own tile notification"));
            squareTileTextAttributes[0].InnerText = quote.Author;
            squareTileTextAttributes[1].InnerText = quote.Content;

            // Integration of the two tile templates
            IXmlNode node = tileXml.ImportNode(squareTileXml.GetElementsByTagName("binding").Item(0), true);
            tileXml.GetElementsByTagName("visual").Item(0).AppendChild(node);

            // Tile Notification
            TileNotification tileNotification = new TileNotification(tileXml);
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
        }

        /// <summary>
        /// Delete HTML tags from the quote props and checks values
        /// </summary>
        /// <param name="quote"></param>
        /// <returns></returns>
        public static Quote Normalize(Quote quote) {
            // Delete HTML
            quote.Author    = DeleteHTMLTags(quote.Author);
            quote.Content   = DeleteHTMLTags(quote.Content);
            quote.Reference = DeleteHTMLTags(quote.Reference);

            // Check values
            if (quote.Reference.Contains("Vos avis")) {
                int index = quote.Reference.IndexOf("Vos avis");
                quote.Reference = quote.Reference.Substring(0, index);
            }
            return quote;
        }

        /// <summary>
        /// Normalize the text:
        /// Remove the html tags (<h1></h1>, ...), ans special chars (&amp;)
        /// </summary>
        /// <param name="text">Normalize</param>
        public static string DeleteHTMLTags(string text)
        {
            if (text == null) {
                return null;
            }

            text = Regex.Replace(text, @"<(.|\n)*?>", string.Empty);

            text = text
                .Replace("&laquo;", "")
                .Replace("&ldquo;", "")
                .Replace("&rdquo;", "")
                .Replace("&nbsp;", "")
                .Replace("&raquo;", "")
                .Replace("&#039;", "'")
                .Replace("&quot;", "'")
                .Replace("&amp;", "&")
                .Replace("[+]", "")
                .Replace("[", "")
                .Replace("]", "")
                .Replace("\n", "")
                .Replace("  ", "");

            return text;
        }


        /* ***********************
         * LISTVIEW SWYPE HERLPERS
         * ***********************
         */
        public static StackPanel Getpanel(object sender, LLM.SwipeDirection direction) {
            var llmItem = sender as LLM.LLMListViewItem;
            StackPanel panel = null;

            if (direction == LLM.SwipeDirection.Right) {
                panel = llmItem.GetSwipeControl<StackPanel>(direction, "RightPanel");
            } else {
                panel = llmItem.GetSwipeControl<StackPanel>(direction, "LeftPanel");
            }

            return panel;
        }

        public static void SwipeMovePanel(StackPanel panel, LLM.SwipeProgressEventArgs args) {
            var cumlative = Math.Abs(args.Cumulative);

            if (panel == null && cumlative - panel.ActualWidth >= 0)
                return;

            if (args.CurrentRate < 0.3 && cumlative - panel.ActualWidth > 0) {
                (panel.RenderTransform as TranslateTransform).X += args.Delta / 2;
            } else if (args.CurrentRate >= 0.3 && args.CurrentRate < 0.4 && cumlative - panel.ActualWidth > 0) {
                (panel.RenderTransform as TranslateTransform).X += args.Delta * 2;
            } else {
                (panel.RenderTransform as TranslateTransform).X = args.SwipeDirection == LLM.SwipeDirection.Left ? cumlative - panel.ActualWidth : panel.ActualHeight - cumlative;
            }
        }

        public static void SwipeReleasePanel(StackPanel panel, LLM.SwipeReleaseEventArgs args) {
            var story = new Storyboard();
            var transform = panel.RenderTransform as TranslateTransform;
            story.Children.Add(Utils.CreateDoubleAnimation(transform, "X", args.EasingFunc, args.ItemToX, args.Duration - 10));
            story.Begin();
        }
    }

    /* **********
     * CONVERTERS
     * **********
     */
    public class FavoriteColorConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            if ((bool)value) {
                return new SolidColorBrush(new Color() { R = 246, G = 71, B = 71, A = 255 }); // red color
            }
            return Application.Current.Resources["ApplicationPageBackgroundThemeBrush"]; // ApplicationForegroundThemeBrush
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }
    }

    public class FavoriteSwypeColorConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            if ((bool)value) {
                return new SolidColorBrush((Color)parameter);
            }
            return new SolidColorBrush(new Color() { R = 231, G = 76, B = 60, A = 255 });
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }
    }

    public class FavoriteIconConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            if ((bool)value) {
                return Quote.FavoriteIcon;
            }
            return Quote.UnFavoriteIcon;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }
    }

    public class FavoriteColorIconConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            return new SolidColorBrush(Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }
    }

    public class ShareSwypeColorConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            if ((bool)value) {
                return new SolidColorBrush((Color)parameter);
            }
            return new SolidColorBrush(new Color() { R = 52, G = 152, B = 219, A = 255 });
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }
    }

    public class TextVisibilityConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            return string.IsNullOrEmpty((string)value) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }
    }

    public class UpperTextConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            return ((string)value).ToUpper();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }
    }

    public class EmptyViewVisibility : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            return string.IsNullOrEmpty((string)value) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }
    }
    
}
