using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace citations365.Converters {
    public class TextVisibility : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            return string.IsNullOrEmpty((string)value) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }
    }
}
