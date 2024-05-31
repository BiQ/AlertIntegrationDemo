using System;

namespace BiQ.AlertIntegrationDemo.DtoAlertChanges
{
    public class Notification
    {
        public Guid Id { get; set; }
        public DateTimeOffset Time { get; set; }
        public string? EventType { get; set; }
        public string? Source { get; set; }
        public int ShadowSourceId { get; set; }
        public string? CustomerNumber { get; set; }
        public string? Key { get; set; }
        public int PayloadVersion { get; set; }
        public object? Payload { get; set; }
        public bool? Anonymized { get; set; }
        public DateTimeOffset? Committed { get; set; }
    }
}
