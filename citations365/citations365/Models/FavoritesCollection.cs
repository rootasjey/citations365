using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace citations365.Models {
    public class FavoritesCollection : KeyedCollection<string, Quote> {
        protected override string GetKeyForItem(Quote item) {
            return item.Link;
        }
    }
}
