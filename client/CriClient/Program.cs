namespace CriClient
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
