using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthTokens.Helpers
{
    public class AuthenticationType
    {
        public AuthenticationType(AuthenticationTypeEnum authenticationTypeEnum, string name, bool isChecked)
        {
            AuthenticationTypeEnum = authenticationTypeEnum;
            Name = name;
            IsChecked = isChecked;
        }

        public AuthenticationTypeEnum AuthenticationTypeEnum { get; set; }
        public string Name { get; set; }
        public bool IsChecked { get; set; }
    }
}
