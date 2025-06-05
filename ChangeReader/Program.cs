using BiQ.AlertIntegrationDemo.DtoAlertChanges;
using ChangeReader;
using System.Net.Http.Headers;

namespace BiQ.AlertIntegrationDemo.ChangeReader
{
    internal class Program
    {
        

        static async Task Main()
        {
            Console.WriteLine("Starting Change Reader");

            ReadingHelpers.InitBookmark();

            while (true)
            {
                try
                {
                    // Get next changes
                    var bookmark = ReadingHelpers.ReadBookmark();
                    HttpClient client = ReadingHelpers.CreateAlertChangesHttpClient();
                    string url = $"/tenants/{ConfigValues.TenantId}/shadowsources/{ConfigValues.ShadowSourceId}/next-approved-changes/";
                    var jsonContent = JsonConverter.Serialize(new { from = bookmark });
                    using var content = new StringContent(jsonContent);
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    var response = await client.PostAsync(url, content);
                    response.EnsureSuccessStatusCode();

                    // Deserialize next changes
                    var nextApprovedChanges = JsonConverter.DeserializeAs<NextApprovedChanges>(
                        await response.Content.ReadAsStringAsync());

                    if (nextApprovedChanges is null || nextApprovedChanges.ApprovedChanges is null)
                        throw new Exception("ApprovedChanges is NULL. This should never happen. Contact BiQ.");

                    // We now got the next changes - handle them...
                    foreach (var change in nextApprovedChanges.ApprovedChanges)
                    {
                        if (change.Committed is null)
                            throw new Exception("Committed is NULL in approved change. This should never happen. Contact BiQ.");

                        ReadingHelpers.HandleOneChange(change);
                        bookmark = change.Committed.Value;
                        ReadingHelpers.SaveBookmark(bookmark);
                    }

                    Console.WriteLine($"Read {nextApprovedChanges.ApprovedChanges.Count()} changes. New bookmark: {bookmark}");

                    if (!nextApprovedChanges.More)
                        Task.Delay(TimeSpan.FromMinutes(1)).Wait();// Currently not more - wait a bit
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An exception occurred! ({ex.Message}) - Will try again in 2 minutes.");
                    Task.Delay(TimeSpan.FromMinutes(2)).Wait();
                }
            }
        }
    }
}