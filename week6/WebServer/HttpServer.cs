using System;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using WebServer.Attributes;
using WebServer.Controllers;

namespace WebServer
{
    public class HttpServer : IDisposable
    {
        private readonly HttpListener _listener;

        public ServerStatus Status { get; private set; } = ServerStatus.Stop;
        private ServerSettings _settings;

        public HttpServer()
        {
            _listener = new HttpListener();
        }

        public void Start()
        {
            if (Status == ServerStatus.Start)
            {
                Console.WriteLine("Сервер уже запущен");
                return;
            }

            _settings = JsonSerializer.Deserialize<ServerSettings>(File.ReadAllBytes("./settings.json"));

            _listener.Prefixes.Clear();
            _listener.Prefixes.Add($"http://localhost:{_settings.Port}/");

            Console.WriteLine("Запуск сервера...");
            _listener.Start();

            Console.WriteLine("Сервер запущен.");
            Status = ServerStatus.Start;

            Listening();
        }

        public void Stop()
        {
            if (Status == ServerStatus.Stop)
            {
                Console.WriteLine("Сервер уже остановлен");
                return;
            }

            Console.WriteLine("Остановка сервера...");
            _listener.Stop();

            Console.WriteLine("Сервер остановлен");
            Status = ServerStatus.Stop;
        }

        private void Listening()
        {
            _listener.BeginGetContext(new AsyncCallback(ListenerCallback), _listener);
        }

        private void ListenerCallback(IAsyncResult result)
        {
            if (_listener.IsListening)
            {
                try
                {
                    var httpContext = _listener.EndGetContext(result);
                    HttpListenerResponse response = httpContext.Response;

                    byte[] buffer;

                    if (Directory.Exists(_settings.Path))
                    {
                        var rawUrl = httpContext.Request.RawUrl.Replace("%20", " ");
                        buffer = GetFile(rawUrl, response);

                        if (buffer == null)
                        {
                            if (!MethodHandler(httpContext))
                            {
                                response.Headers.Set("Content-Type", "text/plain");
                                response.StatusCode = (int)HttpStatusCode.NotFound;
                                string error = "404 - not found";

                                buffer = Encoding.UTF8.GetBytes(error);

                                Stream output = response.OutputStream;
                                output.Write(buffer, 0, buffer.Length);

                                output.Close();
                            }

                        }
                        else
                        {
                            Stream output = response.OutputStream;
                            output.Write(buffer, 0, buffer.Length);

                            output.Close();
                        }
                    }
                    else
                    {
                        response.Headers.Set("Content-Type", "text/plain");
                        string error = $"Directory '{_settings.Path}' not found.";
                        buffer = Encoding.UTF8.GetBytes(error);

                        Stream output = response.OutputStream;
                        output.Write(buffer, 0, buffer.Length);

                        output.Close();
                    }
                }
                catch (Exception ex)
                { }

                Listening();
            }
         }

        private byte[] GetFile(string rawUrl, HttpListenerResponse response)
        {
            byte[] buffer = null;
            var filePath = _settings.Path + rawUrl;

            if (Directory.Exists(filePath))
            {
                filePath += "/index.html";
                if (File.Exists(filePath))
                {
                    response.Headers.Set("Content-Type", "text/html");
                    buffer = File.ReadAllBytes(filePath);
                }
            }
            else if (File.Exists(filePath))
            {
                var mime = MimeMapping.GetMimeMapping(filePath);
                response.Headers.Set("Content-Type", mime);
                buffer = File.ReadAllBytes(filePath);
            }

            return buffer;
        }

        public void Dispose()
        {
            Stop();
        }

        private bool MethodHandler(HttpListenerContext _httpContext)
        {
            // объект запроса
            HttpListenerRequest request = _httpContext.Request;

            // объект ответа
            HttpListenerResponse response = _httpContext.Response;

            if (_httpContext.Request.Url!.Segments.Length <= 2) return false;

            string controllerName = _httpContext.Request.Url.Segments[1].Replace("/", "");
            string methodName = _httpContext.Request.Url.Segments[2].Replace("/", "");

            var assembly = Assembly.GetExecutingAssembly();

            var controller = FindControllerAttribute(assembly.GetTypes().Where(t => Attribute.IsDefined(t, typeof(ApiController))), controllerName);

            if (controller == null) return false;

            var methodType = assembly.GetTypes().FirstOrDefault(t => t.Name == $"Http{_httpContext.Request.HttpMethod}")!;
            var method = FindMethods(controller.GetMethods().Where(t => t.GetCustomAttributes(true)
                .Any(attr => attr.GetType().Name == $"Http{_httpContext.Request.HttpMethod}")),
                methodName, methodType);
                                                
            
            if (method == null) return false;

            List<object> strParams;
            switch (_httpContext.Request.HttpMethod)
            {
                case "GET":
                    strParams = _httpContext.Request.Url
                                            .Segments
                                            .Skip(2)
                                            .Select(s => s.Replace("/", ""))
                                            .ToList<object>();
                    break;
                case "POST":
                    using (var stream = new StreamReader(request.InputStream, request.ContentEncoding))
                    {
                        var query = stream.ReadToEnd();
                        strParams = query.Split('&').Select(x => x.Split('=')[1]).ToList<object>();
                    }
                    break;
                default:
                    return false;
            }

            if (method.GetParameters().Where(x => x.ParameterType == typeof(HttpListenerResponse)).Count() == 1)
                strParams.Add(response);


            object[] queryParams = method.GetParameters()
                                .Select((p, i) => Convert.ChangeType(strParams[i], p.ParameterType))
                                .ToArray();

            var ret = method.Invoke(Activator.CreateInstance(controller), queryParams);

            response.ContentType = "Application/json";

            byte[] buffer = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(ret));
            response.ContentLength64 = buffer.Length;

            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);

            output.Close();

            return true;
        }

        private static Type? FindControllerAttribute(IEnumerable<Type> controllers, string controllerName)
        {
            foreach (var item in controllers)
            {
                var hc = (ApiController)item.GetCustomAttribute(typeof(ApiController))!;
                if (hc.ControllerName is null)
                { 
                    if (controllerName.ToLower() == item.Name.ToLower())
                        return item;
                }
                else if (controllerName.ToLower() == hc.ControllerName!.ToLower())
                    return item;
            }
            return null;
        }

        private static MethodInfo? FindMethods(IEnumerable<MethodInfo> methods, string methodName, Type methodType)
        {
            foreach (var method in methods)
            {
                if (methodType.GetProperty("MethodURI")!.GetValue(method.GetCustomAttribute(methodType)) == null
                    && method.Name.ToLower() == methodName.ToLower()
                    || (string)methodType.GetProperty("MethodURI")!.GetValue(method.GetCustomAttribute(methodType))! == methodName.ToLower())
                {
                    return method;
                }
            }
            return null;
        }
    }
}
