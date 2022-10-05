using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wsdlPNPAD
{
    public class ResponseAnalizador
    {
        public string code { get; set; }
        public string description { get; set; }
        public string uuid { get; set; }
        public string status { get; set; }
        public int risk { get; set; }
        public object deviceFingerPrint { get; set; }
        public bool needMFA { get; set; }
        public List<AuthAnal> auths { get; set; }
        public object deviceTokenCookie { get; set; }
        public object deviceTokenFSO { get; set; }
        public object sessionId { get; set; }
        public object clientTransactionId { get; set; }
        public object transactionId { get; set; }
        public object additionalReturn { get; set; }
        public string jwtSignData { get; set; }
        public object evaluatedContext { get; set; }
        public bool reviewRisk { get; set; }
        public object authType { get; set; }
    }
}