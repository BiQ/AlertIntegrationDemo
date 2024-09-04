using System;

namespace BiQ.AlertIntegrationDemo.DtoAlertChanges
{
    public class CustomerCategoryChangePayload
    {
        public string? ExistingCustomerCategory { get; set; }
        public string? ProposedCustomerCategory { get; set; }
        public DateTimeOffset? CustomerCategoryChangeDate { get; set; }
    }
}
