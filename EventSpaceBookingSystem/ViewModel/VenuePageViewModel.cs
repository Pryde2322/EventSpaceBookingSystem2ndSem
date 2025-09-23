using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using EventSpaceBookingSystem.Model;
using EventSpaceBookingSystem.Services;
using EventSpaceBookingSystem.View;

namespace EventSpaceBookingSystem.ViewModel
{
    public class VenuePageViewModel : INotifyPropertyChanged
    {
        private readonly EventVenueViewModel _eventVenueVM = new();

        private ObservableCollection<EventModel> _venues;
        public ObservableCollection<EventModel> Venues
        {
            get => _venues;
            set { _venues = value; OnPropertyChanged(); }
        }

        private ObservableCollection<EventModel> _allVenues = new();

        private string _username;
        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        private string _searchQuery;
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (_searchQuery != value)
                {
                    _searchQuery = value;
                    OnPropertyChanged();
                    FilterVenues();
                }
            }
        }

        private string _locationQuery;
        public string LocationQuery
        {
            get => _locationQuery;
            set
            {
                if (_locationQuery != value)
                {
                    _locationQuery = value;
                    OnPropertyChanged();
                    FilterVenues();
                }
            }
        }

        public List<string> Categories { get; } = new() { "All Categories", "Mountain", "Beach", "Hotel", "Resort", "Garden", "Conference Hall", "Banquet Hall", "Rooftop", "Lounge", "Private Villa" };
        public string SelectedCategory { get; set; } = "All Categories";

        public List<int> GuestOptions { get; } = new() { 0, 50, 100, 200, 300, 400, 500 }; // 0 means 'All'
        public int SelectedGuest { get; set; } = 0;

        public List<double> RatingOptions { get; } = new() { 0.0, 1.0, 2.0, 3.0, 4.0, 5.0 }; // 0.0 means 'All'
        public double SelectedRating { get; set; } = 0.0;

        private const int ItemsPerPage = 15;
        private int _totalPages = 1;

        private int _currentPage = 1;
        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                if (_currentPage != value)
                {
                    _currentPage = value;
                    OnPropertyChanged();
                    UpdatePagedVenues();
                }
            }
        }

        private int _pageWindowStart = 1;
        private const int MaxVisiblePages = 3;

        private ObservableCollection<int> _pageNumbers;
        public ObservableCollection<int> PageNumbers
        {
            get => _pageNumbers;
            set { _pageNumbers = value; OnPropertyChanged(); }
        }

        public ICommand SearchCommand { get; }
        public ICommand ApplyFilterCommand { get; }
        public ICommand ViewTypeCommand { get; }
        public ICommand ExploreCommand { get; }
        public ICommand CtaCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand GoToPageCommand { get; }

        public VenuePageViewModel()
        {
            SearchCommand = new Command(FilterVenues);
            ApplyFilterCommand = new Command(ApplyCombinedFilter);
            ViewTypeCommand = new Command<string>(OnViewTypeChanged);
            ExploreCommand = new Command<EventModel>(OnExplore);
            CtaCommand = new Command(OnCta);

            PreviousPageCommand = new Command(OnPreviousPage);
            NextPageCommand = new Command(OnNextPage);
            GoToPageCommand = new Command<int>(OnGoToPage);
        }

        public async Task InitAsync(string username)
        {
            Username = username;
            await _eventVenueVM.InitAsync();

            _allVenues = _eventVenueVM.PopularEvents ?? new ObservableCollection<EventModel>();

            // Reset filters to defaults
            SelectedCategory = "All Categories";
            SelectedGuest = 0;
            SelectedRating = 0.0;

            SearchQuery = string.Empty;
            LocationQuery = string.Empty;

            CurrentPage = 1;
            UpdatePagedVenues();
        }


        private void FilterVenues()
        {
            // Show full list if both search & location are empty
            if (string.IsNullOrWhiteSpace(SearchQuery) && string.IsNullOrWhiteSpace(LocationQuery))
            {
                _allVenues = _eventVenueVM.PopularEvents ?? new ObservableCollection<EventModel>();
            }
            else
            {
                var filtered = _eventVenueVM.PopularEvents.Where(e =>
                    (string.IsNullOrWhiteSpace(SearchQuery) || e.Title?.ToLower().Contains(SearchQuery.ToLower()) == true) &&
                    (string.IsNullOrWhiteSpace(LocationQuery) || e.Location?.ToLower().Contains(LocationQuery.ToLower()) == true)
                ).ToList();

                _allVenues = new ObservableCollection<EventModel>(filtered);
            }

            CurrentPage = 1;
            UpdatePagedVenues();
        }


        private async void ApplyCombinedFilter()
        {
            var ownerVenues = await UserFileService.LoadAllEventSpacesAsync();

            var combinedVenues = (_eventVenueVM.PopularEvents ?? new ObservableCollection<EventModel>())
                                 .Concat(ownerVenues)
                                 .ToList();

            var filtered = combinedVenues.Where(e =>
                (string.IsNullOrWhiteSpace(SearchQuery) || e.Title?.ToLower().Contains(SearchQuery.ToLower()) == true) &&
                (string.IsNullOrWhiteSpace(LocationQuery) || e.Location?.ToLower().Contains(LocationQuery.ToLower()) == true) &&
                (SelectedCategory == "All Categories" ||
                 (e.Category != null && e.Category.Split(',').Any(cat => cat.Trim().Equals(SelectedCategory, StringComparison.OrdinalIgnoreCase)))) &&
                (SelectedGuest == 0 || int.TryParse(e.Capacity, out var capacity) && capacity >= SelectedGuest) &&
                (SelectedRating == 0 || e.Rating >= SelectedRating)
            ).ToList();

            _allVenues = new ObservableCollection<EventModel>(filtered);
            CurrentPage = 1;
            UpdatePagedVenues();
        }






        private void UpdatePagedVenues()
        {
            var totalItems = _allVenues.Count;
            _totalPages = (int)Math.Ceiling((double)totalItems / ItemsPerPage);

            var paged = _allVenues.Skip((CurrentPage - 1) * ItemsPerPage).Take(ItemsPerPage);
            Venues = new ObservableCollection<EventModel>(paged);

            int start = Math.Max(1, CurrentPage - 1);
            int end = Math.Min(_totalPages, start + 2);

            if (end - start < 2 && start > 1)
                start = Math.Max(1, end - 2);

            PageNumbers = new ObservableCollection<int>(Enumerable.Range(start, end - start + 1));
        }

        private void OnPreviousPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                if (CurrentPage < _pageWindowStart)
                    _pageWindowStart--;
            }
        }

        private void OnNextPage()
        {
            if (CurrentPage < _totalPages)
            {
                CurrentPage++;
                if (CurrentPage >= _pageWindowStart + MaxVisiblePages)
                    _pageWindowStart++;
            }
        }

        private void OnGoToPage(int page)
        {
            if (page >= 1 && page <= _totalPages)
                CurrentPage = page;
        }

        private async void OnExplore(EventModel selectedEvent)
        {
            if (selectedEvent != null)
                await App.Current.MainPage.Navigation.PushAsync(new ExplorePage(Username, selectedEvent));
        }

        private async void OnCta()
        {
            await App.Current.MainPage.DisplayAlert("Not Available", "This feature is not currently available", "OK");
        }

        private void OnViewTypeChanged(string viewType)
        {
            // Optional: handle toggle between grid and list view
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
