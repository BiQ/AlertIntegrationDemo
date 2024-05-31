using System;

namespace BiQ.AlertIntegrationDemo.DtoAlertChanges
{
    public sealed class ApprovedChange
    {
        public Guid Id { get; set; }
        //public int TenantId { get; set; }
        public int ShadowSourceId { get; set; }
        public string? CustomerNumber { get; set; }
        public DateTimeOffset Created { get; set; }
        public string? CreatedBy { get; set; }
        public DateTimeOffset Approved { get; set; }
        public string? ApprovedBy { get; set; }
        public string? Source { get; set; }
        public string? EventType { get; set; }
        public int PayloadVersion { get; set; }
        public object? Payload { get; set; }
        public bool? Anonymized { get; set; }
        public DateTimeOffset? Committed { get; set; }
    }
}
