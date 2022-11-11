using System.Reflection;

namespace WebServer.Attributes
{
    public class ApiController : Attribute
    {
        public string? ControllerName { get; set; }

        public ApiController() { }

        public ApiController(string controllerName)
        {
            ControllerName = controllerName;
        }
    }
}
