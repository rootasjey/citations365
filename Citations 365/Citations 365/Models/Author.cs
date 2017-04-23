using System;
using System.ComponentModel;

namespace citations365.Models {
    public class Author : INotifyPropertyChanged {
        private string _Name { get; set; }

        private string _Link { get; set; }

        private string _ImageLink { get; set; }

        public string Name {
            get {
                return _Name;
            }
            set {
                if (value != _Name) {
                    _Name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }
        
        public string Link {
            get {
                return _Link;
            }
            set {
                if (value != _Link) {
                    _Link = value;
                    NotifyPropertyChanged("Link");
                }
            }
        }
        
        public string ImageLink {
            get {
                return _ImageLink;
            }
            set {
                if (_ImageLink != value) {
                    _ImageLink = value;
                    NotifyPropertyChanged("ImageLink");
                }
            }
        }

        private string _Biography;

        public string Biography {
            get { return _Biography; }
            set {
                _Biography = value;
                NotifyPropertyChanged("Biography");
            }
        }

        private string _Birth;

        public string Birth {
            get { return _Birth; }
            set {
                _Birth = value;
                NotifyPropertyChanged("Birth");
            }
        }

        private string _Death;

        public string Death {
            get { return _Death; }
            set {
                _Death = value;
                NotifyPropertyChanged("Death");
            }
        }

        private string _Job;

        public string Job {
            get { return _Job; }
            set {
                _Job = value;
                NotifyPropertyChanged("Job");
            }
        }

        private string _Quote;

        public string Quote {
            get { return _Quote; }
            set {
                _Quote = value;
                NotifyPropertyChanged("Quote");
            }
        }

        private string _LifeTime;

        public string LifeTime {
            get { return _LifeTime; }
            set {
                _LifeTime = value;
                NotifyPropertyChanged("LifeTime");
            }
        }

        private string _Picture;

        public string Picture {
            get { return _Picture; }
            set {
                _Picture = value;
                NotifyPropertyChanged("Picture");
            }
        }

        private bool _IsLoading;

        public bool IsLoading {
            get { return _IsLoading; }
            set {
                _IsLoading = value;
                NotifyPropertyChanged("IsLoading");
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
