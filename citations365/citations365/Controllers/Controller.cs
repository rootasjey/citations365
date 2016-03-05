using citations365.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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

        private UserSettings _userSettings = null;

        /// <summary>
        /// Searched quotes collection
        /// </summary>
        public ObservableCollection<Quote> searchCollection { get; private set; }

        /// <summary>
        /// Favorited quotes collection
        /// </summary>
        public ObservableCollection<Quote> favoritesCollection { get; private set; }

        /*
         * ************
         * CONSTRUCTOR
         * ************
         */
        public Controller() {

        }

        /*
         * ********
         * METHODS
         * ********
         */

        /*
        * ***************
        * Quotes' Methods
        * ***************
        */
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

        /// <summary>
        /// Update the application's tile with the most recent quote
        /// </summary>
        /// <param name="quote">The quote's content to update the tile with</param>
        public void UpdateTile(Quote quote)
        {

        }

        /// <summary>
        /// Return true is there's an internet connection. False elsewhere.
        /// </summary>
        /// <returns>True if the device is connected to internet. False if not.</returns>
        public bool IsConnectionAvailable()
        {
            return false;
        }

        /*
         * **********
         * IO Methods
         * **********
         */
        /// <summary>
        /// Save user's settings (background color, background task, etc.)
        /// </summary>
        /// <returns>True if the settings has been correctly saved. False if there was an error</returns>
        public static bool SaveSettings()
        {
            return false;
        }

        /// <summary>
        /// Load user's settings (background color, background task, etc.)
        /// </summary>
        /// <returns>True if the settings has been correctly loaded. False if there was an error</returns>
        public static bool LoadSettings()
        {
            return false;
        }

        /// <summary>
        /// Save to IO the first quotes from the todayCollection
        /// </summary>
        /// <returns>True if the save succeded</returns>
        public static bool SaveToday() {
            if (TodayController.TodayCollection.Count < 1) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Save to IO the favortites quotes
        /// </summary>
        /// <returns>True if the save succededreturns>
        public static bool SaveFavorites() {
            return false;
        }

        /// <summary>
        /// Save to IO the authors list
        /// </summary>
        /// <returns>True if the save succeded</returns>
        public static bool SaveAuthors() {
            return false;
        }

        /// <summary>
        /// Load from IO the quotes saved before
        /// </summary>
        /// <returns>True if the retrieve succeded</returns>
        public static async Task<bool> LoadToday() {
            return false;
        }

        /// <summary>
        /// Load from IO the favorites quotes list
        /// </summary>
        /// <returns>True if the retrieve succeded</returns>
        public static bool LoadFavorites() {
            return false;
        }

        /// <summary>
        /// Load from IO the authors list
        /// </summary>
        /// <returns>True if the retrieve succeded</returns>
        public static bool LoadAuthors() {
            return false;
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
}
