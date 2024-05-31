using System;

namespace BiQ.AlertIntegrationDemo.DtoTenant
{
    //A very simple example class to contain your own customer data in your own system.
    //Note a field to store BiQ's AssociatedApprovedChangeId is added to your own fields.
    public class Customer
    {
        public string? CustomerCategoryText { get; set; }
        public string? LatestChangeIdFromBiQ { get; set; }
        public string? Name { get; set; }
        public string? CustomerNumber { get; set; }
        public string? CO { get; set; }
        public string? Attention { get; set; }
        public string? Cpr { get; set; }
        public string? BirthDay { get; set; }
        public string? CompositeAddress { get; set; }
        public string? Cvr { get; set; }
        public string? PNumber { get; set; }
        public string? Phone1 { get; set; }
        public string? Phone2 { get; set; }
        public string? Email { get; set; }
        public string? Street { get; set; }
        public string? StreetCode { get; set; }
        public string? HouseNumber { get; set; }
        public string? HouseLetter { get; set; }
        public string? Floor { get; set; }
        public string? Suite { get; set; }
        public string? City { get; set; }
        public string? Zip { get; set; }
        public string? SubCity { get; set; }
        public string? AddressDarId { get; set; }
        public string? Country { get; set; }
        public string? CountryCode { get; set; }
        public string? MunicipalityCode { get; set; }
        public string? SecondaryName { get; set; }
        public string? SecondaryCpr { get; set; }
        public string? SecondaryPhone { get; set; }
        public string? SecondaryPhone2 { get; set; }
        public string? SecondaryEmail { get; set; }
        public string? PoBox { get; set; }
        public string? ChangedBy { get; set; }
        public DateTimeOffset ChangedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public static string ExtraData1Name => ConfigValues.extraData1FieldName;
        public string? ExtraData1Value { get; set; } 
        public static string ExtraData2Name => ConfigValues.extraData2FieldName;
        public string? ExtraData2Value { get; set; }
        public string? MasterSystemDeepLink { get; set; }
        public string? Active { get; set; }
    }
}
