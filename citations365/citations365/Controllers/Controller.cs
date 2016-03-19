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
using Windows.Storage;
using Windows.UI;
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

        /// <summary>
        /// Searched quotes collection
        /// </summary>
        public ObservableCollection<Quote> searchCollection { get; private set; }

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
        /// Add a quote to the favorites quotes list if it's not already added
        /// NOTE: This function is part of the Controller Class since it's used
        /// by multiple Views.
        /// </summary>
        /// <param name="quote">The quote to add to the favorites list</param>
        /// <returns>True if the quote has been correctly added. False if there was an error</returns>
        public bool AddToFavorites(Quote quote)
        {
            // Test the quote's presence with its link
            return false;
        }

        /// <summary>
        /// Remove a quote from the favorites quotes list if it exists
        /// NOTE: This function is part of the Controller Class since it's used
        /// by multiple Views.
        /// </summary>
        /// <param name="quote">The quote to add to remove from favorites list</param>
        /// <returns>True if the quote has been correctly removed. False if there was an error</returns>
        public bool RemoveFromFavorites(Quote quote)
        {
            // Test the quote's presence with its link
            return false;
        }


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
        public void UpdateTile(Quote quote) {

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
            if (quote.Author.Contains("Vos avis")) {
                quote.Author = "Anonyme";
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
