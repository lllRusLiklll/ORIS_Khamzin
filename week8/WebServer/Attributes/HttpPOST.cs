namespace WebServer.Attributes
{
    public class HttpPOST : Attribute
    {
        public string? MethodURI { get; set; }

        public HttpPOST() { }

        public HttpPOST(string methodURI)
        {
            MethodURI = methodURI;
        }
    }
}
