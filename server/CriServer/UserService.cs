using CriServer.IServices;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CriServer
{
    class UserService : IUserService
    {
        private readonly CriContext _criContext;
        
        public UserService(IServiceProvider services)
        {
            _criContext = services.GetService<CriContext>();
        }

        public void RegisterUser(User newUser)
        {
            _criContext.Add<User>(newUser);
            _criContext.SaveChanges();
        }
    }
}
