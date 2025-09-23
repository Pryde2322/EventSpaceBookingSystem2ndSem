using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using EventSpaceBookingSystem.Model;
using EventSpaceBookingSystem.Services;

namespace EventSpaceBookingSystem.ViewModel
{
    public class SignUpPageViewModel : INotifyPropertyChanged
    {
        private string _email;
        private string _username;
        private string _status;
        private bool _isPasswordVisible = true;
        private bool _isConfirmPasswordVisible = true;
        private bool _isTermsAccepted;
        private string _password;
        private string _confirmPassword;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set { _isPasswordVisible = value; OnPropertyChanged(); }
        }

        public bool IsConfirmPasswordVisible
        {
            get => _isConfirmPasswordVisible;
            set { _isConfirmPasswordVisible = value; OnPropertyChanged(); }
        }

        public bool IsTermsAccepted
        {
            get => _isTermsAccepted;
            set { _isTermsAccepted = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set { _confirmPassword = value; OnPropertyChanged(); }
        }

        public void TogglePasswordVisibility()
        {
            IsPasswordVisible = !IsPasswordVisible;
        }

        public void ToggleConfirmPasswordVisibility()
        {
            IsConfirmPasswordVisible = !IsConfirmPasswordVisible;
        }

        public async Task SignUpAsync()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                await App.Current.MainPage.DisplayAlert("Error", "Please enter a valid email address.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(Username))
            {
                await App.Current.MainPage.DisplayAlert("Error", "Please enter a username.", "OK");
                return;
            }

            if (!IsTermsAccepted)
            {
                await App.Current.MainPage.DisplayAlert("Error", "Please accept the terms and conditions.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                await App.Current.MainPage.DisplayAlert("Error", "Password fields cannot be empty.", "OK");
                return;
            }

            if (Password != ConfirmPassword)
            {
                await App.Current.MainPage.DisplayAlert("Error", "Passwords do not match.", "OK");
                return;
            }

            var user = new Users
            {
                Email = this.Email,
                Username = this.Username,
                Password = this.Password,
                Status = "standard" // Default for newly registered users
            };

            try
            {
                await UserFileService.SaveUserAsync(user);
                await App.Current.MainPage.DisplayAlert("Success", "Account successfully created and saved!", "OK");

                // Clear fields after successful signup
                Email = string.Empty;
                Username = string.Empty;
                Password = string.Empty;
                ConfirmPassword = string.Empty;
                IsTermsAccepted = false;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", $"Failed to save user: {ex.Message}", "OK");
            }
        }


        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
