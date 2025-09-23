using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EventSpaceBookingSystem.ViewModel
{
    public class NavigationBarViewModel : INotifyPropertyChanged
    {
        private string _username;
        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        private string _status;
        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsOwnerTabVisible));
            }
        }

        // This will automatically hide the owner tab if the user is already an event space owner
        public bool IsOwnerTabVisible => Status != "event space owner";

        public NavigationBarViewModel(string username, string status)
        {
            Username = username;
            Status = status;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
