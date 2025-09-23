using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Input;
using CommunityToolkit.Maui.Views;
using EventSpaceBookingSystem.Model;
using EventSpaceBookingSystem.Services;
using EventSpaceBookingSystem.View;
using Microsoft.Maui.Controls;

namespace EventSpaceBookingSystem.ViewModel
{
    public class CartPageViewModel : INotifyPropertyChanged
    {
        private string _username;
        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        private string _email;
        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        private ObservableCollection<Booking> _bookings;
        public ObservableCollection<Booking> Bookings
        {
            get => _bookings;
            set { _bookings = value; OnPropertyChanged(); }
        }

        public ICommand CancelCommand { get; }
        public ICommand PayNowCommand { get; }
        public ICommand RateNowCommand { get; }

        public CartPageViewModel(string username, string email)
        {
            Username = username;
            Email = email; 

            LoadBookings(username);

            CancelCommand = new Command<Booking>(OnCancel);
            PayNowCommand = new Command<Booking>(OnPayNow);
            RateNowCommand = new Command<Booking>(OnRateNow); // ✅
        }


        private async void LoadBookings(string username)
        {
            var bookingVM = new BookingViewModel();
            var loadedBookings = await bookingVM.LoadBookingsAsync(username);

            foreach (var booking in loadedBookings)
            {
                // Use the UserFileService helper to resolve the path
                var resolvedPath = UserFileService.ResolveImagePath(booking.Image);

                // Convert to valid file:// URI if it's a real path
                booking.Image = resolvedPath.Contains("event_placeholder.png")
                    ? "event_placeholder.png"
                    : $"file://{resolvedPath.Replace("\\", "/")}";
            }

            Bookings = new ObservableCollection<Booking>(loadedBookings);
        }


        private async void OnCancel(Booking booking)
        {
            if (booking == null) return;

            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Cancel Booking",
                $"Are you sure you want to cancel your booking for '{booking.Headline}'?",
                "Yes", "No");

            if (!confirm) return;

            try
            {
                var bookings = await UserFileService.LoadBookingsAsync(Username);

                var toRemove = bookings.FirstOrDefault(b => b.BookingId == booking.BookingId);
                if (toRemove != null)
                {
                    bookings.Remove(toRemove);
                    await UserFileService.SaveAllBookingsAsync(Username, bookings);

                    // 🛎️ Add notification
                    var users = await UserFileService.LoadUsersAsync();
                    var user = users.FirstOrDefault(u => u.Username == Username);
                    if (user != null)
                    {
                        UserFileService.AddNotificationById(user.Id, "Booking Cancelled", $"You have cancelled your booking for '{booking.Headline}'.");
                    }

                    // 🛎️ Notify owner
                    int ownerId = await UserFileService.FindOwnerIdByEventTitleAsync(booking.Headline);
                    if (ownerId > 0)
                    {
                        SpaceOwnerFileService.AddNotificationByOwnerId(ownerId, "Booking Cancelled",
                            $"{Username} canceled your '{booking.Headline}' with BookingID {booking.BookingId}.");
                    }
                    Bookings.Remove(toRemove);

                    await Application.Current.MainPage.DisplayAlert("Cancelled", "Your booking has been cancelled.", "OK");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Booking not found.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"An error occurred while cancelling: {ex.Message}", "OK");
            }

            await Application.Current.MainPage.Navigation.PushAsync(new CartPage(Username, Email));
        }



        private async void OnPayNow(Booking booking)
        {
            if (booking == null) return;

            // Navigate to the new PaymentPage with username and booking info
            await Application.Current.MainPage.Navigation.PushAsync(new PaymentPage(Username, Email, booking));

        }
        private async void OnRateNow(Booking booking)
        {
            if (booking == null) return;

            var popup = new RatePopup();
            var result = await Application.Current.MainPage.ShowPopupAsync(popup);

            if (result is int rating && rating >= 1 && rating <= 5)
            {
                booking.Rating = rating;

                var allBookings = await UserFileService.LoadBookingsAsync(Username);
                var matched = allBookings.FirstOrDefault(b => b.BookingId == booking.BookingId);
                if (matched != null)
                {
                    matched.Rating = rating;
                    matched.Status = "Rated Successfully";
                    await UserFileService.SaveAllBookingsAsync(Username, allBookings);
                }

                await Application.Current.MainPage.DisplayAlert("Thank you!", $"You rated this service {rating} star(s).", "OK");

                // ✅ Compute average from all ratings
                double average = allBookings.Where(b => b.Rating > 0).Average(b => b.Rating);
                await File.WriteAllTextAsync(
                    Path.Combine(FileSystem.AppDataDirectory, $"Ratings_{Username}.txt"),
                    $"Average Rating: {average:F2}"
                );

                // ✅ Update the event's rating in EO-{ownerId}.txt
                int ownerId = await UserFileService.FindOwnerIdByEventTitleAsync(booking.Headline);
                if (ownerId > 0)
                {
                    string eventFile = Path.Combine(UserFileService.GetJsonDirectory, $"EO-{ownerId}.txt");
                    var ownerEvents = await UserFileService.LoadEventModelsAsync(eventFile);

                    var targetEvent = ownerEvents.FirstOrDefault(e => e.Title == booking.Headline);
                    if (targetEvent != null)
                    {
                        targetEvent.Rating = (int)average;

                        string updatedJson = JsonSerializer.Serialize(ownerEvents, new JsonSerializerOptions { WriteIndented = true });
                        await File.WriteAllTextAsync(eventFile, updatedJson);
                    }

                    // 🛎️ Notify owner
                    SpaceOwnerFileService.AddNotificationByOwnerId(ownerId, "Booking Rated",
                        $"{Username} rated your '{booking.Headline}' with {rating} stars.");
                }
            }
        }






        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
