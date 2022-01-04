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
            PacketService packetservice = new PacketService();
            packetservice.Register("sinem", "123");

        }
    }





}
