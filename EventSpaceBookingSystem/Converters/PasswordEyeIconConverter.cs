using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace EventSpaceBookingSystem.Converters
{
    public class PasswordEyeIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isPasswordVisible)
                return isPasswordVisible ? "eye_open.png" : "eye_closed.png";
            return "eye_closed.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
