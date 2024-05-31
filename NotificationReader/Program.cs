using BiQ.AlertIntegrationDemo.DtoAlertChanges;
using System.Net.Http.Headers;

namespace BiQ.AlertIntegrationDemo.NotificationReader
{
    internal class Program
    {
        private const String bookmarkFileName = "notificationreaderstorage.json";

        static async Task Main()
        {
            Console.WriteLine("Starting Notification Reader");

            // Test local bookmark storage
            if (!File.Exists(bookmarkFileName))
            {
                // Create a new default bookmark file
                SaveLocalBookmark(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));
                Console.WriteLine("Local storage file for bookmark not found" +
                    $" A new default file ({bookmarkFileName}) has been created.");
            }

            while (true)
            {
                try
                {
                    // Get next notifications
                    var bookmark = ReadLocalBookmark();
                    HttpClient client = CreateNotificationHttpClient();
                    string url = $"/tenants/{ConfigValues.TenantId}/shadowsources/{ConfigValues.ShadowSourceId}/next-notifications/";
                    var body = new { from = bookmark };
                    var jsonContent = JsonConverter.Serialize(body);
                    using var content = new StringContent(jsonContent);
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    var response = await client.PostAsync(url, content);
                    response.EnsureSuccessStatusCode();

                    // Deserialize next notifications
                    var nextNotifications = JsonConverter.DeserializeAs<NextNotifications>(
                        await response.Content.ReadAsStringAsync());

                    if (nextNotifications is null || nextNotifications.Notifications is null)
                        throw new Exception("Notifications is NULL. This should never happen. Contact BiQ.");

                    // We now got the next notifications - handle them...
                    foreach (var notif in nextNotifications.Notifications)
                    {
                        if (notif.Committed is null)
                            throw new Exception("Committed is NULL in notification. This should never happen. Contact BiQ.");

                        HandleOneNotification(notif);
                        bookmark = notif.Committed.Value;
                        SaveLocalBookmark(bookmark);
                    }

                    Console.WriteLine($"Got {nextNotifications.Notifications.Count()} notifications. New bookmark: {bookmark}");

                    if (!nextNotifications.More)
                        Task.Delay(TimeSpan.FromMinutes(1)).Wait();// Currently not more - wait a minute

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An exception occurred! ({ex.Message}) - Will try again in 2 minutes.");
                    Task.Delay(TimeSpan.FromMinutes(2)).Wait();
                }
            }
        }

        private static void HandleOneNotification(Notification notif)
        {
            switch (notif.EventType)
            {
                case "person-deceased":
                    Console.WriteLine($"Customer ({notif.CustomerNumber}) has just died!");
                    break;

                case "person-formerly-deceased":
                    Console.WriteLine($"Customer ({notif.CustomerNumber}) is dead!");
                    break;

                default:
                    // Ignoring unneeded eventTypes.
                    break;
            }
        }

        
        private static HttpClient CreateNotificationHttpClient()
        {
            var client = new HttpClient(
            new BiqAuthenticationHandler(
                new Uri(ConfigValues.AuthorizationBase!),
                ConfigValues.ApiKey.ToString(),
            new HttpClientHandler()))
            {
                BaseAddress = new Uri(ConfigValues.AlertChangesBase)
            };
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            return client;
        }

        private static DateTimeOffset ReadLocalBookmark()
        {
            string storageJsonString = File.ReadAllText(bookmarkFileName);
            return JsonConverter.DeserializeDateTimeOffset(storageJsonString);
        }

        private static void SaveLocalBookmark(DateTimeOffset timestamp)
        {
            string jsonString = JsonConverter.Serialize(timestamp);
            File.WriteAllText(bookmarkFileName, jsonString);
        }
    }
}
