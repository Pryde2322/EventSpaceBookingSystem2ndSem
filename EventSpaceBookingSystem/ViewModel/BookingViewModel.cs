using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using EventSpaceBookingSystem.Model;
using EventSpaceBookingSystem.Services;
using EventSpaceBookingSystem.View;

namespace EventSpaceBookingSystem.ViewModel
{
    public class BookingViewModel
    {
        public BookingViewModel() { }

        public async Task<ObservableCollection<Booking>> LoadBookingsAsync(string username, string email = "")
        {
            var saved = await UserFileService.LoadBookingsAsync(username);

            if (saved == null || saved.Count == 0)
            {
                await Application.Current.MainPage.DisplayAlert("No Bookings", "You don't have any bookings yet. Redirecting you to explore venues...", "OK");

                if (!string.IsNullOrWhiteSpace(email))
                {
                    await Application.Current.MainPage.Navigation.PushAsync(new VenuePage(username, email));
                }
                else
                {
                    await Application.Current.MainPage.Navigation.PushAsync(new VenuePage(username));
                }

                // Return empty collection for view binding
                return new ObservableCollection<Booking>();

                // For testing with dummy data:
                // return GetTestBookings(); // ← UNCOMMENT THIS TO TEST
            }

            return saved;
        }

        public ObservableCollection<Booking> GetTestBookings()
        {
            return new ObservableCollection<Booking>
            {
                new Booking
                {
                    Image = "event_placeholder.png",
                    Headline = "Wedding Reception",
                    PublishedDate = "Published: January 10, 2025",
                    BookingDate = new DateTime(2025, 2, 16),
                    Status = "Pending",
                    GuestCount = 300,
                    EventTime = "Evening",
                    ExtraChairs = "Standard",
                    TimeExtension = "N/A",
                    Price = 15000.00m
                }
            };
        }
    }
}
