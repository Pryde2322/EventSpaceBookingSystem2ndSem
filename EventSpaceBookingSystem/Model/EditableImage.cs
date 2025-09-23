using Microsoft.Maui.Controls;
using System.ComponentModel;

namespace EventSpaceBookingSystem.Model
{
    public class EditableImage : INotifyPropertyChanged
    {
        public string Path { get; set; }
        public ImageSource Source { get; set; }
        public bool IsNew { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
