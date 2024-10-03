using System;
using System.Collections.Generic;

namespace BiQ.AlertIntegrationDemo.DtoShadow
{
    public class Customer
    {
        public string? CustomerNumber { get; set; }
        public string? Created { get; set; }
        public string? CreatedBy { get; set; }
        public string? ChangedBy { get; set; }
        public string? Changed { get; set; }
        public string? CustomerCategoryText { get; set; } 
        public Dictionary<string, string>? ExtraData { get; set; }
        public Contact? Contact1 { get; set; }
        public Contact? Contact2 { get; set; }
        public string? AssociatedApprovedChange { get; set; }
        public string? MasterSystemDeepLink { get; set; }
        public string? Active { get; set; }
    }
}
