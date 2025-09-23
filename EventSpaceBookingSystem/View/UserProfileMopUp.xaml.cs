using Mopups.Pages;
using Mopups.Services;
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Storage;
using EventSpaceBookingSystem.ViewModel;
using EventSpaceBookingSystem.Services;
using EventSpaceBookingSystem.View;
using System.IO;

namespace EventSpaceBookingSystem
{
    public partial class UserProfileMopUp : PopupPage
    {
        public UserProfileMopUpViewModel ViewModel { get; private set; }

        public UserProfileMopUp(string username, Point anchorPosition)
        {
            InitializeComponent();

            ViewModel = new UserProfileMopUpViewModel();
            BindingContext = ViewModel;
            ViewModel.LoadUserProfileAsync(username);

            AbsoluteLayout.SetLayoutBounds(this, new Rect(anchorPosition.X, anchorPosition.Y, -1, -1));
            AbsoluteLayout.SetLayoutFlags(this, AbsoluteLayoutFlags.None);
        }

        private async void Logout_Clicked(object sender, EventArgs e)
        {
            await MopupService.Instance.PopAsync();
            Preferences.Remove("LoggedInUsername");
            await Application.Current.MainPage.DisplayAlert("Logout", "You have been logged out.", "OK");
            Application.Current.MainPage = new NavigationPage(new View.LandingPage());
        }
        private async void OnBackgroundTapped(object sender, EventArgs e)
        {
            await Mopups.Services.MopupService.Instance.PopAsync();
        }


        private async void OnAvatarTapped(object sender, EventArgs e)
        {
            try
            {
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Select your profile image",
                    FileTypes = FilePickerFileType.Images
                });

                if (result == null)
                {
                    await Application.Current.MainPage.DisplayAlert("Cancelled", "Image selection was cancelled.", "OK");
                    return;
                }

                int userId = ViewModel.UserId;

                if (userId <= 0)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Invalid user ID. Cannot save avatar.", "OK");
                    return;
                }

                string savedPath = await UserFileService.SaveUserAvatarAsync(userId, result);

                if (string.IsNullOrEmpty(savedPath))
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to save the avatar image.", "OK");
                    return;
                }

                ViewModel.AvatarPath = savedPath;
            }
            catch (UnauthorizedAccessException)
            {
                await Application.Current.MainPage.DisplayAlert("Permission Denied", "App does not have permission to access files.", "OK");
            }
            catch (IOException ioEx)
            {
                await Application.Current.MainPage.DisplayAlert("File Error", $"File access error: {ioEx.Message}", "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Unexpected Error", $"Something went wrong:\n{ex.Message}", "OK");
            }
        }

        private async void OnRecentBookingTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CartPage(ViewModel.Username, ViewModel.Email));
            await Mopups.Services.MopupService.Instance.PopAsync(); // Close popup first
        }


    }
}
