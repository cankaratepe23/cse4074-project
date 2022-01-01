using System.Net;

namespace CriServer.IServices
{
    interface IUserService
    {
        void RegisterUser(User newUser);
        void LoginUser(string username, string password, IPAddress ipAddress);
        void LogoutUser(IPAddress ipAddress);
    }
}
