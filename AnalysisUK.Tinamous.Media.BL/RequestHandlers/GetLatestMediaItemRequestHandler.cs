using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AnalysisUK.Tinamous.Media.Domain.Documents;
using AnalysisUK.Tinamous.Media.Domain.Logging;
using AnalysisUK.Tinamous.Media.Messaging.Dtos;
using AnalysisUK.Tinamous.Media.Messaging.Requests;
using AutoMapper;
using EasyNetQ;

namespace AnalysisUK.Tinamous.Media.BL.RequestHandlers
{
    public class GetLatestMediaItemRequestHandler : IDisposable
    {
        private readonly IBus _bus;
        private readonly IMediaService _mediaService;
        private IDisposable _consumer;

        public GetLatestMediaItemRequestHandler(IBus bus, 
            IMediaService mediaService)
        {
            if (bus == null) throw new ArgumentNullException("bus");

            _bus = bus;
            _mediaService = mediaService;

            InitializeMessaging();
        }

        private void InitializeMessaging()
        {
            _consumer = _bus.RespondAsync<GetLatestMediaItemRequest, GetLatestMediaItemResponse>(GetLatestMediaItemAsync);
        }

        public async Task<GetLatestMediaItemResponse> GetLatestMediaItemAsync(GetLatestMediaItemRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            Logger.LogMessage("GetLatestMediaItem for user: {0}", request.PublishedBy.UserId);

            try
            {
                MediaItem mediaItem = await _mediaService.LoadLatestByUserAsync(request.PublishedBy.UserId, request.RequestingUser.UserId);

                return new GetLatestMediaItemResponse
                {
                    MediaItem = Mapper.Map<MediaItemDto>(mediaItem),
                };
            }
            finally
            {
                stopwatch.Stop();
                Logger.LogMessage("GetLatestMediaItem took: {0}", stopwatch.ElapsedMilliseconds);
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