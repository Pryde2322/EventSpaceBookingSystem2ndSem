using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using EventSpaceBookingSystem.Model;
using EventSpaceBookingSystem.Services;

namespace EventSpaceBookingSystem.ViewModel
{

    public class SelectableCategory : INotifyPropertyChanged
    {
        public string Name { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class OwnerUpdateSpaceViewModel : INotifyPropertyChanged
    {
        private readonly EventModel _originalEvent;
        private readonly int _ownerId;

        public OwnerUpdateSpaceViewModel(EventModel spaceToEdit, int ownerId)
        {
            _originalEvent = spaceToEdit;
            _ownerId = ownerId;

            Title = spaceToEdit.Title;
            Description1 = spaceToEdit.Description1;
            Location = spaceToEdit.Location;
            Capacity = spaceToEdit.Capacity;
            DailyRate = spaceToEdit.DailyRate;
            ExtensionRate = spaceToEdit.ExtensionRate;
            TableRate = spaceToEdit.TableRate;

            UploadImageCommand = new Command(OnUploadImage);
            RemoveImageCommand = new Command<EditableImage>(OnRemoveImage);
            ToggleCategoryDropdownCommand = new Command(() => IsCategoryDropdownOpen = !IsCategoryDropdownOpen);

            InitializeCategories();
            LoadExistingImages();
        }

        // ------------------ Text Fields ------------------
        private string _title;
        public string Title { get => _title; set { _title = value; OnPropertyChanged(); } }

        private string _description1;
        public string Description1 { get => _description1; set { _description1 = value; OnPropertyChanged(); } }

        private string _location;
        public string Location { get => _location; set { _location = value; OnPropertyChanged(); } }

        private string _capacity;
        public string Capacity { get => _capacity; set { _capacity = value; OnPropertyChanged(); } }

        private string _dailyRate;
        public string DailyRate { get => _dailyRate; set { _dailyRate = value; OnPropertyChanged(); } }

        private string _extensionRate;
        public string ExtensionRate { get => _extensionRate; set { _extensionRate = value; OnPropertyChanged(); } }

        private string _tableRate;
        public string TableRate { get => _tableRate; set { _tableRate = value; OnPropertyChanged(); } }

        // ------------------ Categories ------------------
        public ObservableCollection<SelectableCategory> AvailableCategories { get; set; } = new();
        public ObservableCollection<string> SelectedCategories { get; set; } = new();

        private bool _isCategoryDropdownOpen;
        public bool IsCategoryDropdownOpen
        {
            get => _isCategoryDropdownOpen;
            set { _isCategoryDropdownOpen = value; OnPropertyChanged(); }
        }

        public ICommand ToggleCategoryDropdownCommand { get; }

        private void InitializeCategories()
        {
            var savedCategories = (_originalEvent.Category ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var options = new[]
            {
                "Mountain", "Beach", "Hotel", "Resort", "Garden",
                "Conference Hall", "Banquet Hall", "Rooftop", "Lounge", "Private Villa"
            };

            foreach (var cat in options)
            {
                var item = new SelectableCategory
                {
                    Name = cat,
                    IsSelected = savedCategories.Contains(cat)
                };
                item.PropertyChanged += (_, _) => UpdateSelectedCategories();
                AvailableCategories.Add(item);
            }

            UpdateSelectedCategories();
        }

        public string SelectedCategoryDisplayText =>
            SelectedCategories.Any() ? string.Join(", ", SelectedCategories) : "Select up to 3 categories...";

        private void UpdateSelectedCategories()
        {
            var selected = AvailableCategories.Where(c => c.IsSelected).Select(c => c.Name).ToList();

            if (selected.Count > 3)
            {
                var last = AvailableCategories.Last(c => c.IsSelected);
                last.IsSelected = false;
                Application.Current.MainPage.DisplayAlert("Limit", "You can select up to 3 categories only.", "OK");
                return;
            }

            SelectedCategories.Clear();
            foreach (var name in selected)
                SelectedCategories.Add(name);

            OnPropertyChanged(nameof(SelectedCategories));
            OnPropertyChanged(nameof(SelectedCategoryDisplayText));
        }

        // ------------------ Images ------------------
        public ObservableCollection<EditableImage> DisplayImages { get; set; } = new();
        public ICommand UploadImageCommand { get; }
        public ICommand RemoveImageCommand { get; }

        private bool _isUploading;
        public bool IsUploading { get => _isUploading; set { _isUploading = value; OnPropertyChanged(); } }

        public ImageSource PrimaryImage
        {
            get
            {
                // Use image from display list if exists
                var displayImage = DisplayImages.FirstOrDefault()?.Source;
                if (displayImage != null)
                    return displayImage;

                // Use fallback from saved ImageUrl if exists
                if (!string.IsNullOrWhiteSpace(_originalEvent.ImageUrl))
                {
                    var fallbackPath = Path.Combine(AppContext.BaseDirectory, "Json", _originalEvent.ImageUrl);

                    if (File.Exists(fallbackPath))
                        return ImageSource.FromFile(fallbackPath);
                }

                // Final fallback
                return "placeholder_image.png";
            }
        }


        private void LoadExistingImages()
        {
            if (_originalEvent.ImageUrls == null) return;

            var loadedImages = SpaceOwnerFileService.LoadOwnerImages(_originalEvent.ImageUrls);
            foreach (var image in loadedImages)
                DisplayImages.Add(image);

            OnPropertyChanged(nameof(PrimaryImage));
        }




        private async void OnUploadImage()
        {
            try
            {
                IsUploading = true;

                var results = await FilePicker.PickMultipleAsync(new PickOptions { PickerTitle = "Select up to 20 images" });
                if (results == null || !results.Any()) return;

                foreach (var file in results)
                {
                    try
                    {
                        if (DisplayImages.Count >= 20)
                        {
                            await ShowError("You can only upload up to 20 images.");
                            break;
                        }

                        var ext = Path.GetExtension(file.FileName).ToLower();
                        if (ext != ".jpg" && ext != ".jpeg" && ext != ".png")
                        {
                            await ShowError($"Unsupported file type: {ext}. Only JPG and PNG allowed.");
                            continue;
                        }

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

                        DisplayImages.Add(new EditableImage
                        {
                            IsNew = true,
                            Path = "",
                            Source = ImageSource.FromStream(() => new MemoryStream(ms.ToArray()))
                        });
                    }
                    catch (Exception ex)
                    {
                        await ShowError($"Error with file {file.FileName}: {ex.Message}");
                    }
                }

                OnPropertyChanged(nameof(PrimaryImage));
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


        private void OnRemoveImage(EditableImage image)
        {
            if (DisplayImages.Contains(image))
            {
                DisplayImages.Remove(image);
                OnPropertyChanged(nameof(PrimaryImage));
            }
        }


        // ------------------ Save ------------------
        public async Task SaveChangesAsync()
        {
            if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Description1) ||
                string.IsNullOrWhiteSpace(Location) || string.IsNullOrWhiteSpace(Capacity) ||
                string.IsNullOrWhiteSpace(DailyRate) || string.IsNullOrWhiteSpace(ExtensionRate) ||
                string.IsNullOrWhiteSpace(TableRate))
            {
                await ShowError("Please fill in all required fields.");
                return;
            }

            if (SelectedCategories.Count == 0)
            {
                await ShowError("Please select at least one category.");
                return;
            }

            var newImages = new ObservableCollection<ImageSource>(
                DisplayImages.Where(i => i.IsNew).Select(i => i.Source));

            var savedPaths = await SpaceOwnerFileService.SaveOwnerImagesAsync(_ownerId, newImages);

            int newIdx = 0;
            foreach (var img in DisplayImages)
            {
                if (img.IsNew)
                {
                    img.Path = savedPaths.ElementAtOrDefault(newIdx++);
                    img.IsNew = false;
                }
            }

            _originalEvent.Title = Title;
            _originalEvent.Description1 = Description1;
            _originalEvent.Location = Location;
            _originalEvent.Capacity = Capacity;
            _originalEvent.DailyRate = DailyRate;
            _originalEvent.ExtensionRate = ExtensionRate;
            _originalEvent.TableRate = TableRate;
            _originalEvent.FormattedPrice = $"₱{DailyRate} / day";
            _originalEvent.Category = string.Join(", ", SelectedCategories);
            _originalEvent.ImageUrls = DisplayImages.Select(i => i.Path).Where(p => !string.IsNullOrWhiteSpace(p)).ToList();

            await SpaceOwnerFileService.SaveUpdatedSpaceAsync(_originalEvent, _ownerId);
        }

        private Task ShowError(string msg) =>
            Application.Current.MainPage.DisplayAlert("Error", msg, "OK");

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
