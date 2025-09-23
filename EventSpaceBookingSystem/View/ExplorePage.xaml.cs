using System;
using EventSpaceBookingSystem.Helpers;
using EventSpaceBookingSystem.Model;
using EventSpaceBookingSystem.ViewModel;
using Microsoft.Maui.Controls;
using Mopups.Services;

namespace EventSpaceBookingSystem.View
{
    public partial class ExplorePage : ContentPage
    {
        private readonly ExplorePageViewModel _viewModel;
        private readonly string _email;

        // Constructor for LandingPage (no login)
        public ExplorePage(EventModel selectedEvent)
            : this(string.Empty, string.Empty, selectedEvent) { }

        // Constructor for HomePage, VenuePage (with username)
        public ExplorePage(string username, EventModel selectedEvent)
            : this(username, string.Empty, selectedEvent) { }

        // Constructor for CartPage or future pages (with email)
        public ExplorePage(string username, string email, EventModel selectedEvent)
        {
            InitializeComponent();
            _viewModel = new ExplorePageViewModel(username, email, selectedEvent);
            BindingContext = _viewModel;
            _email = email;
        }


        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // 🔁 Try to fetch missing email using the username
            if (string.IsNullOrWhiteSpace(_email) && !string.IsNullOrWhiteSpace(_viewModel.Username))
            {
                var users = await Services.UserFileService.LoadUsersAsync();
                var currentUser = users.FirstOrDefault(u => u.Username.Equals(_viewModel.Username, StringComparison.OrdinalIgnoreCase));

                if (currentUser != null)
                {
                    _viewModel.Email = currentUser.Email; // ✅ ViewModel updated
                }
            }

            await _viewModel.LoadVenueDetailsAsync();
        }


        private async void OnPageAppearing(object sender, EventArgs e)
        {
            await _viewModel.LoadVenueDetailsAsync();
        }

        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private void OnBookmarkButtonClicked(object sender, EventArgs e)
        {
            _viewModel.ToggleBookmark();
        }

        private void OnMenuButtonClicked(object sender, EventArgs e)
        {
            DisplayActionSheet("Options", "Cancel", null, "Share", "Report", "Add to favorites");
        }

        private async void OnImageTapped(object sender, EventArgs e)
        {
            await DisplayAlert("Image Viewer", "Image tap handler (gallery feature) coming soon.", "OK");
        }

        private async void OnAddImageButtonClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Feature", "This feature is only available for venue owners", "OK");
        }

        private async void OnHomeClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_viewModel.Username))
            {
                await DisplayAlert("Login Required", "Please log in to book this venue.", "OK");
                await Navigation.PushAsync(new LoginPage());
                return;
            }

            if (!string.IsNullOrWhiteSpace(_viewModel.Username))
                await Navigation.PushAsync(new HomePage(_viewModel.Username, _viewModel.Email));
        }

        private async void OnExploreClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_viewModel.Username))
            {
                await DisplayAlert("Login Required", "Please log in to book this venue.", "OK");
                await Navigation.PushAsync(new LoginPage());
                return;
            }
            if (!string.IsNullOrWhiteSpace(_viewModel.Username))
                await Navigation.PushAsync(new VenuePage(_viewModel.Username, _viewModel.Email));
        }

        private async void OnCartClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_viewModel.Username))
            {
                await DisplayAlert("Login Required", "Please log in to book this venue.", "OK");
                await Navigation.PushAsync(new LoginPage());
                return;
            }
            if (!string.IsNullOrWhiteSpace(_viewModel.Username))
                await Navigation.PushAsync(new CartPage(_viewModel.Username, _viewModel.Email));
        }

        private async void OnBecomeOwnerClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new EventSpaceOwnerJoinPage(_viewModel.Username, _viewModel.Email));
        }

        private async void OnNotificationClicked(object sender, EventArgs e)
        {
            try
            {
                string currentUsername = _viewModel.Username; // <-- Make sure this field is set from login

                var users = await Services.UserFileService.LoadUsersAsync();
                var currentUser = users.FirstOrDefault(u => u.Username.Equals(currentUsername, StringComparison.OrdinalIgnoreCase));

                if (currentUser != null)
                {
                    int userId = currentUser.Id;

                    // Open the Notification MopUp
                    await MopupService.Instance.PushAsync(new NotificationMopups(userId));

                }
                else
                {
                    await DisplayAlert("Error", "User not found.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Something went wrong: {ex.Message}", "OK");
            }
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            if (sender is VisualElement button)
            {
                var location = await button.GetAbsoluteLocationAsync();
                var size = button.Bounds; // get button size

                if (location != null)
                {
                    // Use dynamic positioning (anchor + button height = popup top)
                    var anchor = new Point(location.Value.X, location.Value.Y + size.Height);
                    var popup = new UserProfileMopUp(_viewModel.Username, anchor);
                    await MopupService.Instance.PushAsync(popup);
                }
            }
        }

        private async void OnBookNowClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_viewModel.Username))
            {
                await DisplayAlert("Login Required", "Please log in to book this venue.", "OK");
                await Navigation.PushAsync(new LoginPage());
                return;
            }

            BookingOptionsFrame.IsVisible = true;
        }

        private async void OnConfirmBookingClicked(object sender, EventArgs e)
        {
            BookingOptionsFrame.IsVisible = false;

            bool success = await ProcessBooking();

            if (success)
            {
                await DisplayAlert("Success", "Your booking has been confirmed!", "OK");
            }
            else
            {
                await DisplayAlert("Error", "Unable to process your booking. Please try again.", "OK");
            }
        }

        private Task<bool> ProcessBooking()
        {
            return Task.FromResult(true);
        }
    }
}
