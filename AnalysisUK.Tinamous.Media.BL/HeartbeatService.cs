using System;
using System.Reflection;
using System.Threading;
using AnalysisUK.Tinamous.Media.Domain.Logging;
using AnalysisUK.Tinamous.Messaging.Common.Dtos.System;
using AnalysisUK.Tinamous.Messaging.Common.Events.System;
using EasyNetQ;

namespace AnalysisUK.Tinamous.Media.BL
{
    public class HeartbeatService : IHeartbeatService
    {
        private readonly IBus _bus;
        private readonly string _machineName;
        private Timer _timer;
        private const int TimerIntervalSeconds = 10;
        private bool _enabled;
        private readonly string _version;

        public HeartbeatService(IBus bus, string machineName)
        {
            _bus = bus;
            _machineName = machineName;
            _timer = new Timer(OnTimerTick, null, TimerIntervalSeconds * 1000, TimerIntervalSeconds * 1000);
            _version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private void OnTimerTick(object state)
        {
            if (!_enabled)
            {
                return;
            }

            try
            {
                // Publish a heartbeat for this service.
                var heartBeatEvent = new HeartBeatEvent
                {
                    IntervalSeconds = TimerIntervalSeconds,
                    Server = _machineName,
                    Service = "Media",
                    Time = DateTime.UtcNow,
                    MachineInfo = new MachineInfo(),
                    ProcessInfo = new ProcessInfo(),
                    SoftwareVersion = _version,
                    IsMaster = false,
                };
                _bus.PublishAsync(heartBeatEvent);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error publishing heartbeat. Is the RabbitMQ service available?");
            }
        }

        public void Dispose()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }

        public void Start()
        {
            _enabled = true;
        }

        public void Stop()
        {
            _enabled = false;
        }
    }
}