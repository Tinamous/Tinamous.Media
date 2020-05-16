using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AnalysisUK.Tinamous.Media.Domain.Documents;
using AnalysisUK.Tinamous.Media.Domain.Logging;
using AnalysisUK.Tinamous.Membership.Messaging.Dtos.User;
using AnalysisUK.Tinamous.Membership.Messaging.Requests.User;
using AnalysisUK.Tinamous.Messaging.Common.Dtos;
using AnalysisUK.Tinamous.Messaging.Common.Enums;
using EasyNetQ;

namespace AnalysisUK.Tinamous.Media.BL
{
    public class MembershipService : IMembershipService
    {
        private readonly IBus _bus;
        private static readonly IDictionary<Guid, User> UsersById = new Dictionary<Guid, User>();
        private static readonly object LockObject = new object();

        public MembershipService(IBus bus)
        {
            if (bus == null) throw new ArgumentNullException("bus");
            _bus = bus;
        }

        /// <summary>
        /// Not currently used. Hopefully Media service doesn't need to know anything
        /// about the member/device (maybe account/user retention time for media object?)
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task<User> LoadAsync(Guid accountId, Guid id)
        {
            Logger.LogMessage("Requesting user: {0}", id);

            lock (LockObject)
            {
                if (UsersById.ContainsKey(id))
                {
                    return UsersById[id];
                }
            }

            try
            {
                Logger.LogMessage("User not found in cache, getting from membership.");
                var request = new GetAccountUserByIdRequest
                {
                    User = new UserSummaryDto { AccountId = accountId, UserId = id },
                    RequestSource = Source.Media
                };
                var response = await _bus.RequestAsync<GetAccountUserByIdRequest, GetAccountUserByIdResponse>(request);
                Logger.LogMessage("Got user {0}", response.User);
                var user = AutoMapper.Mapper.Map<User>(response.User);
                return Cache(user);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "****** Failed to load user by id:" + id);
                throw;
            }
        }

        private User Cache(User user)
        {
            if (user == null)
            {
                Logger.LogWarn("Trying to cache null user.");
                return null;
            }

            lock (LockObject)
            {
                if (UsersById.ContainsKey(user.Id))
                {
                    UsersById[user.Id] = user;
                }
                else
                {
                    UsersById.Add(user.Id, user);
                }

                Logger.LogMessage("Cached user: {0}", user);
            }

            // TODO:
            return user;
        }

        /// <summary>
        /// Update the user in the cache.
        /// </summary>
        /// <param name="userDto"></param>
        public void UserUpdated(UserDto userDto)
        {
            lock (LockObject)
            {
                if (UsersById.ContainsKey(userDto.Id))
                {
                    UsersById.Remove(userDto.Id);
                }

                var user = AutoMapper.Mapper.Map<User>(userDto);
                Cache(user);
            }
        }

        public void RemoveUser(Guid userId)
        {
            lock (LockObject)
            {
                if (UsersById.ContainsKey(userId))
                {
                    UsersById.Remove(userId);
                }
            }
        }
    }
}