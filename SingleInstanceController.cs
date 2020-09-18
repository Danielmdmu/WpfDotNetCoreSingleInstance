using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;

namespace ExplorationSingleInstance
{
    public class SingleInstanceController
    {
        private const string EXIT_STRING = "__EXIT__";
        private const string separator = "|#|";
        private Action<string> argumentHandler;
        private bool isOnlyInstance;
        private bool isRunning;
        private Thread Thread;

        public SingleInstanceController(string id, string[] args, Action<string> action)
        {
            Identifier = id;
            isRunning = false;
            argumentHandler = action;

            Mutex = new Mutex(true, id, out isOnlyInstance);

            if (IsOnlyInstance)
            {
                StartServer();
            }
            else
            {
                SignalFirstInstance(args);
            }
        }

        ~SingleInstanceController()
        {
            StopServer();
        }

        public string Identifier { get; }

        public bool IsOnlyInstance => isOnlyInstance;

        private Mutex Mutex { get; }

        public void StartServer()
        {
            Thread = new Thread((pipeName) =>
            {
                isRunning = true;

                string text;

                while (true)
                {
                    using (var server = new NamedPipeServerStream(pipeName as string))
                    {
                        server.WaitForConnection();

                        using (StreamReader reader = new StreamReader(server))
                        {
                            text = reader.ReadToEnd();
                        }

                        if (text.Equals(EXIT_STRING))
                        {
                            break;
                        }

                        argumentHandler.Invoke(text);

                        if (!isRunning)
                        {
                            break;
                        }
                    }
                }
                Environment.Exit(0);
            });

            Thread.Start(Identifier);
        }

        public void StopServer()
        {
            isRunning = false;
            Write(EXIT_STRING);
            Thread.Sleep(TimeSpan.FromSeconds(2));
        }

        public bool Write(string text, int timeout = 300)
        {
            using (var client = new NamedPipeClientStream(Identifier))
            {
                try
                {
                    client.Connect(timeout);
                }
                catch
                {
                    return false;
                }

                if (!client.IsConnected)
                {
                    return false;
                }

                using var writer = new StreamWriter(client);
                writer.Write(text);
                writer.Flush();
            }

            return true;
        }

        private void SignalFirstInstance(string[] args)
        {
            string argumentString = string.Empty;
            if (args?.Length >= 1)
            {
                StringBuilder sb = new StringBuilder();

                foreach (var arg in args)
                {
                    sb.AppendLine(arg);
                }
                argumentString = sb.ToString();
            }

            Write(argumentString);

            Environment.Exit(0);
        }
    }
}