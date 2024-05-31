using System.Collections.Generic;

namespace BiQ.AlertIntegrationDemo.DtoAlertChanges
{
    public class NextNotifications
    {
        public IEnumerable<Notification>? Notifications { get; set; }
        public bool More { get; set; }
    }
}
