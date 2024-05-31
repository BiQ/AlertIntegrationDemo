using System;

namespace BiQ.AlertIntegrationDemo.DtoAlertChanges
{
    public sealed class NameAndAddressChangePayload
    {
        public string? ProposedName { get; set; }
        public string? ExistingName { get; set; }
        public Address? ProposedAddress { get; set; }
        public Address? ExistingAddress { get; set; }
        public DateTimeOffset? NameChangeDate { get; set; }
        public DateTimeOffset? AddressChangeDate { get; set; }

    }
}
