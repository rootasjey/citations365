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

        private static UserSettings _userSettings = null;

        public static UserSettings userSettings {
            get {
                if (_userSettings == null) {
                    _userSettings = new UserSettings();
                }
                return _userSettings;
            }
            set {
                _userSettings = value;
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

        /*
         * **********
         * IO Methods
         * **********
         */
        #region IO
        /// <summary>
        /// Save user's settings (background color, background task, etc.)
        /// </summary>
        /// <returns>True if the settings has been correctly saved. False if there was an error</returns>
        public static async Task<bool> SaveSettings() {
            try {
                await DataSerializer<UserSettings>.SaveObjectsAsync(userSettings, "userSettings.xml");
                return true;
            } catch (IsolatedStorageException exception) {
                // error
                return false;
            }
        }

        /// <summary>
        /// Load user's settings (background color, background task, etc.)
        /// </summary>
        /// <returns>True if the settings has been correctly loaded. False if there was an error</returns>
        public static async Task<bool> LoadSettings() {
            try {
                UserSettings settings = await DataSerializer<UserSettings>.RestoreObjectsAsync("userSettings.xml");
                if (settings != null) {
                    userSettings = settings;
                    return true;
                }
                return false;

            } catch (IsolatedStorageException exception) {
                // error
                return false;
            }
        }

        /// <summary>
        /// Save to IO the authors list
        /// </summary>
        /// <returns>True if the save succeded</returns>
        public static bool SaveAuthors() {
            return false;
        }

        /// <summary>
        /// Load from IO the authors list
        /// </summary>
        /// <returns>True if the retrieve succeded</returns>
        public static bool LoadAuthors() {
            return false;
        }

        #endregion IO

        /// <summary>
        /// Delete HTML tags from the quote props and checks values
        /// </summary>
        /// <param name="quote"></param>
        /// <returns></returns>
        public static Quote Normalize(Quote quote) {
            // Delete HTML
            quote.author    = DeleteHTMLTags(quote.author);
            quote.content   = DeleteHTMLTags(quote.content);
            quote.reference = DeleteHTMLTags(quote.reference);

            // Check values
            if (quote.author.Contains("Vos avis")) {
                quote.author = "Anonyme";
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
}
