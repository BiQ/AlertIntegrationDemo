using System;

namespace BiQ.AlertIntegrationDemo.DtoAlertChanges
{
    public class CvrNumberChangePayload
    {
        public string? ExistingCvrNumber { get; set; }
        public string? ProposedCvrNumber { get; set; }
        public DateTimeOffset? CvrNumberChangeDate { get; set; }
    }
}
