using System;
using System.Globalization;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NLog;

namespace ChatServer
{
    [UsedImplicitly]
    internal class Server: IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private const int Port = 6700;
        private readonly TcpListener _server;

        public Server()
        {
            Logger.Info("Initialized");
            var localAddress = IPAddress.Parse("127.0.0.1");
            _server = new TcpListener(localAddress, Port);
            _server.Start();

            // Run the listener in Background Thread
            Task.Run(StartListen);
        }

        private static void Main(string[] args)
        {
            using var server = new Server();
            Console.Read();
        }

        private async Task StartListen()
        {
            try
            {
                while (true)
                {
                    TcpClient client = null;
                    try
                    {
                        Logger.Debug("Waiting for a connection...");

                        // Blocking call to accept requests
                        client = await _server.AcceptTcpClientAsync().ConfigureAwait(false);
                        Logger.Info($"Client Connected: {client}");

                        // Get the stream object to read and write data.
                        var stream = client.GetStream();

                        var bytes = new byte[256];
                        while (stream.DataAvailable)
                        {
                            var data = Encoding.ASCII.GetString(bytes, 0, stream.Read(bytes, 0, bytes.Length));
                            Logger.Info($"Received: {data}");

                            data = data.ToUpper(CultureInfo.CurrentCulture);

                            var msg = Encoding.ASCII.GetBytes(data);
                            await stream.WriteAsync(msg, 0, msg.Length).ConfigureAwait(false);

                            Logger.Info($"Sent back: {data}");
                        }

                        Logger.Debug("Closing Stream");
                        stream.Close();
                    }
                    finally
                    {
                        if (client != null && client.Connected)
                        {
                            Logger.Debug($"Closing client connection: {client}");
                            client.Close();
                        }
                    }
                }
            }
            catch (SocketException e)
            {
                Logger.Error($"SocketException: {e}");
            }
            finally
            {
                Logger.Info($"Closing server");
                _server.Stop();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool dispose)
        {
            if (dispose)
            {
            }
        }
    }
}
