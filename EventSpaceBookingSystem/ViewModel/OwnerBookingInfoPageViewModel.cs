using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using EventSpaceBookingSystem.Model;
using EventSpaceBookingSystem.Services;

namespace EventSpaceBookingSystem.ViewModel
{
    public class OwnerBookingInfoPageViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<BookingDisplayItem> PendingBookings { get; set; } = new();
        public ICommand AcceptCommand { get; }
        public ICommand RejectCommand { get; }

        private readonly int _ownerId;

        public OwnerBookingInfoPageViewModel(int ownerId)
        {
            _ownerId = ownerId;
            AcceptCommand = new Command<BookingDisplayItem>(async (item) => await OnAccept(item));
            RejectCommand = new Command<BookingDisplayItem>(async (item) => await OnReject(item));
            LoadPendingBookings();
        }

        private async void LoadPendingBookings()
        {
            var users = await UserFileService.LoadUsersAsync();
            var allPending = new ObservableCollection<BookingDisplayItem>();

            // Load event titles owned by current owner
            var ownerEventSpaces = await SpaceOwnerFileService.LoadEventModelsAsync(_ownerId);
            var ownerTitles = ownerEventSpaces.Select(e => e.Title).ToHashSet();

            foreach (var user in users)
            {
                var bookings = await UserFileService.LoadBookingsAsync(user.Username);
                foreach (var booking in bookings)
                {
                    if (booking.Status == "Pending" && ownerTitles.Contains(booking.Headline))
                    {
                        allPending.Add(new BookingDisplayItem
                        {
                            Booking = booking,
                            Username = user.Username,
                            Email = user.Email
                        });
                    }
                }
            }

            PendingBookings = allPending;
            OnPropertyChanged(nameof(PendingBookings));
        }



        private async Task OnAccept(BookingDisplayItem item)
        {
            if (item == null) return;

            await UserFileService.UpdateBookingStatusAsync(item.Username, item.Booking.BookingId, "Confirmed");

            var users = await UserFileService.LoadUsersAsync();
            var user = users.FirstOrDefault(u => u.Username == item.Username);
            if (user != null)
            {
                UserFileService.AddNotificationById(user.Id, "Booking Accepted", $"Your booking for '{item.Booking.Headline}' has been accepted.");
            }

            PendingBookings.Remove(item);
        }

        private async Task OnReject(BookingDisplayItem item)
        {
            if (item == null) return;

            await UserFileService.UpdateBookingStatusAsync(item.Username, item.Booking.BookingId, "Rejected");

            var users = await UserFileService.LoadUsersAsync();
            var user = users.FirstOrDefault(u => u.Username == item.Username);
            if (user != null)
            {
                UserFileService.AddNotificationById(user.Id, "Booking Rejected", $"Your booking for '{item.Booking.Headline}' has been rejected.");
            }

            PendingBookings.Remove(item);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class BookingDisplayItem
    {
        public Booking Booking { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
    }
}
