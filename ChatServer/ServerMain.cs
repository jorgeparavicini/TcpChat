using System;

namespace ChatServer
{
    public static class ServerMain
    {
        private static void Main(string[] args)
        {
            using var server = new Server();
            Console.Read();
        }

    }
}