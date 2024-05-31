using System;

namespace BiQ.AlertIntegrationDemo.DtoAlertChanges
{
    public class PNumberChangePayload
    {
        public string? ExistingPNumber { get; set; }
        public string? ProposedPNumber { get; set; }
        public DateTimeOffset? PNumberChangeDate { get; set; }
    }
}
