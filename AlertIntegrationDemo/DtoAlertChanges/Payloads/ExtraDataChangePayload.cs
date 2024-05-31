using System;
using System.Collections.Generic;

namespace BiQ.AlertIntegrationDemo.DtoAlertChanges
{
    public class ExtraDataChangePayload
    {
        public IDictionary<string, object>? ExistingExtraData { get; set; }
        public IDictionary<string, object>? ProposedExtraData { get; set; }
        public DateTime? ExtraDataChangeDate { get; }
    }
}
