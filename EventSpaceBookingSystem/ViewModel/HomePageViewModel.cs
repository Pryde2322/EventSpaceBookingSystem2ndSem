using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using EventSpaceBookingSystem.Model;
using EventSpaceBookingSystem.View;

namespace EventSpaceBookingSystem.ViewModel
{
    public class HomePageViewModel : INotifyPropertyChanged
    {
        private readonly EventVenueViewModel _eventVenueViewModel = new();

        public ObservableCollection<EventModel> PopularEvents { get; set; } = new();
        public ObservableCollection<EventModel> FilteredEvents { get; set; } = new();

        
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
                    OnSearchNavigate();
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
                    OnSearchNavigate();
                }
            }
        }

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

        public EventModel CurrentEvent => FilteredEvents.Count > 0 ? FilteredEvents[CurrentIndex] : null;

        public ICommand PreviousCommand { get; }
        public ICommand NextCommand { get; }
        public ICommand ExploreCommand { get; }
        public ICommand SearchCommand { get; }

        public HomePageViewModel()
        {
            PreviousCommand = new Command(OnPrevious);
            NextCommand = new Command(OnNext);
            ExploreCommand = new Command<EventModel>(OnExplore);
            SearchCommand = new Command(OnSearchNavigate); // Updated
        }

        public async Task InitAsync(string username)
        {
            Username = username;

            await _eventVenueViewModel.InitAsync();

            PopularEvents = _eventVenueViewModel.PopularEvents ?? new ObservableCollection<EventModel>();
            FilteredEvents = new ObservableCollection<EventModel>(PopularEvents);
            _currentIndex = 0;

            OnPropertyChanged(nameof(PopularEvents));
            OnPropertyChanged(nameof(FilteredEvents));
            OnPropertyChanged(nameof(CurrentEvent));
        }

        private async void OnSearchNavigate()
        {
            // Both fields are empty — just show defaults
            if (string.IsNullOrWhiteSpace(SearchQuery) && string.IsNullOrWhiteSpace(LocationQuery))
            {
                var venuePage = new VenuePage(Username); // existing constructor with only username
                await App.Current.MainPage.Navigation.PushAsync(venuePage);
            }
            else
            {
                // At least one filter is present — show filtered results
                var venuePage = new VenuePage(Username, SearchQuery, LocationQuery);
                await App.Current.MainPage.Navigation.PushAsync(venuePage);
            }
        }




        private void OnPrevious()
        {
            if (CurrentIndex > 0)
            {
                CurrentIndex--;
            }
        }

        private void OnNext()
        {
            if (CurrentIndex < FilteredEvents.Count - 1)
            {
                CurrentIndex++;
            }
        }

        private async void OnExplore(EventModel selectedEvent)
        {
            if (selectedEvent != null)
            {
                await App.Current.MainPage.Navigation.PushAsync(new ExplorePage(Username, selectedEvent));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
