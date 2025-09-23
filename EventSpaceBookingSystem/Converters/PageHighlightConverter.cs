using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace EventSpaceBookingSystem.Converters
{
    public class PageHighlightConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2 || values[0] == null || values[1] == null)
                return Color.FromArgb("#F5F5F5");

            int pageNumber = (int)values[0];
            int currentPage = (int)values[1];

            return pageNumber == currentPage ? Color.FromArgb("#686868") : Color.FromArgb("#F5F5F5");
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
