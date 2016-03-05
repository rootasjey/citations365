using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private string _appBackground = "default";

        /// <summary>
        /// Bing Search secret key
        /// </summary>
        private const string _bingSearchKey = "pCzCBMoEJtZ76ni+ge9sbAYr5PXDfe2ksLPW63wxcVs= ";

        /// <summary>
        /// Tells if the app is on offline mode
        /// </summary>
        public bool offline {
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
        public string appBackground {
            get {
                return _appBackground;
            }
            set {
                if (value != _appBackground) {
                    _appBackground = value;
                }
            }
        }

        /// <summary>
        /// Tells which background style the user chosed
        /// </summary>
        public string bingSearchKey {
            get {
                return _bingSearchKey;
            }
        }
    }
}
