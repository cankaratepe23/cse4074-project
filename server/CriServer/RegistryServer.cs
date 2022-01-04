using CriServer.IServices;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CriServer
{
    class RegistryServer
    {
        private readonly IUserService _userService;

        private TcpListener tcpListener;
        private UdpClient udpListener;
        private const int TCP_PORT = 5555;
        private const int UDP_PORT = 5556;

        public RegistryServer(IUserService userService)
        {
            _userService = userService;
        }

        public void Start()
        {
            tcpListener = new TcpListener(System.Net.IPAddress.Any, TCP_PORT);
            udpListener = new UdpClient(UDP_PORT);
            tcpListener.Start();
            while (true)
            {
                if (!tcpListener.Pending())
                {
                    Thread.Sleep(20);
                    continue;
                }
                TcpClient client = tcpListener.AcceptTcpClient();
                NetworkStream incomingStream = client.GetStream();
                List<int> incomingBuffer = new();
                int currentRead;
                while ((currentRead = incomingStream.ReadByte()) != -1)
                {
                    incomingBuffer.Add(currentRead);
                }
                string messageReceived = Encoding.UTF8.GetString(incomingBuffer.Select( b => (byte) b ).ToArray());
                Log.Information("Received TCP message:\n{Message}", messageReceived);
            }
        }

        public void Stop()
        {
            tcpListener.Stop();
            udpListener.Close();
            udpListener.Dispose();
        }
    }
}
