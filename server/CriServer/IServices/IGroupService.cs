using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CriServer.IServices
{
    interface IGroupService
    {
        void AddToGroupByGuid(Guid groupId, List<User> users);
        Group CreateGroup(List<User> users);
        Group GetGroupByGroupId(Guid groupId);
    }
}
