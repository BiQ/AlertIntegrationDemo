using System;

namespace BiQ.AlertIntegrationDemo.DtoAlertChanges
{
    public class EmailChangePayload
    {
        public string? ExistingEmail { get; set; }
        public string? ProposedEmail { get; set; }
        public DateTimeOffset? EmailChangeDate { get; set; }
    }
}
