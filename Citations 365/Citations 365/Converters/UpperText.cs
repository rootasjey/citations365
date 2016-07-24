using System;
using Windows.UI.Xaml.Data;

namespace citations365.Converters {
    public class UpperText : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            return ((string)value).ToUpper();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }
    }
}
