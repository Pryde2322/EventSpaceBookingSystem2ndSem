using System;
using System.Collections.ObjectModel;
using System.IO;
using Newtonsoft.Json;
using EventSpaceBookingSystem.Model;

namespace EventSpaceBookingSystem.ViewModel
{
    public class NotifBoxViewModel
    {
        public ObservableCollection<NotificationModel> Notifications { get; set; } = new();

        private readonly string filePath;

        public NotifBoxViewModel(int ownerId)
        {
            string notifDir = Path.Combine(Services.UserFileService.GetJsonDirectory, "OwnerNotifs", ownerId.ToString());
            if (!Directory.Exists(notifDir))
            {
                Directory.CreateDirectory(notifDir);
            }

            filePath = Path.Combine(notifDir, "notifications.txt");
            LoadNotifications();
        }

        private void LoadNotifications()
        {
            if (File.Exists(filePath))
            {
                string content = File.ReadAllText(filePath);
                var data = JsonConvert.DeserializeObject<ObservableCollection<NotificationModel>>(content);
                if (data != null)
                    Notifications = new ObservableCollection<NotificationModel>(data);
            }
        }

        private void SaveNotifications()
        {
            string content = JsonConvert.SerializeObject(Notifications, Formatting.Indented);
            File.WriteAllText(filePath, content);
        }

        /// <summary>
        /// Adds an owner-targeted notification.
        /// </summary>
        public void AddNotification(string type, string username, string eventSpaceName, string bookingId, string rating = "")
        {
            string title = "Event Notification";
            string message = "";

            switch (type.ToLower())
            {
                case "booked":
                    message = $"{username} booked your {eventSpaceName} with a BookingID {bookingId}. Confirm it now.";
                    break;
                case "cancelled":
                    message = $"{username} canceled your {eventSpaceName} with a BookingID {bookingId}.";
                    break;
                case "paid":
                    message = $"{username} paid your {eventSpaceName} with a BookingID {bookingId}.";
                    break;
                case "started":
                    message = $"The event started booked by {username} with a BookingID {bookingId}.";
                    break;
                case "rated":
                    message = $"{username} rated your {eventSpaceName} with a {rating} stars.";
                    break;
                default:
                    message = "An update occurred related to your event space.";
                    break;
            }

            var newNotif = new NotificationModel
            {
                Title = title,
                Message = message,
                Timestamp = DateTime.Now.ToString("MMM dd, yyyy h:mm tt")
            };

            Notifications.Insert(0, newNotif); // Newest on top
            SaveNotifications();
        }
    }
}
