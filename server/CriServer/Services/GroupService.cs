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
        private const int MAX_USER_COUNT = 100;

        private readonly CriContext _criContext;

        public GroupService(IServiceProvider services)
        {
            _criContext = services.GetService<CriContext>();
        }

        public Group CreateGroup(List<User> users)
        {
            if (users.Count > MAX_USER_COUNT)
            {
                throw new ArgumentException("Groups can't have more than 100 users.");
            }
            Group newGroup = new Group();
            foreach (User user in users)
            {
                newGroup.Users.Add(user);
            }
            _criContext.Add<Group>(newGroup);
            _criContext.SaveChanges();
            return newGroup;
        }

        public void AddToGroupByGuid(Guid groupId, List<User> users)
        {
            if (users.Count > MAX_USER_COUNT)
            {
                throw new ArgumentException("Groups can't have more than 100 users.");
            }
            Group groupToAddTo = GetGroupByGroupId(groupId);
            foreach (User user in users)
            {
                groupToAddTo.Users.Add(user);
            }
            _criContext.SaveChanges();
        }

        public Group GetGroupByGroupId(Guid groupId)
        {
            return _criContext.Groups.Where(g => g.GroupId == groupId).FirstOrDefault();
        }
    }
}
