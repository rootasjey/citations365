using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace citations365.Models {
    public class Author {
        /// <summary>
        /// Author's name
        /// </summary>
        private string _name { get; set; }

        /// <summary>
        /// Author's link
        /// </summary>
        private string _link { get; set; }

        public string Name {
            get {
                return _name;
            }
            set {
                if (value != _name) {
                    _name = value;
                }
            }
        }

        public string Link {
            get {
                return _link;
            }
            set {
                if (value != _link) {
                    _link = value;
                }
            }
        }
    }
}
