using System;
using System.Collections.Generic;

namespace BiQ.AlertIntegrationDemo.DtoAlertChanges
{ 
    public class CreatePersonPayload
    {
        public string? Name { get; set; }
        public DateTime? BirthDate { get; set; }
        public Address? Address { get; set; }
        public string? Cpr { get; set; }
        public string? CareOf { get; set; }
        public string? Attention { get; set; }
        public string? Email { get; set; }
        public string? Phone1 { get; set; }
        public string? Phone2 { get; set;  }
        public string? SecondaryContactName { get; set; }
        public string? SecondaryContactEmail { get; set; }
        public string? SecondaryContactPhoneNumber1 { get; set; }
        public string? SecondaryContactPhoneNumber2 { get; set;  }
        public string? SecondaryContactCprNumber { get; set; }
        public string? PoBox { get; set; }
        public IDictionary<string, string>? ExtraData { get; set; }
        public string? Active { get; set; }
    }
}
