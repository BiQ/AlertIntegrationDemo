using BiQ.AlertIntegrationDemo;
using BiQ.AlertIntegrationDemo.DtoAlertChanges;
using ChangeReader;
using System.Net.Http.Headers;

namespace ChangeCallbackReceiver
{
    public class Program
    {
        private static DateTimeOffset? Bookmark = null;
        private static bool IsFetching = false;

        public static void Main(string[] args)
        {
            ReadingHelpers.InitBookmark();

            var builder = WebApplication.CreateSlimBuilder(args);

            var app = builder.Build();

            app.MapGet("/calback-receiver/20E54A22-E46F-4399-B65E-6EFAB2DE51DB", () => // A URL bots and scrapers won't find to easily
            {
                if (IsFetching)
                {
                    // got a callback while getting next changes - we are god, just let the old thread complete the fetching!
                }
                else
                {
                    new Thread(async () => await KeepGettingNextChangesUntilThereAreNoMore()).Start();
                }

                return Results.Ok();
            });

            app.Run();
        }

        private static async Task KeepGettingNextChangesUntilThereAreNoMore()
        {
            IsFetching = true;
            bool thereIsMore = true;

            while (thereIsMore)
                thereIsMore = await NextChanges();

            IsFetching = false;
        }

        private static async Task<bool> NextChanges()
        {
            Bookmark = ReadingHelpers.ReadBookmark();

            // Get next changes
            HttpClient client = ReadingHelpers.CreateAlertChangesHttpClient();
            string url = $"/tenants/{ConfigValues.TenantId}/shadowsources/{ConfigValues.ShadowSourceId}/next-approved-changes/";
            var jsonContent = JsonConverter.Serialize(new { from = Bookmark });
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
                ReadingHelpers.HandleOneChange(change);

                if (change.Committed is null)
                    throw new Exception("Committed is NULL in approved change. This should never happen. Contact BiQ.");

                ReadingHelpers.SaveBookmark(change.Committed.Value);
            }
            return nextApprovedChanges.More;
        }
    }
}
