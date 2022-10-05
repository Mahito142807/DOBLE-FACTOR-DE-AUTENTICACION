using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wsdlPNPAD
{
    public class ResponseAutorizador
    { 
    public string code { get; set; }
    public string description { get; set; }
    public object status { get; set; }
    public string uuid { get; set; }
    public object deviceFingerPrint { get; set; }
    public object risk { get; set; }
    public bool needMFA { get; set; }
    public object auths { get; set; }
    public object deviceTokenCookie { get; set; }
    public object deviceTokenFSO { get; set; }
    public object sessionId { get; set; }
    public object clientTransactionId { get; set; }
    public object transactionId { get; set; }
    public object additionalReturn { get; set; }
    public object telephoneNumberOfChallenge { get; set; }
}

}