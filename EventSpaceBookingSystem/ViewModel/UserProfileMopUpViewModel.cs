using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using EventSpaceBookingSystem.Model;
using EventSpaceBookingSystem.Services;
using System.Linq;
using Mopups.Services;
using System.IO;

namespace EventSpaceBookingSystem.ViewModel
{
    public class UserProfileMopUpViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string email;
        private string username;
        private string status;
        private string avatarPath = "user24px.png";
        private Booking recentBooking = new();
        private string recentBookingCategory = "";
        private bool hasBookings = false;
        private int userId;

        public string Email { get => email; set { email = value; OnPropertyChanged(); } }
        public string Username { get => username; set { username = value; OnPropertyChanged(); } }
        public string Status { get => status; set { status = value; OnPropertyChanged(); } }

        public string AvatarPath
        {
            get => avatarPath;
            set { avatarPath = value; OnPropertyChanged(); }
        }

        public Booking RecentBooking
        {
            get => recentBooking;
            set
            {
                recentBooking = value;
                OnPropertyChanged();
                RecentBookingCategory = value?.Category ?? ""; // update category display
            }
        }

        public string RecentBookingCategory
        {
            get => recentBookingCategory;
            set { recentBookingCategory = value; OnPropertyChanged(); }
        }

        public bool HasBookings
        {
            get => hasBookings;
            set { hasBookings = value; OnPropertyChanged(); }
        }

        public int UserId
        {
            get => userId;
            set { userId = value; OnPropertyChanged(); }
        }

        public async void LoadUserProfileAsync(string uname)
        {
            Username = uname;

            var users = await UserFileService.LoadUsersAsync();
            var user = users.FirstOrDefault(u => u.Username == uname);

            if (user != null)
            {
                UserId = user.Id;
                Email = user.Email;
                Username = user.Username;
                Status = user.Status;

                // Load latest avatar
                string avatarDir = Path.Combine(UserFileService.GetJsonDirectory, "User Images", user.Id.ToString());

                if (Directory.Exists(avatarDir))
                {
                    var avatarFiles = Directory.GetFiles(avatarDir, "avatar_*.png")
                                               .OrderByDescending(f => f)
                                               .ToList();
                    AvatarPath = avatarFiles.Any() ? avatarFiles.First() : "user24px.png";
                }
                else
                {
                    AvatarPath = "user24px.png";
                }

                // Load bookings
                var bookings = await UserFileService.LoadBookingsAsync(uname);
                if (bookings != null && bookings.Any())
                {
                    var latest = bookings.OrderByDescending(b => b.BookingDate).First();

                    // 🔄 Resolve image path to a valid file URI or fallback
                    if (!string.IsNullOrEmpty(latest.Image) && !latest.Image.StartsWith("file://"))
                    {
                        string absolutePath = Path.Combine(UserFileService.GetJsonDirectory, latest.Image.Replace("/", Path.DirectorySeparatorChar.ToString()));

                        if (File.Exists(absolutePath))
                        {
                            latest.Image = $"file://{absolutePath.Replace("\\", "/")}";
                        }
                        else
                        {
                            latest.Image = "event_placeholder.png";
                        }
                    }

                    RecentBooking = latest;
                    HasBookings = true;
                }
                else
                {
                    RecentBooking = new Booking { Image = "event_placeholder.png" };
                    HasBookings = false;
                }
            }
            else
            {
                UserId = -1;
                Email = "";
                Status = "Unknown";
                AvatarPath = "user24px.png";
                HasBookings = false;
                RecentBooking = new Booking { Image = "event_placeholder.png" };
            }
        }


        private void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}