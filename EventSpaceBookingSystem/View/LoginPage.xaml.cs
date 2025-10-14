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
            try
            {
                string result = await _viewModel.LoginAsync();

                if (string.IsNullOrEmpty(result))
                {
                    await DisplayAlert("Error", "Login failed. Please try again.", "OK");
                    return;
                }

                if (result.StartsWith("Standard"))
                {
                    await Navigation.PushAsync(new HomePage(_viewModel.LoggedInUser?.Username ?? "Guest", _viewModel.LoggedInUser?.Email ?? "N/A"));
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
                        if (result.Contains(":") && int.TryParse(result.Split(':')[1], out int ownerId))
                        {
                            Session.CurrentOwnerId = ownerId;
                            await Navigation.PushAsync(new OwnerHomePage(ownerId));
                        }
                        else
                        {
                            await DisplayAlert("Error", "Invalid owner data.", "OK");
                        }
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
            catch (Exception ex)
            {
                await DisplayAlert("Login Error", ex.Message, "OK");
            }
        }


        private void OnShowPasswordClicked(object sender, EventArgs e)
        {
            _viewModel.TogglePasswordVisibility();
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

    }
}
