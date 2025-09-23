using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using EventSpaceBookingSystem.Model;
using EventSpaceBookingSystem.Services;

namespace EventSpaceBookingSystem.ViewModel
{
    public class EventVenueViewModel : INotifyPropertyChanged
    {
        private const int MaxEvents = 100;

        public ObservableCollection<EventModel> PopularEvents { get; private set; } = new();

        private static readonly string[] Categories = new[]
        {
            "Mountain", "Beach", "Hotel", "Resort", "Garden",
            "Conference Hall", "Banquet Hall", "Rooftop", "Lounge", "Private Villa"
        };

        public EventVenueViewModel()
        {
            // Constructor no longer runs the async task directly
        }

        public async Task InitAsync()
        {
            await LoadEventSpacesAsync();
        }

        private async Task LoadEventSpacesAsync()
        {
            try
            {
                var loadedEvents = await UserFileService.LoadAllEventSpacesAsync();
                var sortedEvents = loadedEvents.OrderByDescending(e => e.Rating).ToList();

                foreach (var ev in sortedEvents)
                {
                    string resolvedPath = UserFileService.ResolveImagePath(ev.ImageUrls?.FirstOrDefault());
                    ev.UploadedImage = ImageSource.FromFile(resolvedPath);
                }


                Random rand = new Random();

                if (sortedEvents.Count < MaxEvents)
                {
                    int remaining = MaxEvents - sortedEvents.Count;
                    for (int i = 0; i < remaining; i++)
                    {
                        var selectedCategories = Categories
                            .OrderBy(_ => rand.Next())
                            .Take(rand.Next(1, 4))
                            .ToList();

                        string categoryString = string.Join(", ", selectedCategories);

                        sortedEvents.Add(new EventModel
                        {
                            Title = $"Sample Venue {i + 1}",
                            UploadedImage = "event_placeholder.png",
                            Rating = 5,
                            ReviewCount = 10,
                            Capacity = $"{100 + i * 10}",
                            FormattedPrice = $"₱{999.99 + i * 50:0.00} / day",
                            Location = i % 2 == 0 ? "Cebu" : "Manila",
                            Category = categoryString,
                            Description1 = "Lorem ipsum dolor sit amet, consectetur adipiscing elit.",
                            Description2 = "Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."
                        });
                    }
                }

                PopularEvents = new ObservableCollection<EventModel>(sortedEvents);
                OnPropertyChanged(nameof(PopularEvents));
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", $"Failed to load events: {ex.Message}", "OK");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
