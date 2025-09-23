using EventSpaceBookingSystem.Helpers;
using EventSpaceBookingSystem.Model;
using EventSpaceBookingSystem.ViewModel;
using Microsoft.Maui.Controls;
using Mopups.Services;

namespace EventSpaceBookingSystem.View
{
    public partial class PaymentPage : ContentPage
    {
        private readonly PaymentPageViewModel _viewModel;

        public PaymentPage(string username, string email, Booking booking)
        {
            InitializeComponent();
            _viewModel = new PaymentPageViewModel(username, email, booking);
            BindingContext = _viewModel;
        }

        private async void OnHomeClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new HomePage(_viewModel.Username, _viewModel.Email));
        }

        private async void OnExploreClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new VenuePage(_viewModel.Username, _viewModel.Email));
        }

        private async void OnCartClicked(object sender, EventArgs e)
        {
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
    }
}
