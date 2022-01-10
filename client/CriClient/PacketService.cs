using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Timers;

namespace CriClient
{
    class PacketService
    {
        const int USERNAME_MAX_LENGTH = 16;
        const int PASSWORD_MAX_LENGTH = 16;
        const int TCP_PORT = 5555;
        const int UDP_PORT = 5556;
        const string SERVER = "192.168.1.24";
            //"127.0.0.1";
        const int MESSAGE_MAX_LENGTH = 325;
        const int MAX_USER_COUNT = 100;

        public void SendPacket(bool isUdp, string payload)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(payload);
            if (!isUdp)
            {
                TcpClient client = new TcpClient(SERVER, TCP_PORT);
                NetworkStream stream = client.GetStream();
                stream.Write(data, 0, data.Length);
                Console.WriteLine("Sent");

                stream.Close();
            }

            else
            {
                UdpClient udpClient = new UdpClient();
                udpClient.Send(data, data.Length, SERVER, UDP_PORT);

            }

        }
        public void SendHeartbeat(string username)
        {
            Timer HbTimer = new Timer();
            HbTimer.Interval = 6000;
            HbTimer.Elapsed += (sender, e) => HeartBeat(sender, e, username);
            HbTimer.AutoReset = true;
            HbTimer.Enabled = true;

        }
        private void HeartBeat(object sender, ElapsedEventArgs e, string username)
        {
            //SendPacket(true, ProtocolCode.Hello + "\n" + username);
            Console.WriteLine("Heartbeat sent");
        }
        public string ReceivePacket()
        {
            IPAddress ipad = IPAddress.Parse(SERVER);
            TcpListener server = new TcpListener(IPAddress.Any, TCP_PORT);
            server.Start();
            List<byte> bytes = new List<byte>();
            string data = null;
            TcpClient client = server.AcceptTcpClient();
            NetworkStream stream = client.GetStream();

            int i;
            while ((i = stream.ReadByte()) != -1)
            {
                bytes.Add((byte)i);
            }
            data = System.Text.Encoding.UTF8.GetString(bytes.ToArray());
            //Console.WriteLine("Received: {0}", data);
            client.Close();
            server.Stop();
            return data;
        }



        public Response Register(string username, string password)
        {
            if (username.Length <= USERNAME_MAX_LENGTH && password.Length <= PASSWORD_MAX_LENGTH)
            {
                string packet = ProtocolCode.Register + "\n" + username + "\n" + password;
                //string packet = $"00\n{username}\n{password}";
                SendPacket(false, packet);
                string[] tokenizedanswer;
                int counter = 0;
                do
                {
                    string answer = ReceivePacket();
                    tokenizedanswer = answer.Split("\n");
                    counter++;
                } while (!ProtocolCode.Register.Equals(tokenizedanswer[0]) && counter < 2);
                
                if (tokenizedanswer[1] == "ALREADY_EXISTS")
                {
                    return new Response() { IsSuccessful = false, MessageToUser = "This user already exists." };
                }
                if(tokenizedanswer[1] == "OK")
                {
                    return new Response() { IsSuccessful = true, MessageToUser = "Registered Successfully" };
                }
                return new Response() { IsSuccessful = false, MessageToUser = "Unknown Error" };
            }
            else
            {
                throw new Exception("username or password char limit exceeded");
            }


        }

        public Response Login(string username, string password)
        {
            if (username.Length <= USERNAME_MAX_LENGTH && password.Length <= PASSWORD_MAX_LENGTH)
            {
                string packet = ProtocolCode.Login + "\n" + username + "\n" + password;
                SendPacket(false, packet);
                string[] tokenizedanswer;
                int counter = 0;
                do
                {
                    string answer = ReceivePacket();
                    tokenizedanswer = answer.Split("\n");
                    counter++;
                } while (!ProtocolCode.Login.Equals(tokenizedanswer[0]) && counter < 2);
                if (tokenizedanswer[1] == "OK")
                {
                    return new Response { IsSuccessful = true, MessageToUser = "Login successful. " };
                }
                if (tokenizedanswer[1] == "FAIL")
                {
                    return new Response { IsSuccessful = false, MessageToUser = "Cannot login. " };
                }
                return new Response() { IsSuccessful = false, MessageToUser = "Unknown Error" };
            }
            else
            {
                throw new Exception("username or password char limit exceeded");
            }
        }

        public void Logout()
        {
            string packet = ProtocolCode.Logout.ToString();
            SendPacket(false, packet);
        }

        public void Hello(string username)
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

        public Response Search(string username)
        {
            if (username.Length <= USERNAME_MAX_LENGTH)
            {
                string packet = ProtocolCode.Search + "\n" + username;
                SendPacket(false, packet);
                string[] tokenizedanswer;
                int counter = 0;
                do
                {
                    string answer = ReceivePacket();
                    tokenizedanswer = answer.Split("\n");
                    counter++;
                } while (!ProtocolCode.Search.Equals(tokenizedanswer[0]) && counter < 2);
                if (tokenizedanswer[1] == "OFFLINE")
                {
                    return new Response { IsSuccessful = false, MessageToUser = "User is offline. " };
                }
                if (tokenizedanswer[1] == "NOT_FOUND")
                {
                    return new Response { IsSuccessful = false, MessageToUser = "User not found. " };
                }
                return new Response() { IsSuccessful = false, MessageToUser = "Unknown Error" };
            }
            else
            {
                throw new Exception("username char limit exceeded");
            }
        }

        public void Chat()
        {
            string packet = ProtocolCode.Chat.ToString();
            SendPacket(false, packet);
        }

        public void Text(string username, string message)
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

        public void GroupCreate(List<string> usernames)
        {
            if (usernames.Count <= MAX_USER_COUNT)
            {
                string packet = ProtocolCode.GroupCreate + "\n" + string.Join("\n", usernames);
                SendPacket(false, packet);
                string[] tokenizedanswer;
                int counter = 0;
                do
                {
                    string answer = ReceivePacket();
                    tokenizedanswer = answer.Split("\n");
                    counter++;
                } while (!ProtocolCode.GroupCreate.Equals(tokenizedanswer[0]) && counter < 2);
            }
            else
            {
                throw new Exception("user count limit exceeded");
            }
        }

        public void GroupSearch(Guid gid)
        {
            string packet = ProtocolCode.GroupSearch + "\n" + gid;
            SendPacket(false, packet);
            string[] tokenizedanswer;
            int counter = 0;
            do
            {
                string answer = ReceivePacket();
                tokenizedanswer = answer.Split("\n");
                counter++;
            } while (!ProtocolCode.GroupSearch.Equals(tokenizedanswer[0]) && counter < 2);
        }

        public void GroupText(Guid gid, string username, string message)
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
