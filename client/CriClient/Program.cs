using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CriClient
{
    class Program
    {

        static bool packetsent = false;
        static void Main(string[] args)
        {

            PacketService packetservice = new PacketService();

            while (true)
            {

                Console.WriteLine("1.Register\n2.Login\n3.HeartBeat");
                string menuopt = Console.ReadLine();
                menuopt = menuopt.ToLower();
                Console.WriteLine("Type username of max. 16 characters");
                string uname = Console.ReadLine();
                Console.WriteLine("Type password of max. 16 characters");
                string pword = Console.ReadLine();


                if (menuopt == "1" || menuopt == "register")
                {
                    var response = packetservice.Register(uname, pword);
                    Console.WriteLine(response.MessageToUser);
                    packetsent = true;
                }
                else if (menuopt == "3" || menuopt == "heartbeat")
                {
                    packetservice.SendHeartbeat(uname);
                    packetsent = true;
                }
                else if (menuopt == "2" || menuopt == "login")
                {
                    var response = packetservice.Login(uname, pword);
                    Console.WriteLine(response.MessageToUser);
                    packetsent = true;
                }
                else
                {
                    System.Environment.Exit(0);
                }

            }



            //packetservice.ReceivePacket();


        }
    }
}
