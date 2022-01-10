﻿using CriServer.IServices;
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
            Thread tcpThread = new Thread(() => TcpListen(Log.Logger));
            Thread udpThread = new Thread(() => UdpListen(Log.Logger));
            tcpThread.Start();
            udpThread.Start();
        }

        private void TcpListen(ILogger logger)
        {
            logger.Information("logger from TcpListen()");
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
                        logger.Information("Received TCP connection from {IP} Sleeping...", client.Client.RemoteEndPoint);
                        //Thread.Sleep(2000);
                        string messageReceived =
                            Encoding.UTF8.GetString(incomingBuffer.Select(b => (byte)b).ToArray());
                        logger.Information("Received TCP message from {IP}:\n{Message}", client.Client.RemoteEndPoint,
                            messageReceived);

                        string[] parsedMessage = messageReceived.Split("\n");
                        ProtocolCode method = new ProtocolCode(parsedMessage[0]);
                        string[] payload = parsedMessage.Skip(1).ToArray();
                        IPAddress ipAddress = ((IPEndPoint)client.Client.RemoteEndPoint)?.Address;

                        RegistryResponse registryResponse = RegistryResponse.LOGIN_SUCCESSFUL;  // to be changed, handle logout
                        if (ProtocolCode.Register.Equals(method))
                            registryResponse = Register(payload);
                        else if (ProtocolCode.Login.Equals(method))
                            registryResponse = Login(payload, ipAddress);
                        else if (ProtocolCode.Logout.Equals(method))
                            Logout(ipAddress);
                        else if (ProtocolCode.Search.Equals(method))
                            registryResponse = Search(payload);

                        SendPacket(false, ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString(), registryResponse.ToString());
                    }).Start();
                }
            }
        }

        public void SendPacket(bool isUdp, string ip, string payload)
        {
            byte[] data = Encoding.UTF8.GetBytes(payload);
            if (!isUdp)
            {
                TcpClient client = new TcpClient(ip, TCP_PORT);
                NetworkStream stream = client.GetStream();
                stream.Write(data, 0, data.Length);
                stream.Close();
            }
            else
            {
                UdpClient udpClient = new UdpClient();
                udpClient.Send(data, data.Length, ip, UDP_PORT);
            }
        }

        private void UdpListen(ILogger logger)
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
                logger.Information("Received UDP message from {IP}:\n{Message}", remoteEndPoint, tokenizedPayload);
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