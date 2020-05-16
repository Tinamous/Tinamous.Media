using System;
using System.Threading.Tasks;
using AnalysisUK.Tinamous.Media.Domain.Logging;
using AnalysisUK.Tinamous.Media.Messaging.Events;
using AnalysisUK.Tinamous.Membership.Messaging.Events.Device;
using AnalysisUK.Tinamous.Messaging.Common.Dtos;
using EasyNetQ;

namespace AnalysisUK.Tinamous.Media.BL.EventHandlers
{
    public class DeviceDeletedEventHandler: IDisposable
    {
        private readonly IBus _bus;
        private IDisposable _consumer;

        public DeviceDeletedEventHandler(IBus bus)
        {
            if (bus == null) throw new ArgumentNullException("bus");

            _bus = bus;
            _consumer = _bus.SubscribeAsync<DeviceDeletedEvent>("Media", OnMessageAsync);
        }

        public async Task OnMessageAsync(DeviceDeletedEvent deviceDeletedEvent)
        {
            var device = deviceDeletedEvent.Device;
            Logger.LogMessage("Device deleted event: UserId: {0}", device.UserId);

            PurgeOldMediaRequestEvent purgeOldMediaRequest = new PurgeOldMediaRequestEvent
            {
                UpTo = DateTime.UtcNow,
                User = deviceDeletedEvent.Device
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