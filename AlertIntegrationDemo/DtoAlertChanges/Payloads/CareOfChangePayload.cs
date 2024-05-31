using System;

namespace BiQ.AlertIntegrationDemo.DtoAlertChanges
{
    public class CareOfChangePayload
    {
        public string? ExistingCareOf { get; set; }
        public string? ProposedCareOf { get; set; }
        public DateTimeOffset? CareOfChangeDate { get; set; }
    }
}
