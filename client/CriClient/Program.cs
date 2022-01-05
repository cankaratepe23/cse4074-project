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
            Console.WriteLine("1.Register\n2.Login");
            string menuopt = Console.ReadLine();
            menuopt = menuopt.ToLower();
            Console.WriteLine("Type username of max. 16 characters");
            string uname = Console.ReadLine();
            Console.WriteLine("Type password of max. 16 characters");
            string pword = Console.ReadLine();


            if (menuopt == "1" || menuopt == "register")
            {
                packetservice.Register(uname, pword);
                packetsent = true;
            }
            else
            {
                packetservice.Login(uname, pword);
                packetsent = true;
            }

            //packetservice.ReceivePacket();


        }
    }
}
