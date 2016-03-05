using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace citations365.Models
{
    /// <summary>
    /// Data to represent an item in the nav menu.
    /// </summary>
    public class NavMenuItem
    {
        public string Label { get; set; }
        public Symbol Symbol { get; set; }

        // Initial
        //public char SymbolAsChar
        //{
        //    get
        //    {
        //        return (char)this.Symbol;
        //    }
        //}

        public char SymbolAsChar
        {
            get; set;
        }

        //public char SymbolCode { get; set; }

        public Type DestinationPage { get; set; }
        public object Arguments { get; set; }
    }
}
