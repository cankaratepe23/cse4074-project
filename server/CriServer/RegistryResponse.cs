using System.Net;

namespace CriServer
{
    public class RegistryResponse
    {
        private string Value { get; }

        private RegistryResponse(string value)
        {
            Value = value;
        }

        public static RegistryResponse REGISTER_SUCCESSFUL => new(ProtocolCode.Register + "\nOK");

        public static RegistryResponse REGISTER_INVALID_USERNAME_OR_PASSWORD => new(ProtocolCode.Register + "\nFAIL");

        public static RegistryResponse REGISTER_USERNAME_ALREADY_REGISTERED =>
            new(ProtocolCode.Register + "\nALREADY_EXISTS");

        public static RegistryResponse LOGIN_SUCCESSFUL => new(ProtocolCode.Login + "\nOK");
        public static RegistryResponse LOGIN_FAIL => new(ProtocolCode.Login + "\nFAIL");

        public static RegistryResponse SEARCH_USER_ONLINE(IPAddress ipAddress) =>
            new(ProtocolCode.Search + "\n" + ipAddress);

        public static RegistryResponse SEARCH_USER_OFFLINE => new(ProtocolCode.Search + "\nOFFLINE");

        public static RegistryResponse SEARCH_USER_NOT_FOUND => new(ProtocolCode.Search + "\nNOT_FOUND");

        public override string ToString()
        {
            return Value;
        }
    }
}