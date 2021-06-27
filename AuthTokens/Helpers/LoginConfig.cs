using System.Collections.Generic;

namespace AuthTokens.Helpers
{
    public class LoginConfig
    {
        public LoginConfig(string clientId,string tenantId, bool useCustomScopes,
            List<string> scopes, List<AuthenticationType> authenticationTypes)
        {
            ClientId = clientId;
            TenantId = tenantId;
            UseCustomScopes = useCustomScopes;
            Scopes = scopes;
            AuthenticationTypes = authenticationTypes;
        }

        public LoginConfig()
        {
            
        }

        public string ClientId { get; set; }
        public string TenantId { get; set; }
        public bool UseCustomScopes { get; set; }
        public List<string> Scopes { get; set; }
        public List<AuthenticationType> AuthenticationTypes { get; set; }  
    }
}
