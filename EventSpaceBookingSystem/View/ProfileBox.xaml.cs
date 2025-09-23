using Mopups.Pages;
using EventSpaceBookingSystem.ViewModel;
using Microsoft.Maui.Controls;
using Mopups.Services;

namespace EventSpaceBookingSystem.View
{
    public partial class ProfileBox : PopupPage
    {
        private readonly ProfileBoxViewModel viewModel;

        public ProfileBox(int ownerId)
        {
            InitializeComponent();
            viewModel = new ProfileBoxViewModel();
            BindingContext = viewModel;
            viewModel.LoadUserProfileByIdAsync(ownerId);
        }
        public ProfileBox(string ownerId)
        {
            InitializeComponent();
            viewModel = new ProfileBoxViewModel();
            BindingContext = viewModel;
            viewModel.LoadUserProfileByIdAsync(ownerId);
        }

        private async void Logout_Clicked(object sender, EventArgs e)
        {
            await MopupService.Instance.PopAsync();
            Preferences.Remove("LoggedInUsername");
            await Application.Current.MainPage.DisplayAlert("Logout", "You have been logged out.", "OK");
            Application.Current.MainPage = new NavigationPage(new View.LoginPage());
        }
    }
}
