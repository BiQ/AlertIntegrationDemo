using System;
using System.Collections.Generic;

namespace BiQ.AlertIntegrationDemo.DtoAlertChanges
{
    public class ExtraDataChangePayload
    {
        public IDictionary<string, string>? ExistingExtraData { get; set; }
        public IDictionary<string, string>? ProposedExtraData { get; set; }
        public DateTime? ExtraDataChangeDate { get; }
    }
}
