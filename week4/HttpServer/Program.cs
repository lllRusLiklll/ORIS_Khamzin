using System;
using System.Net;
using System.IO;

namespace NetConsoleApp
{
    public class HttpServer
    {
        private readonly string _url;
        private readonly HttpListener _listener;
        private readonly HttpListenerContext _httpContext;

        public HttpServer(string url)
        {
            _url = url;
            _listener = new HttpListener();

            _listener.Prefixes.Add(url);
        }

        public void Start()
        {
            Console.WriteLine("Запуск сервера...");
            _listener.Start();
            Console.WriteLine("Сервер запущен.");

            Receive();
        }

        public void Restart()
        {
            Stop();
            Start();
        }

        public void Stop()
        {
            Console.WriteLine("Остановка сервера...");
            _listener.Stop();
            Console.WriteLine("Сервер остановлен");
        }

        private void Receive()
        {
            _listener.BeginGetContext(new AsyncCallback(ListenerCallback), _listener);
        }

        private void ListenerCallback(IAsyncResult result)
        {
            if (_listener.IsListening)
            {   
                try
                {
                    HttpListenerContext context = _listener.GetContext();

                    HttpListenerResponse response = context.Response;

                    string responseStr = File.ReadAllText("google/index.html");
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseStr);

                    response.ContentLength64 = buffer.Length;
                    Stream output = response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);

                    output.Close();

                    Receive();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Произошла ошибка.");
                    Stop();
                }
                
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var url = "http://localhost:8888/google/";

            var server = new HttpServer(url);

            var command = Console.ReadLine();
            
            while (command.Length > 0)
            {
                if (command == "Start")
                    server.Start();
                else if (command == "Restart")
                    server.Restart();
                else if (command == "Stop")
                    server.Stop();

                command = Console.ReadLine();
            }
        }
    }
}