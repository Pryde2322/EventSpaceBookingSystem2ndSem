using Microsoft.Maui.Controls;
using EventSpaceBookingSystem.ViewModel;
using EventSpaceBookingSystem.Helpers;

namespace EventSpaceBookingSystem.View
{
    public partial class LoginPage : ContentPage
    {
        private readonly LoginPageViewModel _viewModel;

        public LoginPage()
        {
            InitializeComponent();
            _viewModel = new LoginPageViewModel();
            BindingContext = _viewModel;
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            string result = await _viewModel.LoginAsync();

            if (result.StartsWith("Standard"))
            {
                await Navigation.PushAsync(new HomePage(_viewModel.LoggedInUser.Username, _viewModel.LoggedInUser.Email));
            }
            else if (result.StartsWith("Owner"))
            {
                // Check RaMenCoStatus for owner
                var status = _viewModel.LoggedInUser?.RaMenCoStatus ?? string.Empty;

                if (string.IsNullOrWhiteSpace(status))
                {
                    await DisplayAlert("Pending Approval", "Wait for the Admin to activate your account.", "OK");
                    return;
                }

                if (status.Equals("Deactivated", StringComparison.OrdinalIgnoreCase))
                {
                    await DisplayAlert("Account Deactivated", "Your account is deactivated. Please contact Customer Service at RaMenCo.CustomerService@gmail.com", "OK");
                    return;
                }

                if (status.Equals("Activated", StringComparison.OrdinalIgnoreCase))
                {
                    int ownerId = int.Parse(result.Split(':')[1]);
                    Session.CurrentOwnerId = ownerId;
                    await Navigation.PushAsync(new OwnerHomePage(ownerId));
                }
            }
            else if (result == "Admin")
            {
                await Navigation.PushAsync(new AdminPage());
            }
            else
            {
                await DisplayAlert("Error", "Invalid Email or Password.", "OK");
            }
        }




        private async void OnGoogleSignUpClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Not Available", "This feature is not currently available", "OK");
        }

        private async void OnFacebookSignUpClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Not Available", "This feature is not currently available", "OK");
        }

        private async void OnSignUpTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SignUpPage());
        }

        private void OnShowPasswordClicked(object sender, EventArgs e)
        {
            _viewModel.TogglePasswordVisibility();
        }
    }
}
