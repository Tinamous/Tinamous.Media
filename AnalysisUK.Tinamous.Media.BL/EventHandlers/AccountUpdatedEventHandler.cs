using System;
using System.Threading.Tasks;
using AnalysisUK.Tinamous.Media.Domain.Configuration;
using AnalysisUK.Tinamous.Media.Domain.Logging;
using AnalysisUK.Tinamous.Membership.Messaging.Events.Account;
using EasyNetQ;

namespace AnalysisUK.Tinamous.Media.BL.EventHandlers
{
    public class AccountUpdatedEventHandler : IDisposable
    {
        private readonly IBus _bus;
        private readonly IMembershipService _membershipService;
        private IDisposable _consumer;

        public AccountUpdatedEventHandler(IBus bus, IMembershipService membershipService)
        {
            if (bus == null) throw new ArgumentNullException("bus");

            _bus = bus;
            _membershipService = membershipService;

            InitializeMessaging();
        }

        private void InitializeMessaging()
        {
            string subscriptionId = string.Format("Media.{0}", ServerSettings.ServerName);
            _consumer = _bus.SubscribeAsync<AccountUpdatedEvent>(subscriptionId, OnMessageAsync);
        }

        public async Task OnMessageAsync(AccountUpdatedEvent obj)
        {
            Logger.LogMessage("Account updated: {0}", obj.AccountId);
            // Eventually Account will have media retention time information
            // so we may wish to store that locally for deletion of media items.
            //_membershipService.UpdateAccount(obj.AccountId);
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