namespace WebServer.Attributes
{
    public class HttpGET : Attribute
    {
        public string? MethodURI { get; set; }

        public HttpGET() { }

        public HttpGET(string methodURI)
        {
            MethodURI = methodURI;
        }
    }
}
