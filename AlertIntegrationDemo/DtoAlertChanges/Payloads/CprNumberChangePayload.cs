using System;

namespace BiQ.AlertIntegrationDemo.DtoAlertChanges
{
    public class CprNumberChangePayload
    {
        public string? ExistingCprNumber { get; set; }
        public string? ProposedCprNumber { get; set; }
        public DateTimeOffset? CprNumberChangeDate { get; set; }
    }
}
