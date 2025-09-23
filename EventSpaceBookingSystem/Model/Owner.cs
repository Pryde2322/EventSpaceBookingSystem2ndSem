using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EventSpaceBookingSystem.Model
{
    public class Owner : INotifyPropertyChanged
    {
        private bool isActive;
        private string raMenCoStatus;

        public string Name { get; set; }
        public int Spaces { get; set; }
        public string LastActive { get; set; }

        public bool IsActive
        {
            get => isActive;
            set
            {
                if (isActive != value)
                {
                    isActive = value;
                    OnPropertyChanged();
                    RaMenCoStatus = isActive ? "Activated" : "Deactivated"; // Auto-update RaMenCoStatus
                }
            }
        }

        public string RaMenCoStatus
        {
            get => raMenCoStatus;
            set
            {
                if (raMenCoStatus != value)
                {
                    raMenCoStatus = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
