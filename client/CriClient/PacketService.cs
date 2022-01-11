﻿using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace CriClient
{
    static class PacketService
    {
        public static bool tcpPacketIncoming = false;
        public static bool canAcceptChatRequest = false;
        static Timer HbTimer;
        private static TcpListener tcpListener;
        private static bool isListeningEnabled = false;

        const int USERNAME_MAX_LENGTH = 16;
        const int PASSWORD_MAX_LENGTH = 16;
        const int TCP_PORT = 5555;
        const int UDP_PORT = 5556;
        const string SERVER = "172.29.91.122";
            //"numellus.tk";
            //"192.168.1.24";
            //"127.0.0.1";
        const int MESSAGE_MAX_LENGTH = 325;
        const int MAX_USER_COUNT = 100;

        public static string SendPacket(bool isUdp, string payload, string destination = SERVER)
        {
            byte[] data = Encoding.UTF8.GetBytes(payload);

            if (!isUdp)
            {
                TcpClient client = new TcpClient(destination, TCP_PORT);
                NetworkStream stream = client.GetStream();
                stream.Write(data, 0, data.Length);

                Console.WriteLine("Sent");
                List<byte> bytes = new List<byte>();
                int i;
                while ((i = stream.ReadByte()) != -1)
                {
                    bytes.Add((byte) i);
                }

                string dataRead = System.Text.Encoding.UTF8.GetString(bytes.ToArray());
                Console.WriteLine("Received: {0}", dataRead);
                stream.Close();
                return dataRead;
            }
            else
            {
                UdpClient udpClient = new UdpClient();
                udpClient.Send(data, data.Length, destination, UDP_PORT);
            }

            return "";
        }

        public static void SendHeartbeat(string username)
        {
            HbTimer = new Timer() { Interval = 6000, AutoReset = true };
            HbTimer.Elapsed += (sender, e) => HeartBeat(sender, e, username);
            HbTimer.Start();
        }

        public static void KillHeartbeat()
        {
            HbTimer.Stop();
            HbTimer.Dispose();
            HbTimer = null;
        }
        private static void HeartBeat(object sender, ElapsedEventArgs e, string username)
        {
            SendPacket(true, ProtocolCode.Hello + "\n" + username);
            //Console.WriteLine("Heartbeat sent");
        }

        public static void StartTcpListen()
        {
            isListeningEnabled = true;
            canAcceptChatRequest = true;
            Thread tcpListenThread = new Thread(() => TcpListen());
            tcpListenThread.Start();
        }

        public static void StopTcpListen()
        {
            isListeningEnabled = false;
            canAcceptChatRequest = false;
        }

        private static void TcpListen()
        {
            tcpListener = new TcpListener(IPAddress.Any, TCP_PORT);
            tcpListener.Start();
            while (isListeningEnabled)
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
                        tcpPacketIncoming = true;
                        Console.WriteLine("Accepted TCP client.");
                        NetworkStream incomingStream = client.GetStream();

                        byte[] incomingBuffer = new byte[256];
                        incomingStream.Read(incomingBuffer, 0, incomingBuffer.Length);
                        string messageReceived = Encoding.UTF8.GetString(incomingBuffer.Select(b => b).Where(b => b != 0).ToArray());

                        string[] parsedMessage = messageReceived.Split("\n");
                        
                        string response = RespondToChatRequest(client.Client.RemoteEndPoint.ToString());
                        byte[] data = Encoding.UTF8.GetBytes(response);
                        incomingStream.Write(data, 0, data.Length);
                        incomingStream.Close();
                        tcpPacketIncoming = false;
                    }).Start();
                }
            }
            tcpListener.Stop();
            tcpListener = null;
        }

        private static string RespondToChatRequest(string fromIp)
        { // TODO decouple this as this should be a UI method
            if (!canAcceptChatRequest)
            {
                return ProtocolCode.Chat + "\nBUSY";
            }
            char userOption = '\0';
            while (!(userOption == 'Y' || userOption == 'N'))
            {
                Console.WriteLine("\nIncoming chat request from {0}", fromIp);
                if (Dataholder.userIPs.ContainsValue(fromIp))
                {
                    Console.WriteLine("This IP was last seen online as user {0}", Dataholder.userIPs.FirstOrDefault((userIp) => userIp.Value == fromIp).Key);
                }
                Console.Write("Would you like to accept the request? (Enter, and then Y or N)");
                userOption = char.ToUpper(Console.ReadKey().KeyChar);
                Console.WriteLine();
            }
            if (userOption == 'Y')
            {
                return ProtocolCode.Chat + "\nOK";
            }
            else
            {
                return ProtocolCode.Chat + "\nREJECT";
            }
        }

        public static string ReceivePacket()
        {
            TcpListener server = new TcpListener(IPAddress.Any, TCP_PORT);
            server.Start();
            List<byte> bytes = new List<byte>();
            string data = null;
            TcpClient client = server.AcceptTcpClient();
            NetworkStream stream = client.GetStream();

            int i;
            while ((i = stream.ReadByte()) != -1)
            {
                bytes.Add((byte) i);
            }

            data = Encoding.UTF8.GetString(bytes.ToArray());
            Console.WriteLine("Received: {0}", data);
            client.Close();
            server.Stop();
            return data;
        }



        public static Response Register(string username, string password)
        {
            if (username.Length <= USERNAME_MAX_LENGTH && password.Length <= PASSWORD_MAX_LENGTH)
            {
                string packet = ProtocolCode.Register + "\n" + username + "\n" + password;
                //string packet = $"00\n{username}\n{password}";
                string answer = SendPacket(false, packet);
                string[] tokenizedanswer;
                int counter = 0;
                do
                {
                    tokenizedanswer = answer.Split("\n");
                    counter++;
                } while (!ProtocolCode.Register.Equals(tokenizedanswer[0]) && counter < 2);

                if (tokenizedanswer[1] == "ALREADY_EXISTS")
                {
                    return new Response() {IsSuccessful = false, MessageToUser = "This user already exists."};
                }

                if (tokenizedanswer[1] == "OK")
                {
                    return new Response() {IsSuccessful = true, MessageToUser = "Registered Successfully"};
                }

                return new Response() {IsSuccessful = false, MessageToUser = "Unknown Error"};
            }
            else
            {
                throw new Exception("username or password char limit exceeded");
            }

            return null;
        }

        public static Response Login(string username, string password)
        {
            if (username.Length <= USERNAME_MAX_LENGTH && password.Length <= PASSWORD_MAX_LENGTH)
            {
                string packet = ProtocolCode.Login + "\n" + username + "\n" + password;
                string answer = SendPacket(false, packet);
                string[] tokenizedanswer;
                int counter = 0;
                do
                {
                    tokenizedanswer = answer.Split("\n");
                    counter++;
                } while (!ProtocolCode.Login.Equals(tokenizedanswer[0]) && counter < 2);

                if (tokenizedanswer[1] == "OK")
                {
                    SendHeartbeat(username);
                    return new Response { IsSuccessful = true, MessageToUser = "Login successful. " };
                }

                if (tokenizedanswer[1] == "FAIL")
                {
                    return new Response {IsSuccessful = false, MessageToUser = "Cannot login. "};
                }

                return new Response() {IsSuccessful = false, MessageToUser = "Unknown Error"};
            }
            else
            {
                throw new Exception("username or password char limit exceeded");
            }
        }

        public static Response Logout(string username)
        {
            string packet = ProtocolCode.Logout.ToString() + "\n" + username;
            string answer = SendPacket(false, packet);
            string[] tokenizedanswer;
            tokenizedanswer = answer.Split("\n");
            if(tokenizedanswer[1] == "OK")
            {
                KillHeartbeat();
                return new Response { IsSuccessful = true, MessageToUser = "Logged out. " };
            }
            return new Response() { IsSuccessful = false, MessageToUser = "Unknown Error" };

        }

        public static void Hello(string username)
        {
            if (username.Length <= USERNAME_MAX_LENGTH)
            {
                string packet = ProtocolCode.Hello + "\n" + username;
                SendPacket(true, packet);
            }
            else
            {
                throw new Exception("username char limit exceeded");
            }
        }

        public static Response Search(string username)
        {
            if (username.Length <= USERNAME_MAX_LENGTH)
            {
                string packet = ProtocolCode.Search + "\n" + username;
                string answer = SendPacket(false, packet);
                string[] tokenizedanswer;
                int counter = 0;
                do
                {
                    tokenizedanswer = answer.Split("\n");
                    counter++;
                } while (!ProtocolCode.Search.Equals(tokenizedanswer[0]) && counter < 2);

                if (tokenizedanswer[1] == "OFFLINE")
                {
                    return new Response {IsSuccessful = false, MessageToUser = "User is offline. "};
                }

                if (tokenizedanswer[1] == "NOT_FOUND")
                {
                    return new Response {IsSuccessful = false, MessageToUser = "User not found. "};
                }
                if(tokenizedanswer[1] == "OK")
                {
                    if (Dataholder.userIPs.ContainsKey(username))
                    {
                        Dataholder.userIPs[username] = tokenizedanswer[2];
                    }
                    else
                    {
                        Dataholder.userIPs.Add(username, tokenizedanswer[2]);
                    }
                    return new Response { IsSuccessful = true, MessageToUser = "User is online. " };
                }
                return new Response() { IsSuccessful = false, MessageToUser = "Unknown Error" };
            }
            else
            {
                throw new Exception("username char limit exceeded");
            }
        }

        public static Response Chat(string username)
        {
            if (username.Length > USERNAME_MAX_LENGTH)
            {
                throw new Exception("Username char limit exceeded");
            }
            string packet = ProtocolCode.Chat.ToString() + "\n" + username;
            string destIp = Dataholder.userIPs[username];
            Console.WriteLine("Sending P2P chat request to: {0}", destIp);
            string answer = SendPacket(false, packet, destination: destIp);
            string[] tokenizedanswer = answer.Split('\n');
            if (tokenizedanswer[1] == "BUSY")
            {
                return new Response() { IsSuccessful = false, MessageToUser = "This user is currently busy." };
            }
            if (tokenizedanswer[1] == "REJECT")
            {
                return new Response() { IsSuccessful = false, MessageToUser = "The user has rejected your chat request." };
            }
            if (tokenizedanswer[1] == "OK")
            {
                return new Response() { IsSuccessful = true, MessageToUser = "" };
            }
            return new Response() { IsSuccessful = false, MessageToUser = "Unkown error." };
        }

        public static void Text(string username, string message)
        {
            if (username.Length <= USERNAME_MAX_LENGTH && message.Length <= MESSAGE_MAX_LENGTH)
            {
                string packet = ProtocolCode.Text + "\n" + username + "\n" + message;
                SendPacket(false, packet);
            }
            else
            {
                throw new Exception("username or message char limit exceeded");
            }
        }

        public static Response GroupCreate(List<string> usernames)
        {
            if (usernames.Count <= MAX_USER_COUNT)
            {
                string packet = ProtocolCode.GroupCreate + "\n" + string.Join("\n", usernames);
                string answer = SendPacket(false, packet);
                string[] tokenizedanswer = answer.Split("\n");
                if(tokenizedanswer[1] == "MEMBERS_NOT_FOUND")
                {
                    var listAnswer = new List<string>(tokenizedanswer);
                    return new Response { IsSuccessful = false, MessageToUser = "Following users are not found: " + string.Join("\n", tokenizedanswer.TakeLast(tokenizedanswer.Length - 2))};
                }
                if(tokenizedanswer[1] == "OK")
                {
                    return new Response { IsSuccessful = true, MessageToUser = "Group successfully created. " };
                }
                return new Response() { IsSuccessful = false, MessageToUser = "Unknown Error" };
            }
            else
            {
                throw new Exception("user count limit exceeded");
            }
        }

        public static Response GroupSearch(Guid gid)
        {
            string packet = ProtocolCode.GroupSearch + "\n" + gid;
            string answer = SendPacket(false, packet);
            string[] tokenizedanswer = answer.Split("\n");
            if(tokenizedanswer[1] == "NOT_FOUND")
            {
                return new Response { IsSuccessful = false, MessageToUser = "Group with the given ID doesn't exist." };
            }
            if(tokenizedanswer[1] == "OK")
            {
                //Dataholder.userIPs.Add(username, tokenizedanswer[2]);
                return new Response { IsSuccessful = true, MessageToUser = string.Join("\n", tokenizedanswer.TakeLast(tokenizedanswer.Length - 2)) };
            }
            return new Response() { IsSuccessful = false, MessageToUser = "Unknown Error" };
        }

        public static void GroupText(Guid gid, string username, string message)
        {
            if (username.Length <= USERNAME_MAX_LENGTH && message.Length <= MESSAGE_MAX_LENGTH)
            {
                string packet = ProtocolCode.GroupText + "\n" + gid + "\n" + username + "\n" + message;
                SendPacket(false, packet);
            }
            else
            {
                throw new Exception("username or message char limit exceeded");
            }
        }
    }
}