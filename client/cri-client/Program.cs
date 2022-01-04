using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.IO;


namespace cri_client
{
    class Program
    {
        static void Main(string[] args)
        {
            int port = 5555;
            string server= "192.168.1.24";
            TcpClient client = new TcpClient(server, port);
            Byte[] data = System.Text.Encoding.UTF8.GetBytes("Hello World!");

            NetworkStream stream = client.GetStream();
            stream.Write(data, 0, data.Length);
            Console.WriteLine("Sent");


            stream.Close();
        }
    }
}
