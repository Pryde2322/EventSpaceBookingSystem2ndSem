using EventSpaceBookingSystem.ViewModel;
using Microsoft.Maui.Controls;
using System; 

namespace EventSpaceBookingSystem.View
{
    public partial class NewPasswordPage : ContentPage
    {
        // Add a field to hold the ViewModel
        private readonly LoginPageViewModel _loginViewModel;

        // Modify the constructor to ACCEPT the ViewModel
        public NewPasswordPage(LoginPageViewModel viewModel)
        {
            InitializeComponent();
            _loginViewModel = viewModel; // Store the passed-in ViewModel
        }

        // ***** REPLACE your old OnSaveClicked with this *****
        private async void OnSaveClicked(object sender, EventArgs e)
        {
            string email = EmailEntry.Text;
            string oldPassword = OldPasswordEntry.Text;
            string newPassword = NewPasswordEntry.Text;
            string confirmPassword = ConfirmPasswordEntry.Text;

            // 1. Basic Validation
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                await DisplayAlert("Error", "Please fill in all fields.", "OK");
                return;
            }

            if (newPassword != confirmPassword)
            {
                await DisplayAlert("Error", "New passwords do not match.", "OK");
                return;
            }

            // 2. Call the logic on your ViewModel
            try
            {
                // YOU MUST CREATE THIS METHOD in your LoginPageViewModel.cs file
                bool success = await _loginViewModel.ChangePasswordAsync(email, oldPassword, newPassword);

                if (success)
                {
                    // This is the only part we kept
                    await DisplayAlert("Success", "Successfully changed", "OK");
                    await Navigation.PopAsync(); // Go back to Login
                }
                else
                {
                    await DisplayAlert("Error", "Could not change password. Please check your email and old password.", "OK");
                }
            }
            catch (Exception ex)
            {
                // Show any other errors
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }
    }
}