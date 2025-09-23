using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using EventSpaceBookingSystem.Model;

namespace EventSpaceBookingSystem.Converters
{
    public class ShowRateNowButtonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Booking booking)
            {
                return (booking.Status == "Payment Successful" || booking.Status == "Rated Successfully")
                    && booking.BookingDate.Date < DateTime.Now.Date
                    && booking.Rating == 0;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

}
