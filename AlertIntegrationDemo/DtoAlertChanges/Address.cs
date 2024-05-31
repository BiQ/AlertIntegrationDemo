using System;

namespace BiQ.AlertIntegrationDemo.DtoAlertChanges
{
    public class Address
    {
        public string? CompositeOrig { get; set; }
        public string? Street { get; set; }
        public string? StreetCode { get; set; }
        public string? HouseNumber { get; set; }
        public string? HouseLetter { get; set; }
        public string? Floor { get; set; }
        public string? Suite { get; set; }
        public string? PostalCode { get; set; }
        public string? PostalCity { get; set; }
        public string? PostalSubCity { get; set; }
        public string? MunicipalityCode { get; set; }
        public string? CountryCode { get; set; }
        public string? CountryName { get; set; }
        public Guid? DarAddressId { get; set; }
    }
}
