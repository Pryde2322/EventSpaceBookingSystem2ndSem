using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EventSpaceBookingSystem.Model;
using Microsoft.Maui.Storage; // ✅ for FileSystem.AppDataDirectory
using Microsoft.Maui.Controls; // ✅ for Application.Current and ImageSource

namespace EventSpaceBookingSystem.Services
{
    public static class SpaceOwnerFileService
    {
        // ✅ Cross-platform JSON folder
#if ANDROID
        private static readonly string JsonDirectory = FileSystem.AppDataDirectory;
#else
        private static readonly string JsonDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Json"));
#endif

        private static string GetOwnerFilePath(int ownerId) =>
            Path.Combine(JsonDirectory, $"EO-{ownerId}.txt");

        public static async Task SaveToFileAsync(ObservableCollection<EventModel> eventSpaces, int ownerId)
        {
            try
            {
                if (!Directory.Exists(JsonDirectory))
                    Directory.CreateDirectory(JsonDirectory);

                var json = JsonSerializer.Serialize(eventSpaces, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(GetOwnerFilePath(ownerId), json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving EventModel data for owner {ownerId}: {ex.Message}");
            }
        }

        public static async Task LoadFromFileAsync(ObservableCollection<EventModel> targetCollection, int ownerId)
        {
            try
            {
                string filePath = GetOwnerFilePath(ownerId);

                if (!File.Exists(filePath))
                {
                    Directory.CreateDirectory(JsonDirectory);
                    await File.WriteAllTextAsync(filePath, "[]");
                }

                var json = await File.ReadAllTextAsync(filePath);
                var eventSpaces = JsonSerializer.Deserialize<ObservableCollection<EventModel>>(json)
                                 ?? new ObservableCollection<EventModel>();

                targetCollection.Clear();
                foreach (var space in eventSpaces)
                    targetCollection.Add(space);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading EventModel data for owner {ownerId}: {ex.Message}");
            }
        }

        public static async Task SaveUpdatedSpaceAsync(EventModel updatedSpace, int ownerId)
        {
            try
            {
                string filePath = GetOwnerFilePath(ownerId);

                if (!File.Exists(filePath))
                {
                    Directory.CreateDirectory(JsonDirectory);
                    await File.WriteAllTextAsync(filePath, "[]");
                }

                var json = await File.ReadAllTextAsync(filePath);
                var eventSpaces = JsonSerializer.Deserialize<List<EventModel>>(json)
                                 ?? new List<EventModel>();

                var existingSpace = eventSpaces.FirstOrDefault(e => e.Title == updatedSpace.Title);
                if (existingSpace != null)
                {
                    existingSpace.Description1 = updatedSpace.Description1;
                    existingSpace.Capacity = updatedSpace.Capacity;
                    existingSpace.DailyRate = updatedSpace.DailyRate;
                    existingSpace.ExtensionRate = updatedSpace.ExtensionRate;
                    existingSpace.TableRate = updatedSpace.TableRate;
                    existingSpace.Location = updatedSpace.Location;
                    existingSpace.Category = updatedSpace.Category;
                    existingSpace.ImageUrls = updatedSpace.ImageUrls;
                }

                var updatedJson = JsonSerializer.Serialize(eventSpaces, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(filePath, updatedJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating EventModel data for owner {ownerId}: {ex.Message}");
            }
        }

        public static async Task<List<string>> SaveOwnerImagesAsync(int ownerId, ObservableCollection<ImageSource> images)
        {
            var savedPaths = new List<string>();
            string baseFolder = Path.Combine(JsonDirectory, "Event Space Images", ownerId.ToString());

            if (!Directory.Exists(baseFolder))
                Directory.CreateDirectory(baseFolder);

            // 🔢 Get the highest existing index
            var existingFiles = Directory.GetFiles(baseFolder, "img_*.png");
            int maxIndex = existingFiles
                .Select(f => Path.GetFileNameWithoutExtension(f))
                .Select(f => int.TryParse(f.Replace("img_", ""), out int num) ? num : 0)
                .DefaultIfEmpty(0)
                .Max();

            int index = maxIndex + 1;

            foreach (var image in images)
            {
                string newFileName = $"img_{index++}.png";
                string fullPath = Path.Combine(baseFolder, newFileName);
                string relativePath = Path.Combine("Event Space Images", ownerId.ToString(), newFileName);

                try
                {
                    if (image is StreamImageSource streamSource)
                    {
                        var originalStream = await streamSource.Stream(CancellationToken.None);

                        using var ms = new MemoryStream();
                        await originalStream.CopyToAsync(ms);
                        ms.Position = 0;

                        using var fileStream = File.Create(fullPath);
                        await ms.CopyToAsync(fileStream);

                        savedPaths.Add(relativePath.Replace("\\", "/"));
                    }
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert("Image Save Error", $"Could not save image '{newFileName}': {ex.Message}", "OK");
                }
            }

            return savedPaths;
        }

        public static List<EditableImage> LoadOwnerImages(List<string> relativePaths)
        {
            var images = new List<EditableImage>();

            foreach (var relPath in relativePaths)
            {
                try
                {
                    var fullPath = Path.Combine(JsonDirectory, relPath);
                    if (File.Exists(fullPath))
                    {
                        byte[] imageBytes = File.ReadAllBytes(fullPath);
                        images.Add(new EditableImage
                        {
                            Path = relPath,
                            Source = ImageSource.FromStream(() => new MemoryStream(imageBytes)),
                            IsNew = false
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load image from {relPath}: {ex.Message}");
                }
            }

            return images;
        }

        public static void AddNotificationByOwnerId(int ownerId, string title, string message)
        {
            try
            {
                string notifDir = Path.Combine(JsonDirectory, "OwnerNotifs", ownerId.ToString());
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
                App.Current.MainPage?.DisplayAlert("Error", $"Failed to add owner notification: {ex.Message}", "OK");
            }
        }

        public static async Task<List<EventModel>> LoadEventModelsAsync(int ownerId)
        {
            string filePath = GetOwnerFilePath(ownerId);

            try
            {
                if (!File.Exists(filePath))
                    return new List<EventModel>();

                var json = await File.ReadAllTextAsync(filePath);

                if (string.IsNullOrWhiteSpace(json))
                    return new List<EventModel>();

                return JsonSerializer.Deserialize<List<EventModel>>(json) ?? new List<EventModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading EventModel data for owner {ownerId}: {ex.Message}");
                return new List<EventModel>();
            }
        }

        public static async Task<Users> GetOwnerByIdAsync(int ownerId)
        {
            var users = await UserFileService.LoadUsersAsync();
            return users.FirstOrDefault(u => u.Id == ownerId);
        }
    }
}
