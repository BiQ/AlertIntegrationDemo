namespace BiQ.AlertIntegrationDemo
{
    public class ConfigValues
    {
        public const int TenantId = 0;

        public const int ShadowSourceId = 0;

        public const string ApiKey = "00000000-0000-0000-0000-000000000000";

        public const string CustomerSystemConnectionString = "Data Source=C:/temp/Idq.IntegrationDemo/AlertIntegrationDemo/CustomerSystem/customersystem.db";

        public const string AuthorizationBase = "https://preprod.search-auth.biq.dk/api/auth/login"; // Or use "https://auth-api.idq.dk/api/auth/login" for production

        public const string CustomerShadowBase = "https://preprod.alert.biq.dk";  // Or use "https://alert.biq.dk" for production

        public const string AlertChangesBase = "https://preprod.alert-changes.biq.dk"; // Or use "https://alert-changes.biq.dk" for production

        public const string extraData1FieldName = "opretSom";

        public const string extraData2FieldName = "rolle";
    }
}
