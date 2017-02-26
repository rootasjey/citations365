namespace Tasks.Models {
    public sealed class Quote {
        private string _content;
        private string _author;
        private string _authorLink;
        private string _date;
        private string _reference;
        private string _link;
        private char _isFavorite;

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
                }
            }
        }
    }
}

