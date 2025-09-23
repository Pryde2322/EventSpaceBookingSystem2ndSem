using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using EventSpaceBookingSystem.Model;
using EventSpaceBookingSystem.Services;
using System.Linq;
using Mopups.Services;
using System.IO;
using EventSpaceBookingSystem.Helpers;

// Alias to avoid conflict with Windows.System.User
using UsersModel = EventSpaceBookingSystem.Model.Users;

namespace EventSpaceBookingSystem.ViewModel
{
    public class ProfileBoxViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string email;
        private string username;
        private string status;
        private string avatarPath = "user24px.png";
        private int userId;

        public string Email
        {
            get => email;
            set { email = value; OnPropertyChanged(); }
        }

        public string Username
        {
            get => username;
            set { username = value; OnPropertyChanged(); }
        }

        public string Status
        {
            get => status;
            set { status = value; OnPropertyChanged(); }
        }

        public string AvatarPath
        {
            get => avatarPath;
            set { avatarPath = value; OnPropertyChanged(); }
        }

        public int UserId
        {
            get => userId;
            set { userId = value; OnPropertyChanged(); }
        }

        public ProfileBoxViewModel() { }

        public async void LoadUserProfileByIdAsync(int ownerId)
        {
            UsersModel user = await SpaceOwnerFileService.GetOwnerByIdAsync(ownerId);

            if (user != null)
            {
                UserId = user.Id;
                Email = user.Email;
                Username = user.Username;
                Status = user.Status;

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
            }
            else
            {
                UserId = -1;
                Email = "";
                Username = "Unknown";
                Status = "Unknown";
                AvatarPath = "user24px.png";
            }
        }

        public async void LoadUserProfileByIdAsync(string ownerId)
        {
                UserId = -9999;
                Email = "admin@admin.com";
                Username = "Admin";
                Status = "Admin";
                AvatarPath = "user24px.png";
        }

        public async Task OnAvatarTapped()
        {
            await Shell.Current.DisplayAlert("Change Avatar", "Avatar tapped. Image upload can be added here.", "OK");
        }

        public async Task LogoutAsync()
        {
            

            bool confirm = await Shell.Current.DisplayAlert("Log Out", "Are you sure you want to log out?", "Yes", "No");
            if (confirm)
            {
                Session.CurrentOwnerId = -1;
                Session.CurrentOwnerName = string.Empty;
                Session.CurrentOwnerEmail = string.Empty;
                await MopupService.Instance.PopAllAsync();
                await Shell.Current.GoToAsync("//LoginPage");
            }
        }

        private void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
