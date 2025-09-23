using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using EventSpaceBookingSystem.Model;
using Microsoft.Maui.Controls;
using EventSpaceBookingSystem.Services;

namespace EventSpaceBookingSystem.ViewModel
{
    public class ExplorePageViewModel : INotifyPropertyChanged
    {

        public bool IsLoggedIn => !string.IsNullOrWhiteSpace(Username);


        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsLoggedIn)); }
        }
        private string _username;

        private string _email;
        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        public ExplorePageViewModel(EventModel selectedEvent)
        {
            LoadSelectedVenue(selectedEvent);
            InitBookingOptions();
        }

        public ExplorePageViewModel(string username, string email, EventModel selectedEvent)
        {
            Username = username;
            Email = email;
            LoadSelectedVenue(selectedEvent);
            InitBookingOptions();
        }

        private EventModel _venue;
        public EventModel Venue
        {
            get => _venue;
            set
            {
                _venue = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(NameSpace));
                OnPropertyChanged(nameof(Category));
                OnPropertyChanged(nameof(Description1));
                OnPropertyChanged(nameof(Description2));
                OnPropertyChanged(nameof(Rating));
                OnPropertyChanged(nameof(StarDisplay));
                OnPropertyChanged(nameof(Price));
                OnPropertyChanged(nameof(Capacity));
                OnPropertyChanged(nameof(VenueImage));
                OnPropertyChanged(nameof(FirstRelatedImage));

            }
        }

        public string NameSpace => Venue?.Title;
        public string Category => Venue?.Category;
        public string Description1 => Venue?.Description1;
        public string Description2 => Venue?.Description2;
        public double Rating => Venue?.Rating ?? 0;
        public string Price => Venue?.FormattedPrice;
        public string Capacity => Venue?.Capacity;
        public ImageSource VenueImage => Venue?.UploadedImage ?? "event_placeholder.png";
        public string FirstRelatedImage => RelatedImages?.FirstOrDefault() ?? "event_placeholder.png";




        public ObservableCollection<string> RelatedImages { get; set; } = new();

        public string StarDisplay
        {
            get
            {
                int full = (int)Math.Floor(Rating);
                bool half = Rating - full >= 0.5;
                int empty = 5 - full - (half ? 1 : 0);
                return new string('★', full) + (half ? "½" : "") + new string('☆', empty);
            }
        }

        public async Task LoadVenueDetailsAsync()
        {
            await Task.Delay(200);
        }

        public void ToggleBookmark() { }

        public void LoadSelectedVenue(EventModel selected)
        {
            if (selected == null)
                return;

            Venue = selected;
            RelatedImages = new ObservableCollection<string>();
            bool imageSet = false;

            if (selected.ImageUrls != null && selected.ImageUrls.Count > 0)
            {
                foreach (var relativePath in selected.ImageUrls)
                {
                    string absolutePath = Path.Combine(UserFileService.GetJsonDirectory, relativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));

                    if (File.Exists(absolutePath))
                    {
                        string fileUri = $"file://{absolutePath.Replace("\\", "/")}";
                        RelatedImages.Add(fileUri);

                        // Set only the first valid image as UploadedImage
                        if (!imageSet)
                        {
                            selected.UploadedImage = ImageSource.FromUri(new Uri(fileUri));
                            imageSet = true;
                        }
                    }
                }
            }

            // Fallback if no valid images were found
            if (!imageSet)
            {
                RelatedImages.Add("event_placeholder.png");
                selected.UploadedImage = "event_placeholder.png";
            }

            // Force UI to refresh all image-related bindings
            OnPropertyChanged(nameof(RelatedImages));
            OnPropertyChanged(nameof(Venue));
            OnPropertyChanged(nameof(VenueImage)); // Optional if you still use VenueImage wrapper
        }





        private Booking _confirmedBooking;
        public Booking ConfirmedBooking
        {
            get => _confirmedBooking;
            set { _confirmedBooking = value; OnPropertyChanged(); }
        }

        // ========== ✅ BOOKING BINDINGS BELOW ==========

        public DateTime MinBookingDate => DateTime.Now.AddDays(3);

        private bool _isBookingVisible;
        public bool IsBookingVisible
        {
            get => _isBookingVisible;
            set { _isBookingVisible = value; OnPropertyChanged(); }
        }

        private DateTime _bookingDate = DateTime.Now.AddDays(3);
        public DateTime BookingDate
        {
            get => _bookingDate;
            set { _bookingDate = value; OnPropertyChanged(); }
        }

        public ObservableCollection<int> GuestOptions { get; set; }
        private int _selectedGuestCount;
        public int SelectedGuestCount
        {
            get => _selectedGuestCount;
            set { _selectedGuestCount = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> TimeOptions { get; set; }
        private string _selectedTimeSlot;
        public string SelectedTimeSlot
        {
            get => _selectedTimeSlot;
            set { _selectedTimeSlot = value; OnPropertyChanged(); }
        }

        private bool _isExtraChairsChecked;
        public bool IsExtraChairsChecked
        {
            get => _isExtraChairsChecked;
            set { _isExtraChairsChecked = value; OnPropertyChanged(); }
        }

        public ObservableCollection<int> ExtraChairsOptions { get; set; }
        private int _selectedExtraChairs;
        public int SelectedExtraChairs
        {
            get => _selectedExtraChairs;
            set { _selectedExtraChairs = value; OnPropertyChanged(); }
        }

        private bool _isTimeExtensionChecked;
        public bool IsTimeExtensionChecked
        {
            get => _isTimeExtensionChecked;
            set { _isTimeExtensionChecked = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> TimeExtensionOptions { get; set; }
        private string _selectedTimeExtension;
        public string SelectedTimeExtension
        {
            get => _selectedTimeExtension;
            set { _selectedTimeExtension = value; OnPropertyChanged(); }
        }

        public ICommand ConfirmBookingCommand { get; private set; }

        private void InitBookingOptions()
        {
            GuestOptions = new ObservableCollection<int>();
            if (int.TryParse(Venue?.Capacity?.Split(' ')[0], out int maxCapacity))
            {
                for (int i = 100; i <= maxCapacity; i += 20)
                    GuestOptions.Add(i);
            }

            TimeOptions = new ObservableCollection<string> { "Morning (8AM - 1PM)", "Evening (5PM - 9PM)" };
            ExtraChairsOptions = new ObservableCollection<int> { 10, 20, 30, 40, 50 };
            TimeExtensionOptions = new ObservableCollection<string> { "30 minutes", "1 hour", "2 hours" };

            ConfirmBookingCommand = new Command(OnConfirmBooking);

            OnPropertyChanged(nameof(GuestOptions));
            OnPropertyChanged(nameof(TimeOptions));
            OnPropertyChanged(nameof(ExtraChairsOptions));
            OnPropertyChanged(nameof(TimeExtensionOptions));
        }

        private async void OnConfirmBooking()
        {
            IsBookingVisible = false;
            OnPropertyChanged(nameof(IsBookingVisible));

            ConfirmedBooking = new Booking
            {
                BookingId = Guid.NewGuid().ToString(),
                Image = Venue?.ImageUrl,
                Headline = Venue?.Title,
                PublishedDate = DateTime.Now.ToString("MMMM dd, yyyy"),
                Status = "Pending",
                BookingDate = BookingDate,
                GuestCount = SelectedGuestCount,
                EventTime = SelectedTimeSlot,
                ExtraChairs = IsExtraChairsChecked ? SelectedExtraChairs.ToString() : "None",
                TimeExtension = IsTimeExtensionChecked ? SelectedTimeExtension : "None",
                Price = await CalculatePriceAsync()

            };

            await UserFileService.SaveBookingAsync(Username, ConfirmedBooking);

            // 🛎️ Notify the event space owner
            int ownerId = await UserFileService.FindOwnerIdByEventTitleAsync(Venue?.Title);
            if (ownerId > 0)
            {
                SpaceOwnerFileService.AddNotificationByOwnerId(ownerId, "New Booking",
                    $"{Username} booked your '{Venue?.Title}' with BookingID {ConfirmedBooking.BookingId}. Confirm it now.");
            }

            await Application.Current.MainPage.DisplayAlert("Success", "Your booking has been confirmed!", "OK");

            // ✅ Redirect to CartPage with username and email
            await Application.Current.MainPage.Navigation.PushAsync(new View.CartPage(Username, Email));
        }

        private async Task<decimal> CalculatePriceAsync()
        {
            decimal basePrice = 0;
            decimal extensionRate = 0;
            decimal tableRate = 0;

            try
            {
                int ownerId = await UserFileService.FindOwnerIdByEventTitleAsync(Venue?.Title);
                if (ownerId > 0)
                {
                    string filePath = Path.Combine(UserFileService.GetJsonDirectory, $"EO-{ownerId}.txt");
                    var ownerEvents = await UserFileService.LoadEventsAsync(filePath);
                    var matchingEvent = ownerEvents.FirstOrDefault(e => e.Title == Venue?.Title);

                    if (matchingEvent != null)
                    {
                        basePrice = decimal.TryParse(matchingEvent.DailyRate, out var daily) ? daily : 0;
                        extensionRate = decimal.TryParse(matchingEvent.ExtensionRate, out var ext) ? ext : 0;
                        tableRate = decimal.TryParse(matchingEvent.TableRate, out var table) ? table : 0;
                    }
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to calculate price: {ex.Message}", "OK");
            }

            decimal extraChairsCost = IsExtraChairsChecked ? SelectedExtraChairs * tableRate : 0;
            decimal extensionCost = IsTimeExtensionChecked ? TimeExtensionToCost(extensionRate, SelectedTimeExtension) : 0;

            return basePrice + extraChairsCost + extensionCost;
        }

        private decimal TimeExtensionToCost(decimal ratePerHour, string extension)
        {
            return extension switch
            {
                "30 minutes" => ratePerHour / 2,
                "1 hour" => ratePerHour,
                "2 hours" => ratePerHour * 2,
                _ => 0
            };
        }




        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
