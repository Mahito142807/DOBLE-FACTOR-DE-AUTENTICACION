using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wsdlPNPAD
{
    public class AuthAnal
    {
        public string authId { get; set; }
        public string authName { get; set; }
        public object type { get; set; }
        public string priority { get; set; }
        public bool show { get; set; }
        public bool dispached { get; set; }
        public object additionalsAuthValues { get; set; }
    }
}