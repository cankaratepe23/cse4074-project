using System.Net;

namespace CriServer.IServices
{
    interface IUserService
    {
        RegistryResponse RegisterUser(string username, string password);
        RegistryResponse LoginUser(string username, string password, IPAddress ipAddress);
        void LogoutUser(IPAddress ipAddress);
        RegistryResponse Search(string username);
        User GetUserByUsername(string username);
    }
}