using System.Collections.ObjectModel;
using EventSpaceBookingSystem.Helpers;
using EventSpaceBookingSystem.ViewModel;
using Microsoft.Maui.Controls;
using Mopups.Services;

namespace EventSpaceBookingSystem.View
{
    [QueryProperty(nameof(Username), "username")]
    [QueryProperty(nameof(SearchQuery), "search")]
    [QueryProperty(nameof(LocationQuery), "location")]
    public partial class VenuePage : ContentPage
    {
        private readonly VenuePageViewModel _viewModel = new();
        private string _email = string.Empty;

        public string Username
        {
            get => _viewModel.Username;
            set => _viewModel.Username = value;
        }

        public string SearchQuery
        {
            set => _viewModel.SearchQuery = value;
        }

        public string LocationQuery
        {
            set => _viewModel.LocationQuery = value;
        }

        public VenuePage()
        {
            InitializeComponent();
            BindingContext = _viewModel;
            Loaded += async (_, _) => await _viewModel.InitAsync(_viewModel.Username);
        }

        public VenuePage(string username)
        {
            InitializeComponent();
            _viewModel.Username = username;
            BindingContext = _viewModel;
            Loaded += async (_, _) => await _viewModel.InitAsync(username);
        }

        public VenuePage(string username, string email)
        {
            InitializeComponent();
            _viewModel.Username = username;
            _email = email;
            BindingContext = _viewModel;
            Loaded += async (_, _) => await _viewModel.InitAsync(username);
        }

        public VenuePage(string username, string search, string location)
        {
            InitializeComponent();
            _viewModel = new VenuePageViewModel
            {
                Username = username,
                SearchQuery = search,
                LocationQuery = location
            };
            BindingContext = _viewModel;

            // Now it's safe to init because search/location are already set
            _viewModel.InitAsync(username);
        }

        private async void OnHomeClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new HomePage(_viewModel.Username, _email));
        }

        private async void OnExploreClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new VenuePage(_viewModel.Username, _email));
        }

        private async void OnCartClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CartPage(_viewModel.Username, _email));
        }

        private async void OnBecomeOwnerClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new EventSpaceOwnerJoinPage(_viewModel.Username, _email));
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
                var size = button.Bounds;

                if (location != null)
                {
                    var anchor = new Point(location.Value.X, location.Value.Y + size.Height);
                    var popup = new UserProfileMopUp(_viewModel.Username, anchor);
                    await MopupService.Instance.PushAsync(popup);
                }
            }
        }
    }
}
