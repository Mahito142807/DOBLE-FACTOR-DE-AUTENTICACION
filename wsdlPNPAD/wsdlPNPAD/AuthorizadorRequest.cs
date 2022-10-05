using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wsdlPNPAD
{
    internal class AuthorizadorRequest
    {
    public object commonParamsRequest { get; set; }
    public object commonUserDataRequest { get; set; }
    public string uuidTransaction { get; set; }
    public string challengeData { get; set; }
    public string authId { get; set; }
    public object additionalAuthValues { get; set; }
    public object additionalChallengeData { get; set; }
    public object externalSessionId { get; set; }
    public object signData { get; set; }
    public object riskEngineData { get; set; }
}

}