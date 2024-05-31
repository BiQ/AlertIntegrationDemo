using BiQ.AlertIntegrationDemo.DtoAlertChanges;
using System.Net.Http.Headers;
using System.Web;

namespace BiQ.AlertIntegrationDemo.ShadowWriter
{
    internal class Program
    {
        private const string bookmarkFileName = "shadowwriterstorage.json";

        static async Task Main()
        {
            Console.WriteLine("Starting Shadow maintainer");
            bool initialRun = false;

            // Test local bookmark storage
            if(!File.Exists(bookmarkFileName))            
            {
                // Create a new default bookmark file
                SaveLocalBookmark(new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero));
                Console.WriteLine("Local storage file for bookmark not found" +
                    $" A new default file ({bookmarkFileName}) has been created.");
                initialRun = true;
            }

            while (true)
            {
                try
                {
                    // Get changes from customer system
                    var bookmark = ReadLocalBookmark();
                    var customerNumbers =
                        CustomerSystem.Db.GetCustomerNumbersFrom(bookmark);
                    foreach (var customerNumber in customerNumbers)
                    {
                        var dbCustomer = CustomerSystem.Db.GetCustomer(customerNumber);
                        if (dbCustomer is null) 
                            throw new Exception($"Customer deleted midt run! Starting over.");
                        DtoShadow.Customer shadowCustomer = MapToShadow(dbCustomer);

                        HttpClient client = CreateShadowHttpClient();

                        var jsonContent = JsonConverter.Serialize(shadowCustomer);

                        using var content = new StringContent(jsonContent);
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        HttpResponseMessage? response = null;
                        if (initialRun || dbCustomer.ChangedAt.Equals(dbCustomer.CreatedAt ?? DateTimeOffset.MinValue))
                        {
                            // It is a new customer
                            string url = $"/tenants/{ConfigValues.TenantId}/shadowsources/{ConfigValues.ShadowSourceId}/customers";
                            response = await client.PostAsync(url, content);
                        }
                        else
                        {
                            // It is a modified customer
                            string url = $"/tenants/{ConfigValues.TenantId}/shadowsources/{ConfigValues.ShadowSourceId}/customers/{HttpUtility.UrlEncode(customerNumber)}";
                            response = await client.PutAsync(url, content);
                        }
                        response.EnsureSuccessStatusCode();

                        bookmark = dbCustomer.ChangedAt;
                        SaveLocalBookmark(bookmark);
                    }

                    Console.WriteLine($"Got {customerNumbers.Count} modified customers. New timestamp: {bookmark}");
                    if (!customerNumbers.Any())
                        Task.Delay(TimeSpan.FromMinutes(1)).Wait();// Currently not more - wait a minute
                    initialRun = false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An exception occurred! ({ex.Message}) - Will try again in 2 minutes.");
                    Task.Delay(TimeSpan.FromMinutes(2)).Wait();
                }
            }
        }

        private static DtoShadow.Customer MapToShadow(DtoTenant.Customer dbCustomer)
        {
            if (dbCustomer is null)
                throw new ArgumentNullException(nameof(dbCustomer));

            DtoShadow.Customer customer = new()
            {
                CustomerNumber = dbCustomer.CustomerNumber,
                CreatedBy = dbCustomer.CreatedBy,
                Created = dbCustomer.CreatedAt.ToString(),
                ChangedBy = dbCustomer.ChangedBy,
                Changed = dbCustomer.ChangedAt.ToString(),
                CustomerCategoryText = MapToCustomerCategory(dbCustomer.CustomerCategoryText),
                Active = dbCustomer.Active,
                AssociatedApprovedChange = dbCustomer.LatestChangeIdFromBiQ,
                Contact1 = new()
                {
                    Name = dbCustomer.Name,
                    Phone1 = dbCustomer.Phone1,
                    Phone2 = dbCustomer.Phone2,
                    Email = dbCustomer.Email,
                    DateOfBirth = dbCustomer.BirthDay,
                    Cpr = dbCustomer.Cpr,
                    Cvr = dbCustomer.Cvr,
                    PNr = dbCustomer.PNumber,
                    PoBox = dbCustomer.PoBox,
                    CareOf = dbCustomer.CO,
                    Attention = dbCustomer.Attention,
                    Address = MapToShadowAddress(dbCustomer)
                },
                Contact2 = MapToShadowContact2(dbCustomer),
                ExtraData = MapToShadowExtradata(dbCustomer),
            };

            return customer;
        }

        private static string? MapToCustomerCategory(string? customerCategoryText)
        {
            if (customerCategoryText is null)
                return null;
            else if (customerCategoryText.Equals("privat")) 
                return "Residential";
            else if (customerCategoryText.Equals("erhverv")) 
                return "Enterprise";
            return customerCategoryText;
        }

        private static Dictionary<string, object>? MapToShadowExtradata(DtoTenant.Customer dbCustomer)
        {
            if (dbCustomer is null)
                return null;

            if (string.IsNullOrEmpty(dbCustomer.ExtraData1Value) &&
                string.IsNullOrEmpty(dbCustomer.ExtraData2Value))
                return null;
            else
            {
                var extradata = new Dictionary<string, object>();
                if (!string.IsNullOrEmpty(dbCustomer.ExtraData1Value))
                    extradata.Add(ConfigValues.extraData1FieldName, dbCustomer.ExtraData1Value);
                if (!string.IsNullOrEmpty(dbCustomer.ExtraData2Value))
                    extradata.Add(ConfigValues.extraData2FieldName, dbCustomer.ExtraData2Value);
                return extradata;
            }
        }

        private static DtoShadow.Contact? MapToShadowContact2(DtoTenant.Customer dbCustomer)
        {
            if (dbCustomer is null)
                return null;

            if (string.IsNullOrEmpty(dbCustomer.SecondaryName) &&
                string.IsNullOrEmpty(dbCustomer.SecondaryPhone) &&
                string.IsNullOrEmpty(dbCustomer.SecondaryPhone2) &&
                string.IsNullOrEmpty(dbCustomer.SecondaryEmail) &&
                string.IsNullOrEmpty(dbCustomer.SecondaryCpr))
                return null;
            else
                return new()
                {
                    Name = dbCustomer.SecondaryName,
                    Phone1 = dbCustomer.SecondaryPhone,
                    Phone2 = dbCustomer.SecondaryPhone2,
                    Email = dbCustomer.SecondaryEmail,
                    Cpr = dbCustomer.SecondaryCpr
                };
        }

        private static DtoShadow.Address? MapToShadowAddress(DtoTenant.Customer dbCustomer)
        {
            if (dbCustomer is null) return null;
            _ = Guid.TryParse(dbCustomer.AddressDarId, out Guid darId);
            DtoShadow.Address shadowAddress = new()
            {
                CompositeOrig = dbCustomer.CompositeAddress,
                CountryCode = dbCustomer.CountryCode,
                CountryName = dbCustomer.Country,
                DarAddressId = darId,
                Floor = dbCustomer.Floor,
                HouseLetter = dbCustomer.HouseLetter,
                HouseNumber = dbCustomer.HouseNumber,
                MunicipalityCode = dbCustomer.MunicipalityCode,
                PostalCity = dbCustomer.City,
                PostalCode = dbCustomer.Zip,
                PostalSubCity = dbCustomer.SubCity,
                Street = dbCustomer.Street,
                StreetCode = dbCustomer.StreetCode,
                Suite = dbCustomer.Suite
            };
            return shadowAddress;
        }

        private static HttpClient CreateShadowHttpClient()
        {
            var client = new HttpClient(
            new BiqAuthenticationHandler(
                new Uri(ConfigValues.AuthorizationBase!),
                ConfigValues.ApiKey.ToString(),
            new HttpClientHandler()))
            {
                BaseAddress = new Uri(ConfigValues.CustomerShadowBase)
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
