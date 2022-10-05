namespace wsdlPNPAD
{
    internal class Request
    {
        
        public object commonParamsRequest { get; set; }
        public object commonUserDataRequest { get; set; }
        public string transactionType { get; set; }
        public string transactionName { get; set; }
        public object segment { get; set; }
        public object deviceId { get; set; }
        public object location { get; set; }
        public object transactionHash { get; set; }
        public string subTypeTransaction { get; set; }
        public string additionalData { get; set; }
        public object factsList { get; set; }
        public object riskEngineData { get; set; }
        public object transactionData { get; set; }
        public object signData { get; set; }
    }
}