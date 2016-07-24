using citations365.Models;
using System;
using Windows.UI.Xaml.Data;

namespace citations365.Converters {
    public class FavoriteIcon : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            if ((bool)value) {
                return Quote.FavoriteIcon;
            }
            return Quote.UnFavoriteIcon;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }
    }
}
