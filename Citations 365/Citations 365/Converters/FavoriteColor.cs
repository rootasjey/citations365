using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace citations365.Converters {
    public class FavoriteColor : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            if ((bool)value) {
                return new SolidColorBrush(new Color() { R = 246, G = 71, B = 71, A = 255 }); // red color
            }
            return new SolidColorBrush(new Color() { R = 0, G = 0, B = 0, A = 0 }); // transparent
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }
    }
}
