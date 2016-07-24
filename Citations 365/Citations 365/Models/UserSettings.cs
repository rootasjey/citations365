using Windows.UI.Xaml;

namespace citations365.Models {
    public class UserSettings {
        /// <summary>
        /// Tells if the app is on offline mode
        /// </summary>
        private bool _offline = false;

        /// <summary>
        /// Tells if the text to speech is ON
        /// </summary>
        private bool _TTSIsActivated = false;

        /// <summary>
        /// Tells which background style the user chosed
        /// </summary>
        private string _appBackground = "";

        private string _appBackgroundURL = "";

        private string _appBackgroundName = "";

        /// <summary>
        /// Bing Search secret key
        /// </summary>
        private const string _bingSearchKey = "pCzCBMoEJtZ76ni+ge9sbAYr5PXDfe2ksLPW63wxcVs= ";

        private ApplicationTheme _applicationTheme = ApplicationTheme.Dark;

        public ApplicationTheme applicationTheme {
            get {
                return _applicationTheme;
            }

            set {
                if (_applicationTheme != value) {
                    _applicationTheme = value;
                }
            }
        }

        /// <summary>
        /// Tells if the app is on offline mode
        /// </summary>
        public bool Offline {
            get {
                return _offline;
            }
            set {
                if (value != _offline) {
                    _offline = value;
                }
            }
        }

        /// <summary>
        /// Tells if the text to speech is ON
        /// </summary>
        public bool TTSIsActivated {
            get {
                return _TTSIsActivated;
            }
            set {
                if (value != _TTSIsActivated) {
                    _TTSIsActivated = value;
                }
            }
        }

        /// <summary>
        /// Tells which background style the user chosed
        /// </summary>
        public string AppBackground {
            get {
                return _appBackground;
            }
            set {
                if (value != _appBackground) {
                    _appBackground = value;
                }
            }
        }

        public string AppBackgroundURL {
            get {
                return _appBackgroundURL;
            }
            set {
                if (value != _appBackgroundURL) {
                    _appBackgroundURL = value;
                }
            }
        }

        public string AppBackgroundName {
            get {
                return _appBackgroundName;
            }
            set {
                if (value != _appBackgroundName) {
                    _appBackgroundName = value;
                }
            }
        }

        /// <summary>
        /// Tells which background style the user chosed
        /// </summary>
        public string BingSearchKey {
            get {
                return _bingSearchKey;
            }
        }
    }
}
