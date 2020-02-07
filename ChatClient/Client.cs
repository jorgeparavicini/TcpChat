using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using NLog;

namespace ChatClient
{
    internal class Client
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private const int Port = 6700;

        public Client()
        {
            var ip = IPAddress.Parse("127.0.0.1");
            var client = new TcpClient();
            client.Connect(ip, Port);

            Logger.Info("Client Connected");
            var ns = client.GetStream();
            var thread = new Thread(o => ReceiveData((TcpClient) o));
            thread.Start(client);

            string s;
            while (!string.IsNullOrEmpty((s = Console.ReadLine())))
            {
                byte[] buffer = Encoding.ASCII.GetBytes(s);
                ns.Write(buffer, 0, buffer.Length);
            }

            client.Client.Shutdown(SocketShutdown.Send);
            thread.Join();
            ns.Close();
            client.Close();
            Console.WriteLine("disconnect from server!!");
            Console.ReadKey();
        }

        private static void Main(string[] args)
        {
            var client = new Client();

            // Block
            Console.Read();
        }

        static void ReceiveData(TcpClient client)
        {
            NetworkStream ns = client.GetStream();
            byte[] receivedBytes = new byte[1024];
            int byte_count;

            while ((byte_count = ns.Read(receivedBytes, 0, receivedBytes.Length)) > 0)
            {
                Console.Write(Encoding.ASCII.GetString(receivedBytes, 0, byte_count));
            }
        }

    }
}
