using System;
using System.Net.Sockets;
using JetBrains.Annotations;

namespace ChatServer
{
    [UsedImplicitly]
    internal class Program: IDisposable
    {
        private const int Port = 6700;
        private readonly TcpClient _client = new TcpClient("localhost", Port);

        private static void Main(string[] args)
        {
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
                _client.Dispose();
            }
        }
    }
}
