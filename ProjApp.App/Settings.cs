using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjApp
{
    public class Settings
    {
        public string ClientID { get; set; } = null;
        public string TenantID { get; set; } = null;
        public string Authority { get; set; } = null;
        public NestedSettings[] Scopes { get; set; } = null; 
    }
}
