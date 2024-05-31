using System;

namespace BiQ.AlertIntegrationDemo.DtoAlertChanges
{
    public class AttentionChangePayload
    {
        public string? ExistingAttention { get; set; }
        public string? ProposedAttention { get; set; }
        public DateTime? AttentionChangeDate { get; set; }
    }
}
