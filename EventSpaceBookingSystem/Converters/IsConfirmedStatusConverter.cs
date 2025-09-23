using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace EventSpaceBookingSystem.Converters
{
    public class IsConfirmedStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status == "Confirmed"; // ✅ Only show for unpaid but confirmed bookings
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
