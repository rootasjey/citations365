using citations365.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace citations365.Controllers
{
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
        public void share(Quote quote)
        {

        }


        /// <summary>
        /// Copy the quote's content to the clipboard
        /// </summary>
        /// <param name="quote">The quote's content to copy</param>
        public void Copy(Quote quote)
        {

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

            quote.IsFavorite = FavoritesController.GetFavoriteIcon(quote.Link);

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

    }

    public class FavoriteColorConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            char codeSymbol = (char)value;
            if (codeSymbol == Quote.FavoriteIcon) {
                var color = new Color();
                color.R = 246;
                color.G = 71;
                color.B = 71;
                color.A = 255;
                var redColor = new SolidColorBrush(color);
                return redColor;
            }
            return Application.Current.Resources["ApplicationForegroundThemeBrush"]; ;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }
    }
}
