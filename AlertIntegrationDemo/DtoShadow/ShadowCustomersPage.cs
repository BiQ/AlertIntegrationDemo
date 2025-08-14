using System.Collections.Generic;

namespace BiQ.AlertIntegrationDemo.DtoShadow
{
    public class ShadowCustomersPage
    {
        public int TotalItems { get; set; }
        public IEnumerable<CustomerInfo> Customers { get; set; } = [];
    }
}
