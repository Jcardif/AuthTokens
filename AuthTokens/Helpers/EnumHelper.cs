using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthTokens.Helpers
{
    public enum AuthenticationTypeEnum
    {
        MultipleOrgsOnly,
        SingleOrgOnly,
        PersonalAccountsOnly,
        BothOrgAndPersonalAccounts
    }

    public enum LoginSettings
    {
        ClientId,
        TenantId,
        AuthType,
        UseCustomScopes,
        Scopes
    }

}
