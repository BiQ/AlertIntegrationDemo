using System;

namespace BiQ.AlertIntegrationDemo.DtoAlertChanges
{
    public sealed class NameChangePayload
    {
        public string? ProposedName { get; set; }
        public string? ExistingName { get; set; }
        public Address? Address { get; set; }
        public DateTimeOffset? NameChangeDate { get; set; }

    }
}
