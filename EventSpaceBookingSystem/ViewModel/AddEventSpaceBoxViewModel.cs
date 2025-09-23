using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using EventSpaceBookingSystem.Model;
using EventSpaceBookingSystem.Services;

namespace EventSpaceBookingSystem.ViewModel
{
    public class AddEventSpaceBoxViewModel : INotifyPropertyChanged
    {
        private readonly OwnerHomePageViewModel _ownerVM;
        private readonly int _ownerId;
        private readonly EventModel _event = new();

        public AddEventSpaceBoxViewModel(OwnerHomePageViewModel ownerVM, int ownerId)
        {
            _ownerVM = ownerVM;
            _ownerId = ownerId;

            UploadImageCommand = new Command(OnUploadImage);
            RemoveImageCommand = new Command<ImageSource>(OnRemoveImage);
            SubmitCommand = new Command(OnSubmitAsync);
            ToggleCategoryDropdownCommand = new Command(() => IsCategoryDropdownOpen = !IsCategoryDropdownOpen);

            AvailableCategories = new ObservableCollection<SelectableCategory>
            {
                new() { Name = "Mountain" },
                new() { Name = "Beach" },
                new() { Name = "Hotel" },
                new() { Name = "Resort" },
                new() { Name = "Garden" },
                new() { Name = "Conference Hall" },
                new() { Name = "Banquet Hall" },
                new() { Name = "Rooftop" },
                new() { Name = "Lounge" },
                new() { Name = "Private Villa" }
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

        // Event field bindings
        public string Title { get => _event.Title; set { _event.Title = value; OnPropertyChanged(); } }
        public string Description1 { get => _event.Description1; set { _event.Description1 = value; OnPropertyChanged(); } }
        public string Location { get => _event.Location; set { _event.Location = value; OnPropertyChanged(); } }
        public string Capacity { get => _event.Capacity; set { _event.Capacity = value; OnPropertyChanged(); } }
        public string DailyRate { get => _event.DailyRate; set { _event.DailyRate = value; OnPropertyChanged(); } }
        public string ExtensionRate { get => _event.ExtensionRate; set { _event.ExtensionRate = value; OnPropertyChanged(); } }
        public string TableRate { get => _event.TableRate; set { _event.TableRate = value; OnPropertyChanged(); } }

        // Category selection
        public ObservableCollection<SelectableCategory> AvailableCategories { get; }
        public ObservableCollection<string> SelectedCategories { get; } = new();

        private bool _isCategoryDropdownOpen;
        public bool IsCategoryDropdownOpen
        {
            get => _isCategoryDropdownOpen;
            set { _isCategoryDropdownOpen = value; OnPropertyChanged(); }
        }

        // Image upload support
        public ObservableCollection<ImageSource> UploadedImages { get; } = new();

        private bool _isUploading;
        public bool IsUploading
        {
            get => _isUploading;
            set { _isUploading = value; OnPropertyChanged(); }
        }

        public ICommand UploadImageCommand { get; }
        public ICommand RemoveImageCommand { get; }
        public ICommand SubmitCommand { get; }
        public ICommand ToggleCategoryDropdownCommand { get; }

        private async void OnUploadImage()
        {
            try
            {
                IsUploading = true;
                var results = await FilePicker.PickMultipleAsync(new PickOptions { PickerTitle = "Select up to 5 images" });
                if (results == null || !results.Any()) return;

                foreach (var file in results)
                {
                    if (UploadedImages.Count >= 5)
                    {
                        await ShowError("Maximum of 5 images allowed.");
                        break;
                    }

                    using var stream = await file.OpenReadAsync();
                    if (stream.Length == 0) continue;

                    using var ms = new MemoryStream();
                    await stream.CopyToAsync(ms);
                    UploadedImages.Add(ImageSource.FromStream(() => new MemoryStream(ms.ToArray())));
                }
            }
            finally { IsUploading = false; }
        }

        private void OnRemoveImage(ImageSource img)
        {
            if (UploadedImages.Contains(img))
                UploadedImages.Remove(img);
        }

        private async void OnSubmitAsync()
        {
            // Validate basic input
            if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Description1) || string.IsNullOrWhiteSpace(Location)
                || string.IsNullOrWhiteSpace(Capacity) || string.IsNullOrWhiteSpace(DailyRate)
                || string.IsNullOrWhiteSpace(ExtensionRate) || string.IsNullOrWhiteSpace(TableRate))
            {
                await ShowError("Please fill in all fields.");
                return;
            }

            // Validate selections
            if (SelectedCategories.Count == 0)
            {
                await ShowError("Select at least one category.");
                return;
            }

            if (UploadedImages.Count == 0)
            {
                await ShowError("Upload at least one image.");
                return;
            }

            try
            {
                // Save images to disk
                var imagePaths = await SpaceOwnerFileService.SaveOwnerImagesAsync(_ownerId, UploadedImages);

                // Finalize and store event model
                _event.ImageUrls = imagePaths;
                _event.FormattedPrice = $"₱{DailyRate} / day";
                _event.Category = string.Join(", ", SelectedCategories);
                _event.Rating = 0;
                _event.ReviewCount = 0;

                _ownerVM.EventSpaces.Add(_event);
                await SpaceOwnerFileService.SaveToFileAsync(_ownerVM.EventSpaces, _ownerId);

                await Mopups.Services.MopupService.Instance.PopAsync();
            }
            catch (Exception ex)
            {
                await ShowError($"Failed to save event space: {ex.Message}");
            }
        }


        private Task ShowError(string msg) =>
            Application.Current.MainPage.DisplayAlert("Error", msg, "OK");

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
