using System;
using System.Net.Sockets;
using System.Text;

namespace ChatClient
{
    internal static class Client
    {
        private static void Main(string[] args)
        {
            sendTCPPacket(ServerIp, Port, "Hello");
        }

        const int Port = 6700;
        const string ServerIp = "127.0.0.1";

        public static void sendTCPPacket(string target, int port, string content) {
            TcpClient client = new TcpClient(target, port);
            NetworkStream nwStream = client.GetStream();
            byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(content);
            nwStream.Write(bytesToSend, 0, bytesToSend.Length);
            client.Close();
        }

    }
}
