using System.Collections.Generic;

namespace BiQ.AlertIntegrationDemo.DtoAlertChanges
{
    public class CreateCompanyPayload
    {
        public string? Name { get; set; }
        public Address? Address { get; set; }
        public string? Cvr { get; set; }
        public string? Pnr { get; set; }
        public string? CareOf { get; set; }
        public string? Attention { get; set; }
        public string? Email { get; set; }
        public string? Phone1 { get; set; }
        public string? Phone2 { get; set; }
        public string? PoBox { get; set; }
        public IDictionary<string, object>? ExtraData { get; set; }
        public string? Active { get; set; }
    }
}
