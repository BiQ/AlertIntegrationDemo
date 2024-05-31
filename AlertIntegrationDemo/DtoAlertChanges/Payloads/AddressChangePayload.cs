using System;

namespace BiQ.AlertIntegrationDemo.DtoAlertChanges
{
    public class AddressChangePayload
    {
        public string? Name { get; set; }
        public Address? ProposedAddress { get; set; }
        public Address? ExistingAddress { get; set; }
        public DateTimeOffset? AddressChangeDate { get; set; }
    }
}
