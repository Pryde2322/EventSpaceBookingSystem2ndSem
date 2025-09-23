using System;
using System.Collections.ObjectModel;
using System.IO;
using Newtonsoft.Json;
using EventSpaceBookingSystem.Model;
using System.Xml;

namespace EventSpaceBookingSystem.ViewModel
{
    public class NotificationMopupsViewModel
    {
        public ObservableCollection<NotificationModel> Notifications { get; set; } = new();

        private readonly string filePath;

        public NotificationMopupsViewModel(int userId)
        {
            string notifDir = Path.Combine(Services.UserFileService.GetJsonDirectory, "UserNotifs", userId.ToString());
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
            string content = JsonConvert.SerializeObject(Notifications, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(filePath, content);
        }

        /// <summary>
        /// Adds a new notification based on the change type and detail.
        /// </summary>
        /// <param name="changeType">E.g., status_update, booking_confirmed, booking_cancelled</param>
        /// <param name="detail">Optional detail (e.g., new status, venue name)</param>
        public void AddNotification(string changeType, string detail = "")
        {
            string title = "";
            string message = "";

            switch (changeType.ToLower())
            {
                case "status_update":
                    title = "Account Status Changed";
                    message = $"Your account status has been updated to: {detail}.";
                    break;

                case "booking_confirmed":
                    title = "Booking Confirmed";
                    message = $"Your booking for '{detail}' has been confirmed.";
                    break;

                case "booking_cancelled":
                    title = "Booking Cancelled";
                    message = $"Your booking for '{detail}' has been cancelled.";
                    break;

                case "profile_updated":
                    title = "Profile Updated";
                    message = "Your profile information was successfully updated.";
                    break;

                case "avatar_updated":
                    title = "Profile Picture Changed";
                    message = "You have successfully updated your profile picture.";
                    break;

                case "booking_paid":
                    title = "Payment Successful";
                    message = $"Your payment for '{detail}' has been processed successfully.";
                    break;

                case "booking_rated":
                    title = "Thank You for Rating";
                    message = $"You rated your experience at '{detail}'. Thank you for your feedback!";
                    break;

                default:
                    title = "Notification";
                    message = string.IsNullOrEmpty(detail) ? "A change has been made to your account." : detail;
                    break;
            }

            var newNotif = new NotificationModel
            {
                Title = title,
                Message = message,
                Timestamp = DateTime.Now.ToString("MMM dd, yyyy h:mm tt")
            };

            Notifications.Insert(0, newNotif); // newest on top
            SaveNotifications();
        }
    }
}
