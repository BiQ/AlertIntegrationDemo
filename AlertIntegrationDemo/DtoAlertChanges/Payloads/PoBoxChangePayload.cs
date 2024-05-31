using System;

namespace BiQ.AlertIntegrationDemo.DtoAlertChanges
{
    public class PoBoxChangePayload
    {
       
        public string? ExistingPoBox { get; set; }
        public string? ProposedPoBox { get; set; }
        public DateTimeOffset? PoBoxChangeDate { get; set; }
    }
}
