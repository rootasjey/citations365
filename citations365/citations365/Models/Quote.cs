using System;
using System.ComponentModel;

namespace BackgroundTasks.Models {
    public class Quote : INotifyPropertyChanged {
        private string _content;
        private string _author;
        private string _authorLink;
        private string _date;
        private string _reference;
        private string _link;
        private char _isFavorite;

        /// <summary>
        /// Favorite symbol icon
        /// </summary>
        public static char FavoriteIcon = '\uE00B';

        /// <summary>
        /// UnFavorite symbol icon
        /// </summary>
        public static char UnFavoriteIcon = '\uE006';

        /// <summary>
        /// Full constructor
        /// </summary>
        /// <param name="content">content</param>
        /// <param name="author">author</param>
        /// <param name="authorLink">author's link biography</param>
        /// <param name="date">quote's date</param>
        /// <param name="reference">reference</param>
        /// <param name="link">quote's link</param>
        public Quote(string content     = "", 
                     string author      = "",
                     string authorLink  = "", 
                     string date        = "", 
                     string reference   = "", 
                     string link        = "") {

            _content    = content;
            _author     = author;
            _authorLink = authorLink;
            _date       = date;
            _reference  = reference;
            _link       = link;
        }

        /// <summary>
        /// Empty constructor
        /// </summary>
        public Quote() {

        }

        public string Content {
            get {
                return _content;
            }
            set {
                if (_content != value) {
                    _content = value;
                }
            }
        }

        public string Author {
            get {
                return _author;
            }
            set {
                if (_author != value) {
                    _author = value;
                }
            }
        }

        public string AuthorLink {
            get {
                return _authorLink;
            }
            set {
                if (_authorLink != value) {
                    _authorLink = value;
                }
            }
        }

        public string Date {
            get {
                return _date;
            }
            set {
                if (_date != value) {
                    _date = value;
                }
            }
        }

        public string Reference {
            get {
                return _reference;
            }
            set {
                if (_reference != value) {
                    _reference = value;
                }
            }
        }

        public string Link {
            get {
                return _link;
            }
            set {
                if (_link != value) {
                    _link = value;
                }
            }
        }

        public char IsFavorite {
            get {
                return _isFavorite;
            }
            set {
                if (_isFavorite != value) {
                    _isFavorite = value;
                    NotifyPropertyChanged("IsFavorite");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName) {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
