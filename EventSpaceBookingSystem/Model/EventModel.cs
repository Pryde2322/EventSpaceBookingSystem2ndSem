using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace EventSpaceBookingSystem.Model
{
    public class EventModel : INotifyPropertyChanged
    {
        private string _title;
        
        private int _rating;
        private int _reviewCount;
        private string _capacity;
        private string _formattedPrice;
        private string _dailyRate;
        private string _extensionRate;
        private string _tableRate;
        private string _location;
        private string _category;
        private string _description1;
        private string _description2;

        public string Title { get => _title; set { _title = value; OnPropertyChanged(); } }
        
        public int Rating { get => _rating; set { _rating = value; OnPropertyChanged(); } }
        public int ReviewCount { get => _reviewCount; set { _reviewCount = value; OnPropertyChanged(); } }
        public string Capacity { get => _capacity; set { _capacity = value; OnPropertyChanged(); } }
        public string FormattedPrice { get => _formattedPrice; set { _formattedPrice = value; OnPropertyChanged(); } }
        public string DailyRate { get => _dailyRate; set { _dailyRate = value; OnPropertyChanged(); } }
        public string ExtensionRate { get => _extensionRate; set { _extensionRate = value; OnPropertyChanged(); } }
        public string TableRate { get => _tableRate; set { _tableRate = value; OnPropertyChanged(); } }
        public string Location { get => _location; set { _location = value; OnPropertyChanged(); } }
        public string Category { get => _category; set { _category = value; OnPropertyChanged(); } }

        public string Description1 { get => _description1; set { _description1 = value; OnPropertyChanged(); } }
        public string Description2 { get => _description2; set { _description2 = value; OnPropertyChanged(); } }

        private ImageSource _uploadedImage;
        public ImageSource UploadedImage
        {
            get => _uploadedImage;
            set { _uploadedImage = value; OnPropertyChanged(); }
        }

        public List<string> ImageUrls { get; set; } = new List<string>();

        public string ImageUrl => ImageUrls?.FirstOrDefault(); // Optional: shortcut to first


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
