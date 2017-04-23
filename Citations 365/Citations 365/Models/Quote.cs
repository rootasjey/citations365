using System;
using System.ComponentModel;

namespace citations365.Models {
    public class Quote : INotifyPropertyChanged {
        private string _content;
        private string _author;
        private string _authorLink;
        private string _date;
        private string _reference;
        private string _link;
        private bool _isFavorite;
        private bool _isShared = false;

        /// <summary>
        /// Favorite symbol icon
        /// </summary>
        public static char FavoriteIcon = '\uE00B';

        /// <summary>
        /// UnFavorite symbol icon
        /// </summary>
        public static char UnFavoriteIcon = '\uE006';

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

        public bool IsFavorite {
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

        public bool IsShared {
            get {
                return _isShared;
            }
            set {
                if (_isShared != value) {
                    _isShared = value;
                    NotifyPropertyChanged("IsShared");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
