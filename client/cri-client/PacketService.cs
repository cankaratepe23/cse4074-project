using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace cri_client
{
    class PacketService
    {
        const int USERNAME_MAX_LENGTH = 16;
        const int PASSWORD_MAX_LENGTH = 16;
        const int TCP_PORT = 5555;
        const int UDP_PORT = 5556;
        const string SERVER = "127.0.0.1";
        const int MESSAGE_MAX_LENGTH = 325;
        const int MAX_USER_COUNT = 100;

        public void SendPacket(bool isUdp, string payload)
        {
            
            Byte[] data = System.Text.Encoding.UTF8.GetBytes(payload);
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
        public void Register(string username, string password)
        {
            if (username.Length <= USERNAME_MAX_LENGTH && password.Length <= PASSWORD_MAX_LENGTH)
            {
                string packet = ProtocolCode.Register + "\n" + username + "\n" + password;
                //string packet = $"00\n{username}\n{password}";
                SendPacket(false, packet);
            }
            else
            {
                throw new Exception("username or password char limit exceeded");
            }


        }

        public void Login(string username, string password)
        {
            if(username.Length <= USERNAME_MAX_LENGTH && password.Length <= PASSWORD_MAX_LENGTH)
            {
                string packet = ProtocolCode.Login + "\n" + username + "\n" + password;
                SendPacket(false, packet);
            }
            else
            {
                throw new Exception("username or password char limit exceededvvvv");
            }
        }

        public void Logout()
        {
            string packet = ProtocolCode.Logout.ToString();
            SendPacket(false, packet);
        }

        public void Hello(string username)
        {
            if(username.Length <= USERNAME_MAX_LENGTH)
            {
                string packet = ProtocolCode.Hello + "\n" + username;
                SendPacket(true, packet);
            }
            else
            {
                throw new Exception("username char limit exceeded");
            }
        }

        public void Search(string username)
        {
            if(username.Length <= USERNAME_MAX_LENGTH)
            {
                string packet = ProtocolCode.Search + "\n" + username;
                SendPacket(false, packet);
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
            if(username.Length <= USERNAME_MAX_LENGTH && message.Length <= MESSAGE_MAX_LENGTH)
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
            if(usernames.Count <= MAX_USER_COUNT)
            {
                string packet = ProtocolCode.GroupCreate + "\n" + String.Join("\n", usernames);
                SendPacket(false, packet);
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
