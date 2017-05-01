using System;
using Windows.UI.Xaml.Data;

namespace citations365.Converters {
    public class LowerText: IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            if (value == null) return "";
            return ((string)value).ToLower();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }
    }
}
