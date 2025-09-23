using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace EventSpaceBookingSystem.Converters
{
    public class HideIfPaidConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is string status && (status == "Pending" || status == "Confirmed");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }



}
