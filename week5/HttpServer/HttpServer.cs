using System;
using System.Net;
using System.Text;
using System.Text.Json;

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
                            response.Headers.Set("Content-Type", "text/plain");
                            response.StatusCode = (int)HttpStatusCode.NotFound;
                            string error = "404 - not found";

                            buffer = Encoding.UTF8.GetBytes(error);
                        }
                    }
                    else
                    {
                        response.Headers.Set("Content-Type", "text/plain");
                        string error = $"Directory '{_settings.Path}' not found.";
                        buffer = Encoding.UTF8.GetBytes(error);
                    }

                    Stream output = response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);

                    output.Close();

                    Listening();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("-");
                }
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
    }
}
