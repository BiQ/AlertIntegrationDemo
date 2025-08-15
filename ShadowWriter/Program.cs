using BiQ.AlertIntegrationDemo.DtoShadow;
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
            DateTimeOffset lastSyncValidation = DateTimeOffset.Now;

            // Test local bookmark storage
            if (!File.Exists(bookmarkFileName))
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
                    var customerNumbers = CustomerSystem.Db.GetCustomerNumbersFrom(bookmark);
                    foreach (var customerNumber in customerNumbers)
                    {
                        var dbCustomer = CustomerSystem.Db.GetCustomer(customerNumber) ??
                            throw new Exception($"Customer deleted midt run! Starting over.");
                        Customer shadowCustomer = MapToShadow(dbCustomer);

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
                    if (customerNumbers.Count == 0)
                        Task.Delay(TimeSpan.FromMinutes(1)).Wait();// Currently not more - wait a minute
                    initialRun = false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An exception occurred! ({ex.Message}) - Will try again in 2 minutes.");
                    Task.Delay(TimeSpan.FromMinutes(2)).Wait();
                }

                //This block runs once a day at 20:00 to make sure the correct customers are in the shadow
                try
                {
                    if (lastSyncValidation < DateTimeOffset.Now.AddDays(-1) && DateTimeOffset.Now.Hour > 20)
                    {
                        List<string> allLocalCustomersIds = CustomerSystem.Db.GetCustomerNumbersFrom(DateTimeOffset.MinValue);
                        List<string> allShadowCustomerIds = await GetAllShadowCustomerIds();
                        foreach (var customerMissingInShadow in allLocalCustomersIds.Except(allShadowCustomerIds))
                        {
                            Console.WriteLine($"Customer {customerMissingInShadow} is missing in shadow - will try to add it.");
                            var dbCustomer = CustomerSystem.Db.GetCustomer(customerMissingInShadow) ??
                                throw new Exception($"Customer {customerMissingInShadow} deleted mid run! Starting over.");
                            DtoShadow.Customer shadowCustomer = MapToShadow(dbCustomer);
                            HttpClient client = CreateShadowHttpClient();
                            var jsonContent = JsonConverter.Serialize(shadowCustomer);
                            using var content = new StringContent(jsonContent);
                            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                            string url = $"/tenants/{ConfigValues.TenantId}/shadowsources/{ConfigValues.ShadowSourceId}/customers";
                            HttpResponseMessage response = await client.PostAsync(url, content);
                            response.EnsureSuccessStatusCode();
                        }
                        foreach (var shadowCustomerNeedingDeletion in allShadowCustomerIds.Except(allLocalCustomersIds))
                        {
                            Console.WriteLine($"Deleted-Customer {shadowCustomerNeedingDeletion} in shadow, should be deleted.");
                            HttpClient client = CreateShadowHttpClient();
                            string url = $"/tenants/{ConfigValues.TenantId}/shadowsources/{ConfigValues.ShadowSourceId}/customers/{shadowCustomerNeedingDeletion}";
                            HttpResponseMessage response = await client.DeleteAsync(url);
                            response.EnsureSuccessStatusCode();
                        }
                        lastSyncValidation = DateTimeOffset.Now;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An exception occurred during daily sync validation! ({ex.Message}) - Will try again in 2 minutes.");
                    Task.Delay(TimeSpan.FromMinutes(2)).Wait();
                }
            }
        }

        private static async Task<List<string>> GetAllShadowCustomerIds()
        {
            List<string> customerNumbers = [];
            HttpClient client = CreateShadowHttpClient();
            int pageNumber = 0;
            int pageSize = 100;
            int totalItems = 0;
            int readCount = 0;
            bool readAgain = true;
            while (readAgain)
            {
                string url = $"/tenants/{ConfigValues.TenantId}/customers?page={pageNumber}&pageSize={pageSize}";
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var page = JsonConverter.DeserializeAs<ShadowCustomersPage>(
                    await response.Content.ReadAsStringAsync()) ??
                        throw new Exception("Unable to read shadow customers page.");
                readCount += page.Customers.Count();
                totalItems = page.TotalItems;
                foreach (var customer in page.Customers)
                {
                    if (customer.CustomerNumber is null)
                        throw new Exception("Customer number is null in shadow customer.");
                    if (customer.ShadowSourceId == ConfigValues.ShadowSourceId)
                        customerNumbers.Add(customer.CustomerNumber ?? "");
                }
                if (totalItems <= readCount)
                    readAgain = false;
                pageNumber++;
            }
            return customerNumbers;
        }

        private static DtoShadow.Customer MapToShadow(DtoTenant.Customer dbCustomer)
        {
            ArgumentNullException.ThrowIfNull(dbCustomer);

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

        private static Dictionary<string, string>? MapToShadowExtradata(DtoTenant.Customer dbCustomer)
        {
            if (dbCustomer is null)
                return null;

            if (string.IsNullOrEmpty(dbCustomer.ExtraData1Value) &&
                string.IsNullOrEmpty(dbCustomer.ExtraData2Value))
                return null;
            else
            {
                var extradata = new Dictionary<string, string>();
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
