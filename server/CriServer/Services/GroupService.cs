using CriServer.IServices;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CriServer.Services
{
    class GroupService : IGroupService
    {
        private readonly CriContext _criContext;

        public GroupService(IServiceProvider services)
        {
            _criContext = services.GetService<CriContext>();
        }
    }
}
