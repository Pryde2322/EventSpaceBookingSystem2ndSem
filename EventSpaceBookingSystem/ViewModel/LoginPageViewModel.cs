using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using EventSpaceBookingSystem.Model;
using EventSpaceBookingSystem.Services;
using Microsoft.Maui.Controls;
using System.Collections.Generic;
using System.Linq; 
namespace EventSpaceBookingSystem.ViewModel
{
    public class LoginPageViewModel : INotifyPropertyChanged
    {
        private string _email;
        private string _password;
        private bool _isPasswordVisible = true;
        private bool _isTermsAccepted;
        private string _tooltipMessage;
        private bool _isTooltipVisible;


        public event PropertyChangedEventHandler PropertyChanged;

        private Users _loggedInUser;
        public Users LoggedInUser
        {
            get => _loggedInUser;
            set
            {
                _loggedInUser = value;
                OnPropertyChanged();
            }
        }

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set { _isPasswordVisible = value; OnPropertyChanged(); }
        }

        public void TogglePasswordVisibility()
        {
            IsPasswordVisible = !IsPasswordVisible;
        }

        public bool IsTermsAccepted
        {
            get => _isTermsAccepted;
            set { _isTermsAccepted = value; OnPropertyChanged(); }
        }

        public string TooltipMessage
        {
            get => _tooltipMessage;
            set { _tooltipMessage = value; OnPropertyChanged(); }
        }

        public bool IsTooltipVisible
        {
            get => _isTooltipVisible;
            set { _isTooltipVisible = value; OnPropertyChanged(); }
        }

        public async Task<string> LoginAsync()
        {
            if (!IsTermsAccepted)
            {
                TooltipMessage = "You must accept the Terms & Conditions to proceed.";
                IsTooltipVisible = true;
                return "Invalid";
            }

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                TooltipMessage = "Email and Password fields cannot be empty.";
                IsTooltipVisible = true;
                return "Invalid";
            }

            var users = await UserFileService.LoadUsersAsync(UserFileService.GetUsersFilePath);

            // --- Admin login logic ---
            if (Email == "admin@admin.com" && Password == "admin123")
            {
                LoggedInUser = new Users
                {
                    Email = "admin@admin.com",
                    Username = "admin",
                    Password = "admin123",
                    Status = "admin",
                    Id = 0
                };
                return "Admin";
            }

            foreach (var user in users)
            {
                if (user.Email == Email && user.Password == Password && !user.Status.Equals("admin", StringComparison.OrdinalIgnoreCase))
                {
                    LoggedInUser = user;
                    return $"Standard:{user.Id}";
                }
            }

            // --- Event Space Owner login logic ---
            var owners = await UserFileService.LoadUsersAsync(UserFileService.GetEventSpaceOwnersFilePath);
            foreach (var owner in owners)
            {
                if (owner.Email == Email && owner.Password == Password &&
                    (owner.Email.Split('@')[0].Contains(".eventspaceowner") || owner.Status.Equals("event space owner", StringComparison.OrdinalIgnoreCase)))
                {
                    // Use the new .NET MAUI way to get the main page
                    var mainPage = Application.Current?.Windows[0]?.Page;

                    if (string.IsNullOrEmpty(owner.Status))
                    {
                        if (mainPage != null)
                            await mainPage.DisplayAlert("null", "Wait for the Admin to activate your account.", "OK");
                        return "Invalid";
                    }

                    if (owner.Status.Equals("Deactivated", StringComparison.OrdinalIgnoreCase))
                    {
                        if (mainPage != null)
                            await mainPage.DisplayAlert("Account Deactivated", "Your account is deactivated. Please contact Customer Service at RaMenCo.CustomerService@gmail.com", "OK");
                        return "Invalid";
                    }

                    if (owner.Status.Equals("Activated", StringComparison.OrdinalIgnoreCase) || owner.Status.Equals("event space owner", StringComparison.OrdinalIgnoreCase))
                    {
                        LoggedInUser = owner;
                        return $"Owner:{owner.Id}";
                    }
                }
            }

            return "Invalid";
        }

        public async Task<bool> ChangePasswordAsync(string email, string oldPassword, string newPassword)
        {
            
            try
            {
                // --- 1. Check Standard Users ---
                var users = await UserFileService.LoadUsersAsync(UserFileService.GetUsersFilePath);
                var userToUpdate = users.FirstOrDefault(u => u.Email == email && u.Password == oldPassword);

                if (userToUpdate != null)
                {
                    // Found the user. Update their password.
                    userToUpdate.Password = newPassword;

                    // Save the entire list of users back to the file
                    await UserFileService.SaveUsersAsync(users, UserFileService.GetUsersFilePath);
                    return true; // Success!
                }

                // --- 2. Check Event Space Owners ---
                var owners = await UserFileService.LoadUsersAsync(UserFileService.GetEventSpaceOwnersFilePath);
                var ownerToUpdate = owners.FirstOrDefault(o => o.Email == email && o.Password == oldPassword);

                if (ownerToUpdate != null)
                {
                    // Found the owner. Update their password.
                    ownerToUpdate.Password = newPassword;

                    // Save the entire list of owners back to the file
                    await UserFileService.SaveUsersAsync(owners, UserFileService.GetEventSpaceOwnersFilePath);
                    return true; // Success!
                }

                // --- 3. No user found ---
                // User was not found or the old password was incorrect
                return false;
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error in ChangePasswordAsync: {ex.Message}");
                return false;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}