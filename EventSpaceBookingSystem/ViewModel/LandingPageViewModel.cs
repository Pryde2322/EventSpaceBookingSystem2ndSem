using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using EventSpaceBookingSystem.Model;
using EventSpaceBookingSystem.View;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;

namespace EventSpaceBookingSystem.ViewModel
{
    public class LandingPageViewModel : INotifyPropertyChanged
    {
        private EventVenueViewModel _eventVenueViewModel;

        private ObservableCollection<EventModel> _popularEvents;
        public ObservableCollection<EventModel> PopularEvents
        {
            get => _popularEvents;
            set { _popularEvents = value; OnPropertyChanged(); }
        }

        private ObservableCollection<EventModel> _filteredEvents;
        public ObservableCollection<EventModel> FilteredEvents
        {
            get => _filteredEvents;
            set { _filteredEvents = value; OnPropertyChanged(); }
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
                    OnSearch();
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
                    OnSearch();
                }
            }
        }

        public bool IsBusy { get; set; }

        private int _currentIndex;
        public int CurrentIndex
        {
            get => _currentIndex;
            set
            {
                if (_currentIndex != value)
                {
                    _currentIndex = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CurrentEvent));
                }
            }
        }

        public EventModel CurrentEvent => FilteredEvents?.Count > 0 ? FilteredEvents[CurrentIndex] : null;

        public ICommand PreviousCommand { get; }
        public ICommand NextCommand { get; }
        public ICommand ExploreCommand { get; }
        public ICommand SearchCommand { get; }

        public LandingPageViewModel()
        {
            PopularEvents = new ObservableCollection<EventModel>();
            FilteredEvents = new ObservableCollection<EventModel>();

            SearchCommand = new Command(OnSearch);
            ExploreCommand = new Command<EventModel>(OnExplore);
            PreviousCommand = new Command(OnPrevious);
            NextCommand = new Command(OnNext);

            _currentIndex = 0;

            // Load event data
            _ = InitAsync();
        }

        private async Task InitAsync()
        {
            _eventVenueViewModel = new EventVenueViewModel();
            await _eventVenueViewModel.InitAsync();

            PopularEvents = _eventVenueViewModel.PopularEvents ?? new ObservableCollection<EventModel>();
            FilteredEvents = new ObservableCollection<EventModel>(PopularEvents);

            OnPropertyChanged(nameof(PopularEvents));
            OnPropertyChanged(nameof(FilteredEvents));
        }

        private void OnSearch()
        {
            if (PopularEvents == null) return;

            var filtered = PopularEvents
                .Where(e =>
                    (string.IsNullOrWhiteSpace(SearchQuery) || e.Title?.ToLower().Contains(SearchQuery.ToLower()) == true) &&
                    (string.IsNullOrWhiteSpace(LocationQuery) || e.Location?.ToLower().Contains(LocationQuery.ToLower()) == true))
                .OrderByDescending(e => e.Rating)
                .ToList();

            FilteredEvents = new ObservableCollection<EventModel>(filtered);
        }

        private void OnPrevious()
        {
            if (CurrentIndex > 0)
                CurrentIndex--;
        }

        private void OnNext()
        {
            if (CurrentIndex < FilteredEvents.Count - 1)
                CurrentIndex++;
        }

        private async void OnExplore(EventModel selectedEvent)
        {
            if (selectedEvent != null)
            {
                await App.Current.MainPage.Navigation.PushAsync(new ExplorePage(selectedEvent));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
