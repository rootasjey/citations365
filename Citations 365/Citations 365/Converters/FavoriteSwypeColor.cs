using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace citations365.Converters {
    public class FavoriteSwypeColor : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            if ((bool)value) {
                return new SolidColorBrush((Color)parameter);
            }
            return new SolidColorBrush(new Color() { R = 231, G = 76, B = 60, A = 255 });
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }
    }
}
