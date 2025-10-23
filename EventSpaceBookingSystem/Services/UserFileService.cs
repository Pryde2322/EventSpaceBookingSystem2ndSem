using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using EventSpaceBookingSystem.Model;
using System.Linq;
using System.Collections.ObjectModel;
using Microsoft.Maui.Storage; // ✅ needed for FileSystem.AppDataDirectory

namespace EventSpaceBookingSystem.Services
{
    public static class UserFileService
    {
        // ✅ Cross-platform JSON folder handling
#if ANDROID
        private static readonly string JsonDirectory = FileSystem.AppDataDirectory;
#else
        private static readonly string JsonDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Json"));
#endif

        private static readonly string UsersFilePath = Path.Combine(JsonDirectory, "Users.txt");
        private static readonly string EventSpaceOwnersFilePath = Path.Combine(JsonDirectory, "EventSpaceOwners.txt");

        public static string GetJsonDirectory => JsonDirectory;
        public static string GetUsersFilePath => UsersFilePath;
        public static string GetEventSpaceOwnersFilePath => EventSpaceOwnersFilePath;

        static UserFileService()
        {
            if (!Directory.Exists(JsonDirectory))
                Directory.CreateDirectory(JsonDirectory);

            if (!File.Exists(UsersFilePath) || string.IsNullOrWhiteSpace(File.ReadAllText(UsersFilePath)))
                File.WriteAllText(UsersFilePath, "[]");

            if (!File.Exists(EventSpaceOwnersFilePath) || string.IsNullOrWhiteSpace(File.ReadAllText(EventSpaceOwnersFilePath)))
                File.WriteAllText(EventSpaceOwnersFilePath, "[]");

#if DEBUG
            Console.WriteLine($"[UserFileService] JSON Directory: {JsonDirectory}");
#endif
        }

        public static async Task SaveUserAsync(Users user)
        {
            try
            {
                if (user.Status == "event space owner")
                {
                    var userId = await SaveToFileAsync(user, EventSpaceOwnersFilePath);
                    await CreateEventSpaceOwnerFile(userId, user);
                    AddNotificationById(userId, "Account Status Changed", "Your account has been upgraded to Event Space Owner.");
                }
                else
                {
                    var userId = await SaveToFileAsync(user, UsersFilePath);
                    AddNotificationById(userId, "Account Updated", "Your account information has been updated.");
                }
            }
            catch (Exception ex)
            {
                var mainPage = App.Current?.Windows[0]?.Page;
                if (mainPage != null)
                    await mainPage.DisplayAlert("Error", $"Failed to save user: {ex.Message}", "OK");
            }
        }

        public static async Task<int> SaveUserAndReturnIdAsync(Users user)
        {
            try
            {
                if (user.Status == "event space owner")
                {
                    var userId = await SaveToFileAsync(user, EventSpaceOwnersFilePath);
                    await CreateEventSpaceOwnerFile(userId, user);
                    return userId;
                }
                else
                {
                    var userId = await SaveToFileAsync(user, UsersFilePath);
                    return userId;
                }
            }
            catch (Exception ex)
            {
                var mainPage = App.Current?.Windows[0]?.Page;
                if (mainPage != null)
                    await mainPage.DisplayAlert("Error", $"Failed to save user: {ex.Message}", "OK");

                return -1;
            }
        }

        public static async Task<List<EventModel>> LoadAllEventSpacesAsync()
        {
            var events = new List<EventModel>();
            var activeOwnerIds = new List<int>();

            string ownersPath = EventSpaceOwnersFilePath;
            if (File.Exists(ownersPath))
            {
                var json = await File.ReadAllTextAsync(ownersPath);
                var users = JsonSerializer.Deserialize<List<Users>>(json);
                if (users != null)
                {
                    activeOwnerIds = users
                        .Where(u => string.Equals(u.RaMenCoStatus, "Activated", StringComparison.OrdinalIgnoreCase))
                        .Select(u => u.Id)
                        .ToList();
                }
            }

            foreach (var ownerId in activeOwnerIds)
            {
                string eoFile = Path.Combine(JsonDirectory, $"EO-{ownerId}.txt");
                if (!File.Exists(eoFile))
                    continue;

                try
                {
                    var loaded = await LoadEventModelsAsync(eoFile);
                    if (loaded != null && loaded.Any())
                        events.AddRange(loaded);
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert("Warning",
                        $"Skipped '{Path.GetFileName(eoFile)}': {ex.Message}", "OK");
                }
            }

            return events.OrderByDescending(e => e.Rating).ToList();
        }

        public static async Task<List<EventModel>> LoadEventsAsync(string filePath)
        {
            if (!File.Exists(filePath)) return new List<EventModel>();
            var json = await File.ReadAllTextAsync(filePath);
            if (string.IsNullOrWhiteSpace(json)) return new List<EventModel>();

            try
            {
                return JsonSerializer.Deserialize<List<EventModel>>(json) ?? new List<EventModel>();
            }
            catch (JsonException)
            {
                return new List<EventModel>();
            }
        }

        public static async Task SaveEventAsync(EventModel eventModel, string ownerId)
        {
            string filePath = Path.Combine(JsonDirectory, $"EO-{ownerId}.txt");
            var events = await LoadEventsAsync(filePath);
            events.Add(eventModel);

            var serialized = JsonSerializer.Serialize(events, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, serialized);
        }

        private static async Task<int> SaveToFileAsync(Users user, string filePath)
        {
            var users = await LoadUsersAsync(filePath);
            int userId = users.Count + 1;
            user.Id = userId;
            users.Add(user);

            var serialized = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, serialized);

            return userId;
        }

        public static async Task SaveUsersAsync(List<Users> users, string filePath)
        {
            try
            {
                var json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving users to {filePath}: {ex.Message}");
                var mainPage = App.Current?.Windows[0]?.Page;
                if (mainPage != null)
                    await mainPage.DisplayAlert("Error", $"Failed to save user list: {ex.Message}", "OK");
            }
        }

        public static async Task<List<Users>> LoadUsersAsync(string filePath = null)
        {
            filePath ??= UsersFilePath;
            if (!File.Exists(filePath)) return new List<Users>();

            string json = await File.ReadAllTextAsync(filePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                await File.WriteAllTextAsync(filePath, "[]");
                return new List<Users>();
            }

            try
            {
                return JsonSerializer.Deserialize<List<Users>>(json) ?? new List<Users>();
            }
            catch (JsonException)
            {
                var mainPage = App.Current?.Windows[0]?.Page;
                if (mainPage != null)
                    await mainPage.DisplayAlert("Error", "The file is corrupted or invalid.", "OK");
                return new List<Users>();
            }
        }

        private static async Task CreateEventSpaceOwnerFile(int userId, Users user)
        {
            string fileName = $"EO-{userId}.txt";
            string filePath = Path.Combine(JsonDirectory, fileName);

            if (!File.Exists(filePath))
            {
                var serialized = JsonSerializer.Serialize(user, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(filePath, serialized);
            }
        }

        public static async Task<bool> UserExistsAsync(string email, string username)
        {
            var users = await LoadUsersAsync();
            return users.Any(u => u.Email == email || u.Username == username);
        }

        public static async Task<List<EventModel>> LoadEventModelsAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath)) return new List<EventModel>();
                string json = await File.ReadAllTextAsync(filePath);
                if (string.IsNullOrWhiteSpace(json)) return new List<EventModel>();
                return JsonSerializer.Deserialize<List<EventModel>>(json) ?? new List<EventModel>();
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", $"Failed to load event models: {ex.Message}", "OK");
                return new List<EventModel>();
            }
        }

        public static async Task EnsureAllStandardUserBookingFilesAsync()
        {
            var users = await LoadUsersAsync();
            foreach (var user in users.Where(u => u.Status == "standard"))
            {
                string filePath = Path.Combine(JsonDirectory, $"S-{user.Username}.txt");
                if (!File.Exists(filePath))
                    await File.WriteAllTextAsync(filePath, "[]");
            }
        }

        public static async Task<ObservableCollection<Booking>> LoadBookingsAsync(string username)
        {
            string filePath = Path.Combine(JsonDirectory, $"S-{username}.txt");
            if (!File.Exists(filePath))
            {
                await File.WriteAllTextAsync(filePath, "[]");
                return new ObservableCollection<Booking>();
            }

            string json = await File.ReadAllTextAsync(filePath);
            if (string.IsNullOrWhiteSpace(json))
                return new ObservableCollection<Booking>();

            try
            {
                var list = JsonSerializer.Deserialize<List<Booking>>(json);
                return new ObservableCollection<Booking>(list ?? new List<Booking>());
            }
            catch (JsonException ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", $"Failed to load bookings: {ex.Message}", "OK");
                return new ObservableCollection<Booking>();
            }
        }

        public static async Task SaveBookingAsync(string username, Booking booking)
        {
            string filePath = Path.Combine(JsonDirectory, $"S-{username}.txt");
            List<Booking> bookings = new();

            if (File.Exists(filePath))
            {
                string json = await File.ReadAllTextAsync(filePath);
                if (!string.IsNullOrWhiteSpace(json))
                    bookings = JsonSerializer.Deserialize<List<Booking>>(json) ?? new();
            }

            bookings.Add(booking);
            var serialized = JsonSerializer.Serialize(bookings, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, serialized);

            var users = await LoadUsersAsync();
            var user = users.FirstOrDefault(u => u.Username == username);
            if (user != null)
                AddNotificationById(user.Id, "Booking Pending", $"Your booking for '{booking.Headline}' has been in Pending.");
        }

        public static async Task UpdateBookingStatusAsync(string username, string bookingId, string newStatus)
        {
            var bookings = await LoadBookingsAsync(username);
            var target = bookings?.FirstOrDefault(b => b.BookingId == bookingId);

            if (target != null)
            {
                target.Status = newStatus;
                string filePath = Path.Combine(JsonDirectory, $"S-{username}.txt");
                var json = JsonSerializer.Serialize(bookings, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(filePath, json);

                var users = await LoadUsersAsync();
                var user = users.FirstOrDefault(u => u.Username == username);
                if (user != null)
                    AddNotificationById(user.Id, "Booking Status Updated", $"Your booking '{target.Headline}' is now marked as '{newStatus}'.");
            }
        }

        public static async Task<int> FindOwnerIdByEventTitleAsync(string headline)
        {
            var files = Directory.GetFiles(JsonDirectory, "EO-*.txt");
            foreach (var file in files)
            {
                try
                {
                    var events = await LoadEventsAsync(file);
                    var match = events.FirstOrDefault(e => e.Title == headline);
                    if (match != null)
                    {
                        string fileName = Path.GetFileNameWithoutExtension(file);
                        if (fileName.StartsWith("EO-") && int.TryParse(fileName.Split('-')[1], out int ownerId))
                            return ownerId;
                    }
                }
                catch { }
            }
            return -1;
        }

        public static async Task SaveAllBookingsAsync(string username, ObservableCollection<Booking> updatedBookings)
        {
            string filePath = Path.Combine(JsonDirectory, $"S-{username}.txt");
            string json = JsonSerializer.Serialize(updatedBookings, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json);
        }

        public static async Task<List<string>> SaveOwnerImagesAsync(int ownerId, ObservableCollection<ImageSource> images)
        {
            var savedPaths = new List<string>();
            string baseFolder = Path.Combine(JsonDirectory, "Event Space Images", ownerId.ToString());

            if (!Directory.Exists(baseFolder))
                Directory.CreateDirectory(baseFolder);

            int index = 1;
            foreach (var image in images)
            {
                string newFileName = $"img_{index++}.png";
                string fullPath = Path.Combine(baseFolder, newFileName);
                string relativePath = Path.Combine("Event Space Images", ownerId.ToString(), newFileName);

                if (image is StreamImageSource streamSource)
                {
                    var stream = await streamSource.Stream(CancellationToken.None);
                    using var fileStream = File.Create(fullPath);
                    await stream.CopyToAsync(fileStream);
                }

                savedPaths.Add(relativePath.Replace("\\", "/"));
            }

            return savedPaths;
        }

        public static async Task<bool> UpdateUserStatusAsync(string emailOrUsername, string newStatus)
        {
            var users = await LoadUsersAsync();
            var user = users.FirstOrDefault(u => u.Email.Equals(emailOrUsername, StringComparison.OrdinalIgnoreCase)
                                              || u.Username.Equals(emailOrUsername, StringComparison.OrdinalIgnoreCase));

            if (user != null)
            {
                user.Status = newStatus;
                var updatedJson = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(UsersFilePath, updatedJson);
                return true;
            }
            return false;
        }

        public static async Task<string> SaveUserAvatarAsync(int userId, FileResult file)
        {
            if (file == null) return null;
            try
            {
                string userFolder = Path.Combine(JsonDirectory, "User Images", userId.ToString());
                Directory.CreateDirectory(userFolder);

                int nextIndex = Directory.GetFiles(userFolder, "avatar_*.png").Length + 1;
                string fileName = $"avatar_{nextIndex}.png";
                string targetPath = Path.Combine(userFolder, fileName);

                using var sourceStream = await file.OpenReadAsync();
                using var destStream = File.Create(targetPath);
                await sourceStream.CopyToAsync(destStream);
                AddNotificationById(userId, "Profile Picture Changed", "You have successfully updated your profile picture.");
                return targetPath;
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to save avatar: {ex.Message}", "OK");
                return null;
            }
        }

        public static void AddNotificationById(int userId, string title, string message)
        {
            try
            {
                string notifDir = Path.Combine(JsonDirectory, "UserNotifs", userId.ToString());
                if (!Directory.Exists(notifDir))
                    Directory.CreateDirectory(notifDir);

                string notifFile = Path.Combine(notifDir, "notifications.txt");
                ObservableCollection<NotificationModel> notifications = new();

                if (File.Exists(notifFile))
                {
                    var content = File.ReadAllText(notifFile);
                    notifications = JsonSerializer.Deserialize<ObservableCollection<NotificationModel>>(content) ?? new();
                }

                notifications.Insert(0, new NotificationModel
                {
                    Title = title,
                    Message = message,
                    Timestamp = DateTime.Now.ToString("MMM dd, yyyy h:mm tt")
                });

                var updatedContent = JsonSerializer.Serialize(notifications, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(notifFile, updatedContent);
            }
            catch (Exception ex)
            {
                App.Current.MainPage?.DisplayAlert("Error", $"Failed to add notification: {ex.Message}", "OK");
            }
        }

        public static string ResolveImagePath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return "event_placeholder.png";

            string fullPath = Path.Combine(JsonDirectory, relativePath);
            return File.Exists(fullPath) ? fullPath : "event_placeholder.png";
        }
    }
}
