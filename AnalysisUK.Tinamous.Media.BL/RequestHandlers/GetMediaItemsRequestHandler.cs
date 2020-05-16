using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AnalysisUK.Tinamous.Media.Domain.Logging;
using AnalysisUK.Tinamous.Media.Messaging.Requests;
using EasyNetQ;

namespace AnalysisUK.Tinamous.Media.BL.RequestHandlers
{
    public class GetMediaItemsRequestHandler : IDisposable
    {
        private readonly IBus _bus;
        private readonly IMediaService _mediaService;
        private IDisposable _consumer;


        public GetMediaItemsRequestHandler(IBus bus, IMediaService mediaService)
        {
            if (bus == null) throw new ArgumentNullException("bus");


            _bus = bus;
            _mediaService = mediaService;


            InitializeMessaging();
        }

        private void InitializeMessaging()
        {
            _consumer = _bus.RespondAsync<GetMediaItemsRequest, GetMediaItemsResponse>(GetMediaItemsAsync);
        }

        public async Task<GetMediaItemsResponse> GetMediaItemsAsync(GetMediaItemsRequest request)
        {
            Logger.LogWarn("GetMediaItems not implemented");
            throw new NotImplementedException("GetMediaItems handler not implemented");

            var stopwatch = Stopwatch.StartNew();
            Logger.LogMessage("GetMediaItems : {0}", string.Join(",", request.Ids));

            try
            {

                // Do work here...

            }
            finally
            {
                stopwatch.Stop();
                Logger.LogMessage("GetMediaItems took: {0}", stopwatch.ElapsedMilliseconds);
            }

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