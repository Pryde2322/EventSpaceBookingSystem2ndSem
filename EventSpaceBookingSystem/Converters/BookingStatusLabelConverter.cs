using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using EventSpaceBookingSystem.Model;

namespace EventSpaceBookingSystem.Converters
{
    public class BookingStatusLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Booking booking)
            {
                if (booking.Status == "Payment Successful")
                {
                    if (booking.BookingDate.Date > DateTime.Now.Date)
                        return "See you!";
                    else if (booking.BookingDate.Date <= DateTime.Now.Date && booking.Rating == 0)
                        return "Rate the Service";
                }

                if (booking.Status == "Rated Successfully")
                    return "Thank You";
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }



}
