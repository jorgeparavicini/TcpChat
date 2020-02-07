using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
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
        private readonly Dictionary<int, TcpClient> _clients = new Dictionary<int, TcpClient>();

        public Server()
        {
            var localAddress = IPAddress.Parse("127.0.0.1");
            _server = new TcpListener(localAddress, Port);
            Logger.Info($"Initialized Server with port: {Port}");
            _server.Start();

            // Run the listener in Background Thread
            Task.Run(StartServer);
        }

        private static void Main(string[] args)
        {
            using var server = new Server();
            Console.Read();
        }

        private async Task StartServer()
        {
            try
            {
                var clientCount = 1;
                while (true)
                {
                    Logger.Debug("Listening for connection");
                    var client = await _server.AcceptTcpClientAsync().ConfigureAwait(false);
                    lock(_clients) _clients.Add(clientCount, client);
                    Logger.Info($"Received Connection: {clientCount}");

                    var count = clientCount;
                    var clientThread = new Thread(() => HandleConnection(count))
                    {
                        Name = $"TCP Client Thread: {clientCount}"
                    };
                    clientThread.Start();
                    clientCount++;
                }
            }
            catch (SocketException e)
            {
                Logger.Error($"SocketException: {e}");
            }
            catch (Exception e)
            {
                Logger.Fatal($"Caught unhandled exception: {e}");
                throw;
            }
            finally
            {
                Logger.Info("Closing server");
                _server.Stop();
            }
        }

        private void HandleConnection(int id)
        {
            TcpClient client;

            lock (_clients)
            {
                client = _clients[id];
            }

            while (true)
            {
                var stream = client.GetStream();
                var buffer = new byte[1024];

                var byteCount = stream.Read(buffer, 0, buffer.Length);

                if (byteCount == 0)
                {
                    break;
                }

                var data = Encoding.ASCII.GetString(buffer, 0, byteCount);
                Broadcast(data);
                Logger.Info($"Broadcasted {data}");
            }

            lock (_clients) _clients.Remove(id);
            client.Client.Shutdown(SocketShutdown.Both);
            client.Close();
        }

        private void Broadcast(string data)
        {
            var buffer = Encoding.ASCII.GetBytes(data + Environment.NewLine);

            lock (_clients)
            {
                foreach (var stream in _clients.Values.Select(client => client.GetStream()))
                {
                    stream.Write(buffer, 0, buffer.Length);
                }
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
