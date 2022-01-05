using CriServer.IServices;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace CriServer
{
    class UserService : IUserService
    {
        private const int USERNAME_MAX_CHARACTER_LIMIT = 16;
        private const int PASSWORD_MAX_CHARACTER_LIMIT = 16;


        private readonly CriContext _criContext;

        public UserService(IServiceProvider services)
        {
            _criContext = services.GetService<CriContext>();
        }

        public void RegisterUser(User newUser)
        {
            if (!IsUserValid(newUser))
                throw new ArgumentException("Username and password should be less than 16 characters!");

            if (GetUserByUsername(newUser.Username) != null)
                throw new ArgumentException("This username has already been registered!");

            _criContext.Add(newUser);
            _criContext.SaveChanges();
        }

        public void LoginUser(string username, string password, IPAddress ipAddress)
        {
            var user = GetUserByUsername(username);

            if (user == null)
                throw new ArgumentException("User is not found!");
            else if (password != user.Password)
                throw new ArgumentException("Password is incorrect!");

            user.IpAddress = ipAddress;
            _criContext.SaveChanges();
        }

        public void LogoutUser(IPAddress ipAddress)
        {
            var user = GetUserByIPAddress(ipAddress);

            if (user == null)
                throw new ArgumentException("IP address is not found!");

            user.IpAddress = IPAddress.None;
            _criContext.SaveChanges();
        }

        public User GetUserByUsername(string username)
        {
            return _criContext.Users.Where(u => u.Username == username).FirstOrDefault();
        }

        private User GetUserByIPAddress(IPAddress ipAddress)
        {
            return _criContext.Users.Where(u => u.IpAddress == ipAddress).FirstOrDefault();
        }

        private List<User> GetUsersByUsernames(List<string> usernames)
        {
            return _criContext.Users.Where(u => usernames.Contains(u.Username)).ToList();
        }

        private bool IsUserValid(User user)
        {
            return user.Username.Length <= USERNAME_MAX_CHARACTER_LIMIT && user.Password.Length <= PASSWORD_MAX_CHARACTER_LIMIT;
        }
    }
}
