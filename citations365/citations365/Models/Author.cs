using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundTasks.Models {
    public class Author {
        private string _name { get; set; }

        private string _link { get; set; }

        private string _imageLink { get; set; }

        /// <summary>
        /// Author's name
        /// </summary>
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

        /// <summary>
        /// Author's link
        /// </summary>
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

        /// <summary>
        /// Author picture
        /// </summary>
        public string ImageLink {
            get {
                return _imageLink;
            }
            set {
                if (_imageLink != value) {
                    _imageLink = value;
                }
            }
        }
    }
}
