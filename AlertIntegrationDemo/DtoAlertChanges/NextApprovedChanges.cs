using System.Collections.Generic;

namespace BiQ.AlertIntegrationDemo.DtoAlertChanges
{
    public class NextApprovedChanges
    {
        public IEnumerable<ApprovedChange>? ApprovedChanges { get; set; }
        public bool More { get; set; }
    }
}
