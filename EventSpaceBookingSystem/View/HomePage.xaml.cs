using Microsoft.Maui.Controls;
using EventSpaceBookingSystem.ViewModel;
using EventSpaceBookingSystem.Model;
using Mopups.Services;
using EventSpaceBookingSystem.Helpers;

namespace EventSpaceBookingSystem.View
{
    public partial class HomePage : ContentPage
    {
        private readonly HomePageViewModel _viewModel;
        private readonly string _email;

        public HomePage(string username, string email)
        {
            InitializeComponent();
            _viewModel = new HomePageViewModel();
            _viewModel.Username = username;
            _email = email;
            BindingContext = _viewModel;
            // Call async initialization
            Loaded += async (_, _) => await _viewModel.InitAsync(username);
        }

        private async void OnCartClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CartPage(_viewModel.Username, _email));
        }

        private async void OnHomeClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new HomePage(_viewModel.Username, _email));
        }

        private async void OnExploreClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new VenuePage(_viewModel.Username, _email));
        }

        private async void OnBecomeOwnerClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new EventSpaceOwnerJoinPage(_viewModel.Username, _email));
        }

        private async void OnSearchClicked(object sender, EventArgs e)
        {
            if (_viewModel.SearchCommand.CanExecute(null))
            {
                _viewModel.SearchCommand.Execute(null);
            }

            await DisplayAlert("Search", $"Searching for '{_viewModel.SearchQuery}' in '{_viewModel.LocationQuery}'", "OK");
        }

        private void OnPreviousCommand(object sender, EventArgs e)
        {
            _viewModel.PreviousCommand.Execute(null);
            ScrollToCurrentIndex();
        }

        private void OnNextCommand(object sender, EventArgs e)
        {
            _viewModel.NextCommand.Execute(null);
            ScrollToCurrentIndex();
        }

        private void ScrollToCurrentIndex()
        {
            if (eventsCollection != null && _viewModel.CurrentEvent != null)
            {
                eventsCollection.ScrollTo(_viewModel.CurrentEvent, position: ScrollToPosition.Center, animate: true);
            }
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
