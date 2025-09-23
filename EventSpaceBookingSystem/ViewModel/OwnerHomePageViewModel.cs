using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EventSpaceBookingSystem.Model;
using EventSpaceBookingSystem.Services;

namespace EventSpaceBookingSystem.ViewModel
{
    public class OwnerHomePageViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<EventModel> eventSpaces = new();
        private int _ownerId;
        public int OwnerId => _ownerId;

        public OwnerHomePageViewModel(int ownerId)
        {
            _ownerId = ownerId;
            _ = LoadSpacesAsync(); // Load owned event spaces on initialization
        }

        public ObservableCollection<EventModel> EventSpaces
        {
            get => eventSpaces;
            set
            {
                if (eventSpaces != value)
                {
                    eventSpaces = value;
                    OnPropertyChanged();
                }
            }
        }

        public async Task LoadSpacesAsync()
        {
            await SpaceOwnerFileService.LoadFromFileAsync(EventSpaces, _ownerId);
        }

        public async Task RemoveSpaceAsync(EventModel eventModel)
        {
            if (EventSpaces.Contains(eventModel))
            {
                EventSpaces.Remove(eventModel);
                await SpaceOwnerFileService.SaveToFileAsync(EventSpaces, _ownerId);
            }
        }
        //public async Task UpdateSpaceAsync(EventModel updatedEvent)
        //{
        //    var existing = EventSpaces.FirstOrDefault(e => e. == updatedEvent.Id);
        //    if (existing != null)
        //    {
        //        int index = EventSpaces.IndexOf(existing);
        //        EventSpaces[index] = updatedEvent;
        //        await SpaceOwnerFileService.SaveToFileAsync(EventSpaces, _ownerId);
        //    }
        //}
        public async Task ReloadAsync()
        {
            EventSpaces.Clear();
            await LoadSpacesAsync();
        }


        public async Task AddSpaceAsync(EventModel eventModel)
        {
            EventSpaces.Add(eventModel);
            await SpaceOwnerFileService.SaveToFileAsync(EventSpaces, _ownerId);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
