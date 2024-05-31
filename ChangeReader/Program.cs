using BiQ.AlertIntegrationDemo.DtoAlertChanges;
using System.Net.Http.Headers;

namespace BiQ.AlertIntegrationDemo.ChangeReader
{
    internal class Program
    {
        private const string bookmarkFileName = "changereaderstorage.json";

        static async Task Main()
        {
            Console.WriteLine("Starting Change Reader");

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
                    // Get next changes
                    var bookmark = ReadLocalBookmark();
                    HttpClient client = CreateAlertChangesHttpClient();
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

                        HandleOneChange(change);
                        bookmark = change.Committed.Value;
                        SaveLocalBookmark(bookmark);
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

        
        private static HttpClient CreateAlertChangesHttpClient()
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

        private static void HandleOneChange(ApprovedChange ac)
        {
            // Handle create new
            if (ac.EventType!.Equals("new-person")|| ac.EventType!.Equals("new-company"))
            {
                DtoTenant.Customer newCustomer = new()
                {
                    CustomerNumber = Guid.NewGuid().ToString(),
                    CreatedBy = ac.ApprovedBy,
                    ChangedBy = ac.ApprovedBy,
                    LatestChangeIdFromBiQ = ac.Id.ToString()
                };
                if (ac.EventType!.Equals("new-company"))
                {
                    var createCompanyPayload =
                         JsonConverter.DeserializeContainerAs<CreateCompanyPayload>(ac.Payload);
                    newCustomer.Active = createCompanyPayload?.Active;
                    newCustomer.CompositeAddress = createCompanyPayload?.Address?.CompositeOrig;
                    newCustomer.Street = createCompanyPayload?.Address?.Street;
                    newCustomer.StreetCode = createCompanyPayload?.Address?.StreetCode;
                    newCustomer.HouseNumber = createCompanyPayload?.Address?.HouseNumber;
                    newCustomer.HouseLetter = createCompanyPayload?.Address?.HouseLetter;
                    newCustomer.Zip = createCompanyPayload?.Address?.PostalCode;
                    newCustomer.City = createCompanyPayload?.Address?.PostalCity;
                    newCustomer.SubCity = createCompanyPayload?.Address?.PostalSubCity;
                    newCustomer.MunicipalityCode = createCompanyPayload?.Address?.MunicipalityCode;
                    newCustomer.Country = createCompanyPayload?.Address?.CountryName;
                    newCustomer.CountryCode = createCompanyPayload?.Address?.CountryCode;
                    newCustomer.AddressDarId = createCompanyPayload?.Address?.DarAddressId.ToString();
                    newCustomer.Floor = createCompanyPayload?.Address?.Floor;
                    newCustomer.Suite = createCompanyPayload?.Address?.Suite;
                    newCustomer.CO = createCompanyPayload?.CareOf;
                    newCustomer.Attention = createCompanyPayload?.Attention;
                    newCustomer.Cvr = createCompanyPayload?.Cvr;
                    newCustomer.PNumber = createCompanyPayload?.Pnr;
                    newCustomer.Email = createCompanyPayload?.Email;
                    newCustomer.Name = createCompanyPayload?.Name;
                    newCustomer.Phone1 = createCompanyPayload?.Phone1;
                    newCustomer.Phone2 = createCompanyPayload?.Phone2;
                    newCustomer.PoBox = createCompanyPayload?.PoBox;
                    var extraDataDictComp = createCompanyPayload?.ExtraData;
                    if (extraDataDictComp is not null)
                    {
                        if (extraDataDictComp.ContainsKey(ConfigValues.extraData1FieldName))
                        {
                            var extraData1FieldValue = 
                                (string)extraDataDictComp[ConfigValues.extraData1FieldName];
                            newCustomer.ExtraData1Value = extraData1FieldValue;
                        }
                        if (extraDataDictComp.ContainsKey(ConfigValues.extraData2FieldName))
                        {
                            var extraData2FieldValue = 
                                (string)extraDataDictComp[ConfigValues.extraData2FieldName];
                            newCustomer.ExtraData2Value = extraData2FieldValue;
                        }
                    }
                }
                else 
                {
                    var createPersonPayload =
                        JsonConverter.DeserializeContainerAs<CreatePersonPayload>(ac.Payload);
                    newCustomer.Active = createPersonPayload?.Active;
                    newCustomer.CompositeAddress = createPersonPayload?.Address?.CompositeOrig;
                    newCustomer.Street = createPersonPayload?.Address?.Street;
                    newCustomer.StreetCode = createPersonPayload?.Address?.StreetCode;
                    newCustomer.HouseNumber = createPersonPayload?.Address?.HouseNumber;
                    newCustomer.HouseLetter = createPersonPayload?.Address?.HouseLetter;
                    newCustomer.Zip = createPersonPayload?.Address?.PostalCode;
                    newCustomer.City = createPersonPayload?.Address?.PostalCity;
                    newCustomer.SubCity = createPersonPayload?.Address?.PostalSubCity;
                    newCustomer.MunicipalityCode = createPersonPayload?.Address?.MunicipalityCode;
                    newCustomer.Country = createPersonPayload?.Address?.CountryName;
                    newCustomer.CountryCode = createPersonPayload?.Address?.CountryCode;
                    newCustomer.AddressDarId = createPersonPayload?.Address?.DarAddressId.ToString();
                    newCustomer.Floor = createPersonPayload?.Address?.Floor;
                    newCustomer.Suite = createPersonPayload?.Address?.Suite;
                    newCustomer.BirthDay = createPersonPayload?.BirthDate.ToString();
                    newCustomer.CO = createPersonPayload?.CareOf;
                    newCustomer.Attention = createPersonPayload?.Attention;
                    newCustomer.Cpr = createPersonPayload?.Cpr;
                    newCustomer.Email = createPersonPayload?.Email;
                    newCustomer.Name = createPersonPayload?.Name;
                    newCustomer.Phone1 = createPersonPayload?.Phone1;
                    newCustomer.Phone2 = createPersonPayload?.Phone2;
                    newCustomer.PoBox = createPersonPayload?.PoBox;
                    newCustomer.SecondaryCpr = createPersonPayload?.SecondaryContactCprNumber;
                    newCustomer.SecondaryEmail = createPersonPayload?.SecondaryContactEmail;
                    newCustomer.SecondaryName = createPersonPayload?.SecondaryContactName;
                    newCustomer.SecondaryPhone = createPersonPayload?.SecondaryContactPhoneNumber1;
                    newCustomer.SecondaryPhone2 = createPersonPayload?.SecondaryContactPhoneNumber2;
                    var extraDataDictPers = createPersonPayload?.ExtraData;
                    if (extraDataDictPers is not null)
                    {
                        if (extraDataDictPers.ContainsKey(ConfigValues.extraData1FieldName))
                        {
                            var extraData1FieldValue = (string)extraDataDictPers[ConfigValues.extraData1FieldName];
                            newCustomer.ExtraData1Value = extraData1FieldValue;
                        }
                        if (extraDataDictPers.ContainsKey(ConfigValues.extraData2FieldName))
                        {
                            var extraData2FieldValue = (string)extraDataDictPers[ConfigValues.extraData2FieldName];
                            newCustomer.ExtraData2Value = extraData2FieldValue;
                        }
                    }
                }
                CustomerSystem.Db.InsertCustomer(newCustomer);
                return;
            }
            
            // Handle updates
            var dbCustomer = CustomerSystem.Db.GetCustomer(ac.CustomerNumber);
            if(dbCustomer is null || string.IsNullOrWhiteSpace(dbCustomer.CustomerNumber))
            {
                Console.WriteLine($"Received a change for a deleted customer! skipping.");
                return;
            }
                
            DtoTenant.Customer c = dbCustomer!;
            c.ChangedBy = ac.ApprovedBy;
            c.LatestChangeIdFromBiQ = ac.Id.ToString();

            switch (ac.EventType)
            {
                case "name-change":
                case "name-change-at-subscription-start":
                case "name-correction":
                case "name-correction-at-subscription-start":
                    var nameChangePayload =
                        JsonConverter.DeserializeContainerAs<NameChangePayload>(ac.Payload);
                    c.Name = nameChangePayload?.ProposedName;
                    break;

                case "address-change":
                case "address-change-at-subscription-start":
                case "address-correction":
                case "address-correction-at-subscription-start":
                case "address-correction-before-subscription-start":
                    var addressChangePayload = 
                        JsonConverter.DeserializeContainerAs<AddressChangePayload>(ac.Payload);
                    c.CompositeAddress = addressChangePayload?.ProposedAddress?.CompositeOrig;
                    c.Street = addressChangePayload?.ProposedAddress?.Street;
                    c.StreetCode = addressChangePayload?.ProposedAddress?.StreetCode;
                    c.HouseNumber = addressChangePayload?.ProposedAddress?.HouseNumber;
                    c.HouseLetter = addressChangePayload?.ProposedAddress?.HouseLetter;
                    c.Floor = addressChangePayload?.ProposedAddress?.Floor;
                    c.Suite = addressChangePayload?.ProposedAddress?.Suite;
                    c.City = addressChangePayload?.ProposedAddress?.PostalCity;
                    c.Zip = addressChangePayload?.ProposedAddress?.PostalCode;
                    c.SubCity = addressChangePayload?.ProposedAddress?.PostalSubCity;
                    c.AddressDarId = addressChangePayload?.ProposedAddress?.DarAddressId?.ToString();
                    c.Country = addressChangePayload?.ProposedAddress?.CountryName;
                    c.CountryCode = addressChangePayload?.ProposedAddress?.CountryCode;
                    c.MunicipalityCode = addressChangePayload?.ProposedAddress?.MunicipalityCode;
                    break;

                case "name-and-address-change":
                case "name-and-address-change-at-subscription-start":
                case "name-and-address-correction":
                case "name-and-address-correction-at-subscription-start":
                    var nameAndAddressChangePayload = 
                        JsonConverter.DeserializeContainerAs<NameAndAddressChangePayload>(ac.Payload);
                    c.Name = nameAndAddressChangePayload?.ProposedName;
                    c.CompositeAddress = nameAndAddressChangePayload?.ProposedAddress?.CompositeOrig;
                    c.Street = nameAndAddressChangePayload?.ProposedAddress?.Street;
                    c.StreetCode = nameAndAddressChangePayload?.ProposedAddress?.StreetCode;
                    c.HouseNumber = nameAndAddressChangePayload?.ProposedAddress?.HouseNumber;
                    c.HouseLetter = nameAndAddressChangePayload?.ProposedAddress?.HouseLetter;
                    c.Floor = nameAndAddressChangePayload?.ProposedAddress?.Floor;
                    c.Suite = nameAndAddressChangePayload?.ProposedAddress?.Suite;
                    c.City = nameAndAddressChangePayload?.ProposedAddress?.PostalCity;
                    c.Zip = nameAndAddressChangePayload?.ProposedAddress?.PostalCode;
                    c.SubCity = nameAndAddressChangePayload?.ProposedAddress?.PostalSubCity;
                    c.AddressDarId = nameAndAddressChangePayload?.ProposedAddress?.DarAddressId?.ToString();
                    c.Country = nameAndAddressChangePayload?.ProposedAddress?.CountryName;
                    c.CountryCode = nameAndAddressChangePayload?.ProposedAddress?.CountryCode;
                    c.MunicipalityCode = nameAndAddressChangePayload?.ProposedAddress?.MunicipalityCode;
                    break;

                case "care-of-change":
                case "care-of-change-at-subscription-start":
                    var careOfChangePayload =
                        JsonConverter.DeserializeContainerAs<CareOfChangePayload>(ac.Payload);
                    c.CO = careOfChangePayload?.ProposedCareOf;
                    break;

                case "attention-change":
                case "attention-at-subscription-start":
                    var attentionChangePayload =
                        JsonConverter.DeserializeContainerAs<AttentionChangePayload>(ac.Payload);
                    c.Attention = attentionChangePayload?.ProposedAttention;
                    break;

                case "cpr-number-change":
                    var cprNumberChangePayload = 
                        JsonConverter.DeserializeContainerAs<CprNumberChangePayload>(ac.Payload);
                    c.Cpr = cprNumberChangePayload?.ProposedCprNumber;
                    break;

                case "cvr-number-change":
                    var cvrNumberChangePayload = 
                        JsonConverter.DeserializeContainerAs<CvrNumberChangePayload>(ac.Payload);
                    c.Cvr = cvrNumberChangePayload?.ProposedCvrNumber;
                    break;

                case "p-number-change":
                    var pNumberChangePayload = 
                        JsonConverter.DeserializeContainerAs<PNumberChangePayload>(ac.Payload);
                    c.CO = pNumberChangePayload?.ProposedPNumber;
                    break;

                case "email-change":
                case "email-change-at-subscription-start":
                    var emailChangePayload = 
                        JsonConverter.DeserializeContainerAs<EmailChangePayload>(ac.Payload);
                    c.CO = emailChangePayload?.ProposedEmail;
                    break;

                case "phone-change":
                case "phone-change-at-subscription-start":
                    var phoneNumberChangePayload = 
                        JsonConverter.DeserializeContainerAs<PhoneNumberChangePayload>(ac.Payload);
                    c.CO = phoneNumberChangePayload?.ProposedPhoneNumber;
                    break;

                case "phone-2-change":
                case "phone-2-change-at-subscription-start":
                    var phoneNumber2ChangePayload = 
                        JsonConverter.DeserializeContainerAs<PhoneNumberChangePayload>(ac.Payload);
                    c.CO = phoneNumber2ChangePayload?.ProposedPhoneNumber;
                    break;

                case "new-person":
                case "new-company":
                    // Handled elsewhere
                    return;

                case "secondary-contact-name-change":
                    var secNameChangePayload = 
                        JsonConverter.DeserializeContainerAs<NameChangePayload>(ac.Payload);
                    c.SecondaryName = secNameChangePayload?.ProposedName;
                    break;

                case "secondary-contact-email-change":
                    var secEmailChangePayload = 
                        JsonConverter.DeserializeContainerAs<EmailChangePayload>(ac.Payload);
                    c.SecondaryEmail = secEmailChangePayload?.ProposedEmail;
                    break;

                case "secondary-contact-cpr-number-change":
                    var secCprChangePayload = 
                        JsonConverter.DeserializeContainerAs<CprNumberChangePayload>(ac.Payload);
                    c.SecondaryCpr = secCprChangePayload?.ProposedCprNumber;
                    break;

                case "secondary-contact-phone-change":
                    var secPhoneChangePayload = 
                        JsonConverter.DeserializeContainerAs<PhoneNumberChangePayload>(ac.Payload);
                    c.SecondaryPhone = secPhoneChangePayload?.ProposedPhoneNumber;
                    break;

                case "secondary-contact-phone-2-change":
                    var secPhone2ChangePayload = 
                        JsonConverter.DeserializeContainerAs<PhoneNumberChangePayload>(ac.Payload);
                    c.SecondaryPhone2 = secPhone2ChangePayload?.ProposedPhoneNumber;
                    break;

                case "po-box-change":
                    var poBoxChangePayload = 
                        JsonConverter.DeserializeContainerAs<PoBoxChangePayload>(ac.Payload);
                    c.PoBox = poBoxChangePayload?.ProposedPoBox;
                    break;

                case "extra-data-change":
                    var extraDataChangePayload = 
                        JsonConverter.DeserializeContainerAs<ExtraDataChangePayload>(ac.Payload);
                    if (extraDataChangePayload?.ProposedExtraData?.ContainsKey(ConfigValues.extraData1FieldName) ?? false)
                    {
                        c.ExtraData1Value = extraDataChangePayload.ProposedExtraData[ConfigValues.extraData1FieldName].ToString();
                    }
                    if (extraDataChangePayload?.ProposedExtraData?.ContainsKey(ConfigValues.extraData2FieldName) ?? false)
                    {
                        c.ExtraData2Value = extraDataChangePayload.ProposedExtraData[ConfigValues.extraData2FieldName].ToString();
                    }
                    break;

                default:
                    // Ignoring unneeded eventTypes.
                    return;
            }
            CustomerSystem.Db.UpdateCustomer(c);
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