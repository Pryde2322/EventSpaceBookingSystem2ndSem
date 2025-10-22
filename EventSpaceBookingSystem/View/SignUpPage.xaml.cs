using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EventSpaceBookingSystem.Model;
using EventSpaceBookingSystem.ViewModel;

namespace EventSpaceBookingSystem.View;

public partial class SignUpPage : ContentPage
{
    private readonly SignUpPageViewModel _viewModel;

    public SignUpPage()
    {
        InitializeComponent();
        _viewModel = new SignUpPageViewModel();
        BindingContext = _viewModel;
    }

    private void OnShowPasswordClicked(object sender, EventArgs e)
    {
        _viewModel.TogglePasswordVisibility();
    }

    private void OnShowConfirmPasswordClicked(object sender, EventArgs e)
    {
        _viewModel.ToggleConfirmPasswordVisibility();
    }

    private async void OnSignUpClicked(object sender, EventArgs e)
    {
        _viewModel.Email = txtEmail.Text;
        _viewModel.Username = txtUsername.Text;
        _viewModel.Password = txtPassword.Text;
        _viewModel.ConfirmPassword = txtConfirmPassword.Text;

        await _viewModel.SignUpAsync();

        rst();

    }
    private async void OnLoginTapped(object sender, EventArgs e)
    {
        // Navigate to the LoginPage
        await Navigation.PushAsync(new LoginPage());
    }

    void rst()
    {
        txtEmail.Text = string.Empty;
        txtUsername.Text = string.Empty;
        txtPassword.Text = string.Empty;
        txtConfirmPassword.Text = string.Empty;
        _viewModel.IsTermsAccepted = false;
    }

}
