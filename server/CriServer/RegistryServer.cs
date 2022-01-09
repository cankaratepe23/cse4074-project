using CriServer.IServices;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
            Thread tcpThread = new Thread(TcpListen);
            Thread udpThread = new Thread(UdpListen);
            tcpThread.Start();
            udpThread.Start();
        }

        private void TcpListen()
        {
            tcpListener = new TcpListener(IPAddress.Any, TCP_PORT);
            tcpListener.Start();
            while (true)
            {
                if (!tcpListener.Pending())
                {
                    Thread.Sleep(20);
                }
                else
                {
                    new Thread(() =>
                    {
                        TcpClient client = tcpListener.AcceptTcpClient();
                        NetworkStream incomingStream = client.GetStream();
                        List<int> incomingBuffer = new();
                        int currentRead;
                        while ((currentRead = incomingStream.ReadByte()) != -1)
                        {
                            incomingBuffer.Add(currentRead);
                        }

                        // Simulate long and blocking operation to test multi-threaded functionality
                        Log.Information("Received TCP connection from {IP} Sleeping...", client.Client.RemoteEndPoint);
                        //Thread.Sleep(2000);
                        string messageReceived =
                            Encoding.UTF8.GetString(incomingBuffer.Select(b => (byte)b).ToArray());
                        Log.Information("Received TCP message from {IP}:\n{Message}", client.Client.RemoteEndPoint,
                            messageReceived);
                        Console.WriteLine("Received TCP message from {0}:\n{1}", client.Client.RemoteEndPoint, messageReceived);

                        string[] parsedMessage = messageReceived.Split("\n");
                        ProtocolCode method = new ProtocolCode(parsedMessage[0]);
                        string[] payload = parsedMessage.Skip(1).ToArray();
                        IPAddress ipAddress = ((IPEndPoint)client.Client.RemoteEndPoint)?.Address;

                        RegistryResponse registryResponse = RegistryResponse.LOGIN_SUCCESSFUL;  // to be changed, handle logout
                        if (method.Equals(ProtocolCode.Register)) // FIXME: Reverse these
                            registryResponse = Register(payload);
                        else if (method.Equals(ProtocolCode.Login))
                            registryResponse = Login(payload, ipAddress);
                        else if (method.Equals(ProtocolCode.Logout))
                            Logout(ipAddress);
                        else if (method.Equals(ProtocolCode.Search))
                            registryResponse = Search(payload);

                        byte[] data = Encoding.UTF8.GetBytes(registryResponse.ToString());
                        client.GetStream().Write(data, 0, data.Length);
                        client.GetStream().Close();
                    }).Start();
                }
            }
        }

        private void UdpListen()
        {
            udpListener = new UdpClient(UDP_PORT);
            while (true)
            {
                IPEndPoint remoteEndPoint = null;
                byte[] incomingData = udpListener.Receive(ref remoteEndPoint);
                string payload = Encoding.UTF8.GetString(incomingData);
                List<string> tokenizedPayload = payload.Split('\n').ToList();
                if (!ProtocolCode.Hello.Equals(tokenizedPayload[0]))
                    break;
                Log.Information("Received TCP message from {IP}:\n{Message}", remoteEndPoint, tokenizedPayload);
                Console.WriteLine("Received UDP message from {0}:\n{1}", remoteEndPoint, tokenizedPayload);
            }
        }

        private RegistryResponse Register(string[] payload)
        {
            return _userService.RegisterUser(payload[0], payload[1]);
        }

        private RegistryResponse Login(string[] payload, IPAddress ipAddress)
        {
            return _userService.LoginUser(payload[0], payload[1], ipAddress);
        }

        private void Logout(IPAddress ipAddress)
        {
            _userService.LogoutUser(ipAddress);
        }

        private RegistryResponse Search(string[] payload)
        {
            return _userService.Search(payload[0]);
        }

        public void Stop()
        {
            tcpListener.Stop();
            udpListener.Close();
            udpListener.Dispose();
        }
    }
}