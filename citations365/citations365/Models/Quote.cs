using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace citations365.Models
{
    public class Quote {
        private string _content;
        private string _author;
        private string _authorLink;
        private string _date;
        private string _reference;
        private string _link;


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

        public string content {
            get {
                return _content;
            }
            set {
                if (_content != value) {
                    _content = value;
                }
            }
        }

        public string author {
            get {
                return _author;
            }
            set {
                if (_author != value) {
                    _author = value;
                }
            }
        }

        public string authorLink {
            get {
                return _authorLink;
            }
            set {
                if (_authorLink != value) {
                    _authorLink = value;
                }
            }
        }

        public string date {
            get {
                return _date;
            }
            set {
                if (_date != value) {
                    _date = value;
                }
            }
        }

        public string reference {
            get {
                return _reference;
            }
            set {
                if (_reference != value) {
                    _reference = value;
                }
            }
        }

        public string link {
            get {
                return _link;
            }
            set {
                if (_link != value) {
                    _link = value;
                }
            }
        }
    }
}
