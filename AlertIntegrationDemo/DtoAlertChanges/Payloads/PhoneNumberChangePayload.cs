using System;

namespace BiQ.AlertIntegrationDemo.DtoAlertChanges
{
    public class PhoneNumberChangePayload
    {
       
        public string? ExistingPhoneNumber { get; set; }
        public string? ProposedPhoneNumber { get; set; }
        public DateTimeOffset? PhoneNumberChangeDate { get; set; }
    }
}
