using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace citations365.Converters {
    public class EmptyViewVisibility : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            return string.IsNullOrEmpty((string)value) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }
    }
}
