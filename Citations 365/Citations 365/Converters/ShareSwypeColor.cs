using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace citations365.Converters {
    public class ShareSwypeColor : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            if ((bool)value) {
                return new SolidColorBrush((Color)parameter);
            }
            return new SolidColorBrush(new Color() { R = 52, G = 152, B = 219, A = 255 });
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }
    }
}
