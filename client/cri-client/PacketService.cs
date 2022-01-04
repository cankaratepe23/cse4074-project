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
        const string SERVER = "192.168.1.24";

        public void SendPacket(bool isUdp, string payload)
        {
            //int port = 5555;
            //string server = "192.168.1.24";
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
                //udp gönder
            }

        }
        public void Register(string username, string password)
        {
            if (username.Length <= USERNAME_MAX_LENGTH && password.Length <= PASSWORD_MAX_LENGTH)
            {
                string packet = ProtocolCode.Register +"\n" + username + "\n" + password;
                //string packet = $"00\n{username}\n{password}";
                SendPacket(false, packet);
            }
            else
            {
                throw new Exception("username or password is longer than 16 characters");
            }


        }

        
    }
}
