using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using EventSpaceBookingSystem.Model;
using EventSpaceBookingSystem.Services;
using EventSpaceBookingSystem.View;

namespace EventSpaceBookingSystem.ViewModel
{
    public class EventSpaceOwnerJoinViewModel : INotifyPropertyChanged
    {
        private readonly Users User = new Users();
        private readonly EventModel EventSpace = new EventModel();

        public ICommand JoinCommand { get; }
        public ICommand UploadImageCommand { get; }
        public ICommand RemoveImageCommand { get; }
        public ICommand ToggleCategoryDropdownCommand { get; }

        public string MyEmail { get; set; } = string.Empty;
        public EventSpaceOwnerJoinViewModel(string loggedInEmail)
        {
            MyEmail = loggedInEmail;
            if (!string.IsNullOrWhiteSpace(loggedInEmail))
            {
                if (!loggedInEmail.Contains(".eventspaceowner@"))
                {
                    var parts = loggedInEmail.Split('@');
                    FormattedEmail = $"{parts[0]}.eventspaceowner@{parts[1]}";
                }
                else
                {
                    FormattedEmail = loggedInEmail;
                }
            }

            // 🔍 Check if already an event space owner
            _ = CheckIfAlreadyOwnerAsync(FormattedEmail);

            JoinCommand = new Command(OnJoin);
            UploadImageCommand = new Command(OnUploadImage);
            RemoveImageCommand = new Command<ImageSource>(OnRemoveImage);
            ToggleCategoryDropdownCommand = new Command(() => IsCategoryDropdownOpen = !IsCategoryDropdownOpen);

            AvailableCategories = new ObservableCollection<SelectableCategory>
{
    new SelectableCategory { Name = "Mountain" },
    new SelectableCategory { Name = "Beach" },
    new SelectableCategory { Name = "Hotel" },
    new SelectableCategory { Name = "Resort" },
    new SelectableCategory { Name = "Garden" },
    new SelectableCategory { Name = "Conference Hall" },
    new SelectableCategory { Name = "Banquet Hall" },
    new SelectableCategory { Name = "Rooftop" },
    new SelectableCategory { Name = "Lounge" },
    new SelectableCategory { Name = "Private Villa" }
};


            foreach (var cat in AvailableCategories)
            {
                cat.PropertyChanged += (_, _) =>
                {
                    var selected = AvailableCategories.Where(c => c.IsSelected).Select(c => c.Name).ToList();
                    if (selected.Count > 3)
                    {
                        cat.IsSelected = false;
                        Application.Current.MainPage.DisplayAlert("Limit", "You can select up to 3 categories only.", "OK");
                    }
                    else
                    {
                        SelectedCategories.Clear();
                        foreach (var name in selected)
                            SelectedCategories.Add(name);
                    }
                };
            }
        }

        private async Task CheckIfAlreadyOwnerAsync(string email)
        {
            var existingOwners = await UserFileService.LoadUsersAsync(UserFileService.GetEventSpaceOwnersFilePath);
            if (existingOwners.Any(u => u.Email == email))
            {
                await App.Current.MainPage.DisplayAlert("Notice", "You're already registered as an Event Space Owner.", "OK");

                // ✅ Redirect to HomePage
                await App.Current.MainPage.Navigation.PushAsync(new HomePage(Username, email));
            }
        }


        // Form fields
        private string _formattedEmail;
        public string FormattedEmail { get => _formattedEmail; set { _formattedEmail = value; OnPropertyChanged(); } }

        private string _username;
        public string Username { get => _username; set { _username = value; OnPropertyChanged(); } }
        private string _updateUsername;
        public string UpdateUsername { get => _updateUsername; set { _updateUsername = value; OnPropertyChanged(); } }

        private string _password;
        public string Password { get => _password; set { _password = value; OnPropertyChanged(); } }

        private string _confirmPassword;
        public string ConfirmPassword { get => _confirmPassword; set { _confirmPassword = value; OnPropertyChanged(); } }

        private bool _isTermsAccepted;
        public bool IsTermsAccepted { get => _isTermsAccepted; set { _isTermsAccepted = value; OnPropertyChanged(); } }

        private bool _isPasswordVisible = false;
        public bool IsPasswordVisible { get => _isPasswordVisible; set { _isPasswordVisible = value; OnPropertyChanged(); } }

        private bool _isConfirmPasswordVisible = false;
        public bool IsConfirmPasswordVisible { get => _isConfirmPasswordVisible; set { _isConfirmPasswordVisible = value; OnPropertyChanged(); } }

        public void TogglePasswordVisibility() => IsPasswordVisible = !IsPasswordVisible;
        public void ToggleConfirmPasswordVisibility() => IsConfirmPasswordVisible = !IsConfirmPasswordVisible;

        // Event space fields
        public string Title { get => EventSpace.Title; set { EventSpace.Title = value; OnPropertyChanged(); } }
        public string Capacity { get => EventSpace.Capacity; set { EventSpace.Capacity = value; OnPropertyChanged(); } }
        public string DailyRate { get => EventSpace.DailyRate; set { EventSpace.DailyRate = value; OnPropertyChanged(); } }
        public string ExtensionRate { get => EventSpace.ExtensionRate; set { EventSpace.ExtensionRate = value; OnPropertyChanged(); } }
        public string TableRate { get => EventSpace.TableRate; set { EventSpace.TableRate = value; OnPropertyChanged(); } }
        public string Location { get => EventSpace.Location; set { EventSpace.Location = value; OnPropertyChanged(); } }
        public string Description1 { get => EventSpace.Description1; set { EventSpace.Description1 = value; OnPropertyChanged(); } }
        public string Description2 { get => EventSpace.Description2; set { EventSpace.Description2 = value; OnPropertyChanged(); } }

        // Dropdown + image management
        private bool _isCategoryDropdownOpen;
        public bool IsCategoryDropdownOpen { get => _isCategoryDropdownOpen; set { _isCategoryDropdownOpen = value; OnPropertyChanged(); } }

        public ObservableCollection<SelectableCategory> AvailableCategories { get; set; }
        public ObservableCollection<string> SelectedCategories { get; set; } = new();
        public ObservableCollection<ImageSource> UploadedImages { get; set; } = new();
        private const int MaxImages = 5;

        private bool _isUploading;
        public bool IsUploading { get => _isUploading; set { _isUploading = value; OnPropertyChanged(); } }

        private async void OnUploadImage()
        {
            try
            {
                IsUploading = true;

                var results = await FilePicker.PickMultipleAsync(new PickOptions { PickerTitle = "Select up to 5 images" });
                if (results == null || !results.Any()) return;

                foreach (var file in results)
                {
                    try
                    {
                        if (UploadedImages.Count >= MaxImages)
                        {
                            await ShowError($"You can only upload {MaxImages} images.");
                            break;
                        }

                        var ext = Path.GetExtension(file.FileName).ToLower();
                        if (ext != ".jpg" && ext != ".jpeg" && ext != ".png")
                        {
                            await ShowError($"Unsupported file type: {ext}. Only JPG and PNG allowed.");
                            continue;
                        }

                        // Load and verify image stream
                        using var stream = await file.OpenReadAsync();
                        if (stream == null || stream.Length == 0)
                        {
                            await ShowError($"File {file.FileName} is empty or corrupted.");
                            continue;
                        }

                        using var ms = new MemoryStream();
                        await stream.CopyToAsync(ms);
                        if (ms.Length == 0)
                        {
                            await ShowError($"Failed to read file {file.FileName}.");
                            continue;
                        }

                        // Add image to the collection (safe clone of stream)
                        UploadedImages.Add(ImageSource.FromStream(() => new MemoryStream(ms.ToArray())));
                    }
                    catch (IOException ioEx)
                    {
                        await ShowError($"File access error for {file.FileName}: {ioEx.Message}");
                    }
                    catch (Exception innerEx)
                    {
                        await ShowError($"Unexpected error with {file.FileName}: {innerEx.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                await ShowError($"Image upload failed: {ex.Message}");
            }
            finally
            {
                IsUploading = false;
            }
        }


        private void OnRemoveImage(ImageSource image)
        {
            if (UploadedImages.Contains(image))
                UploadedImages.Remove(image);
        }

        private async void OnJoin()
        {
            if (string.IsNullOrWhiteSpace(UpdateUsername)) { await ShowError("Please enter a username."); return; }
            if (string.IsNullOrWhiteSpace(Password)) { await ShowError("Please enter a password."); return; }
            if (Password != ConfirmPassword) { await ShowError("Passwords do not match."); return; }
            if (!IsTermsAccepted) { await ShowError("Please accept the Terms & Conditions."); return; }

            if (string.IsNullOrWhiteSpace(Title)) { await ShowError("Enter event space title."); return; }
            if (string.IsNullOrWhiteSpace(Capacity)) { await ShowError("Enter capacity."); return; }
            if (string.IsNullOrWhiteSpace(DailyRate)) { await ShowError("Enter daily rate."); return; }
            if (string.IsNullOrWhiteSpace(ExtensionRate)) { await ShowError("Enter extension rate."); return; }
            if (string.IsNullOrWhiteSpace(TableRate)) { await ShowError("Enter table rate."); return; }
            if (string.IsNullOrWhiteSpace(Location)) { await ShowError("Enter location."); return; }
            if (string.IsNullOrWhiteSpace(Description1)) { await ShowError("Enter event space description."); return; }
            if (SelectedCategories.Count == 0) { await ShowError("Select at least one category."); return; }
            if (UploadedImages.Count == 0) { await ShowError("Upload at least one image."); return; }

            User.Username = UpdateUsername;
            User.Password = Password;
            User.ConfirmPassword = ConfirmPassword;
            User.Email = FormattedEmail;
            User.Status = "event space owner";
            User.IsTermsAccepted = IsTermsAccepted;

            await UserFileService.UpdateUserStatusAsync(MyEmail, "event space owner");


            int userId = await UserFileService.SaveUserAndReturnIdAsync(User);
            if (userId == -1) { await ShowError("User could not be saved."); return; }

            var imagePaths = await UserFileService.SaveOwnerImagesAsync(userId, UploadedImages);
            EventSpace.ImageUrls = imagePaths;
            EventSpace.FormattedPrice = $"₱{DailyRate} / day";
            EventSpace.Category = string.Join(", ", SelectedCategories);
            EventSpace.Rating = 0;
            EventSpace.ReviewCount = 0;

            await UserFileService.SaveEventAsync(EventSpace, userId.ToString());
            await Application.Current.MainPage.DisplayAlert("Success", "Registration complete!", "OK");
            await Application.Current.MainPage.Navigation.PushAsync(new HomePage(Username, MyEmail));
        }

        private Task ShowError(string message) =>
            Application.Current.MainPage.DisplayAlert("Error", message, "OK");

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
