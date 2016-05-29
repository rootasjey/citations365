using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace citations365.Presentation
{
    public class MenuItem : NotifyPropertyChanged
    {
        private string _icon;
        private string _label;
        private Type _pageType;


        public string Icon
        {
            get { return this._icon; }
            set { Set(ref this._icon, value); }
        }

        public char SymbolAsChar {
            get; set;
        }

        public string Label
        {
            get { return this._label; }
            set { Set(ref this._label, value); }
        }

        public Type PageType
        {
            get { return this._pageType; }
            set { Set(ref this._pageType, value); }
        }
    }
}
