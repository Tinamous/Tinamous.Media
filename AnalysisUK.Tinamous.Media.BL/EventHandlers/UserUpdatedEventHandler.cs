using System;
using System.Threading.Tasks;
using AnalysisUK.Tinamous.Media.Domain.Configuration;
using AnalysisUK.Tinamous.Media.Domain.Logging;
using AnalysisUK.Tinamous.Membership.Messaging.Events;
using EasyNetQ;

namespace AnalysisUK.Tinamous.Media.BL.EventHandlers
{
    public class UserUpdatedEventHandler : IDisposable
    {
        private readonly IBus _bus;
        private readonly IMembershipService _membershipService;
        private IDisposable _consumer;

        public UserUpdatedEventHandler(IBus bus, IMembershipService membershipService)
        {
            if (bus == null) throw new ArgumentNullException("bus");

            _bus = bus;
            _membershipService = membershipService;

            InitializeMessaging();
        }

        private void InitializeMessaging()
        {
            string subscriptionId = string.Format("Media.{0}", ServerSettings.ServerName);
            _consumer = _bus.SubscribeAsync<IUserUpdatedEvent>(subscriptionId, OnMessageAsync);
        }

        public async Task OnMessageAsync(IUserUpdatedEvent obj)
        {
            Logger.LogMessage("User updated: {0}", obj.User.UserId);
            _membershipService.UserUpdated(obj.UserDetails);
        }

        public void Dispose()
        {
            if (_consumer != null)
            {
                _consumer.Dispose();
                _consumer = null;
            }
        }
    }
}