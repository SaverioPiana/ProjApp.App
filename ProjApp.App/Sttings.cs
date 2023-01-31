using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjApp
{
    public class Sttings
    {
        public string ClientId { get; set; } = null;
        public string TenantId { get; set; } = null;
        public string Authority { get; set; } = null;
        public NestedSettings[] Scopes { get; set; } = null; 
    }
}
