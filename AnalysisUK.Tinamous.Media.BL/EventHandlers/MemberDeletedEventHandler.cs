using System;
using System.Threading.Tasks;
using AnalysisUK.Tinamous.Media.Domain.Logging;
using AnalysisUK.Tinamous.Media.Messaging.Events;
using AnalysisUK.Tinamous.Membership.Messaging.Events.User;
using EasyNetQ;

namespace AnalysisUK.Tinamous.Media.BL.EventHandlers
{
    public class MemberDeletedEventHandler: IDisposable
    {
        private readonly IBus _bus;
        private IDisposable _consumer;

        public MemberDeletedEventHandler(IBus bus)
        {
            if (bus == null) throw new ArgumentNullException("bus");

            _bus = bus;
            _consumer = _bus.SubscribeAsync<MemberDeletedEvent>("Media", OnMessageAsync);
        }

        public async Task OnMessageAsync(MemberDeletedEvent memberDeletedEvent)
        {
            var user = memberDeletedEvent.User;
            Logger.LogMessage("Member deleted event: UserId: {0}", user.UserId);

            PurgeOldMediaRequestEvent purgeOldMediaRequest = new PurgeOldMediaRequestEvent
            {
                UpTo = DateTime.UtcNow,
                User = memberDeletedEvent.User
            };
            await _bus.PublishAsync(purgeOldMediaRequest);
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