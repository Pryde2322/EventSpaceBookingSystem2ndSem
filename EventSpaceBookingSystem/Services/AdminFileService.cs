using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using EventSpaceBookingSystem.Model;
using Microsoft.Maui.Storage; // ✅ needed for FileSystem.AppDataDirectory

namespace EventSpaceBookingSystem.Services
{
    public static class AdminFileService
    {
        // ✅ Cross-platform JSON folder handling
#if ANDROID
        private static readonly string JsonDirectory = FileSystem.AppDataDirectory;
#else
        private static readonly string JsonDirectory = AppContext.BaseDirectory;
#endif

        private static readonly string EventSpaceOwnersFilePath = Path.Combine(JsonDirectory, "EventSpaceOwners.txt");

        public static async Task<List<Owner>> LoadAllOwnersAsync()
        {
            List<Owner> owners = new();

            if (!File.Exists(EventSpaceOwnersFilePath))
                return owners;

            string json = await File.ReadAllTextAsync(EventSpaceOwnersFilePath);

            if (string.IsNullOrWhiteSpace(json))
                return owners;

            try
            {
                var users = JsonSerializer.Deserialize<List<Users>>(json);

                if (users == null)
                    return owners;

                foreach (var user in users)
                {
                    string eoFile = Path.Combine(JsonDirectory, $"EO-{user.Id}.txt");

                    int spaceCount = 0;
                    if (File.Exists(eoFile))
                    {
                        var eventData = await File.ReadAllTextAsync(eoFile);
                        if (!string.IsNullOrWhiteSpace(eventData))
                        {
                            try
                            {
                                var events = JsonSerializer.Deserialize<List<EventModel>>(eventData);
                                spaceCount = events?.Count ?? 0;
                            }
                            catch
                            {
                                // fallback to 0 if corrupted
                            }
                        }
                    }

                    // 🕓 Get last modified date of EO file for "Last Active"
                    string lastModified = File.Exists(eoFile)
                        ? File.GetLastWriteTime(eoFile).ToString("MMM yyyy")
                        : "N/A";

                    owners.Add(new Owner
                    {
                        Name = user.Username,
                        Spaces = spaceCount,
                        RaMenCoStatus = user.RaMenCoStatus,
                        IsActive = string.Equals(user.RaMenCoStatus, "Activated", StringComparison.OrdinalIgnoreCase),
                        LastActive = lastModified
                    });
                }
            }
            catch (JsonException ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", $"Failed to load owners: {ex.Message}", "OK");
            }

            return owners;
        }

        public static async Task<List<Transaction>> LoadAllTransactionsFromBookingsAsync()
        {
            var transactions = new List<Transaction>();
            var bookingFiles = Directory.GetFiles(JsonDirectory, "S-*.txt");

            foreach (var file in bookingFiles)
            {
                var username = Path.GetFileNameWithoutExtension(file).Replace("S-", "");
                var bookings = await UserFileService.LoadBookingsAsync(username);

                foreach (var booking in bookings)
                {
                    if (DateTime.TryParse(booking.PublishedDate, out DateTime parsedDate))
                    {
                        int ownerId = await UserFileService.FindOwnerIdByEventTitleAsync(booking.Headline);

                        transactions.Add(new Transaction
                        {
                            ID = booking.BookingId,
                            Date = parsedDate.ToString("MMM dd, yyyy"),
                            Owner = ownerId >= 0 ? $"EO-{ownerId}" : "Unknown",
                            Space = booking.Headline,
                            Amount = $"₱{booking.Price}"
                        });
                    }
                }
            }

            // Sort by most recent date
            return transactions.OrderByDescending(t =>
            {
                DateTime.TryParseExact(t.Date, "MMM dd, yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsed);
                return parsed;
            }).ToList();
        }

        public static async Task<bool> UpdateOwnerStatusAsync(string username, string newStatus)
        {
            if (!File.Exists(EventSpaceOwnersFilePath))
                return false;

            string json = await File.ReadAllTextAsync(EventSpaceOwnersFilePath);
            var users = JsonSerializer.Deserialize<List<Users>>(json);

            var user = users?.FirstOrDefault(u => u.Username == username);
            if (user != null)
            {
                user.Status = "event space owner"; // retain actual status
                user.RaMenCoStatus = newStatus;

                string updatedJson = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(EventSpaceOwnersFilePath, updatedJson);
                return true;
            }

            return false;
        }
    }
}
