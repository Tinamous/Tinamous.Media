using System;
using System.Diagnostics;
using System.Security;
using System.Threading.Tasks;
using AnalysisUK.Tinamous.Media.Domain.Logging;
using AnalysisUK.Tinamous.Media.Messaging.Dtos;
using AnalysisUK.Tinamous.Media.Messaging.Requests;
using AutoMapper;
using EasyNetQ;

namespace AnalysisUK.Tinamous.Media.BL.RequestHandlers
{
    public class GetMediaItemByIdRequestHandler : IDisposable
    {
        private readonly IBus _bus;
        private readonly IMediaService _mediaService;
        private IDisposable _consumer;

        public GetMediaItemByIdRequestHandler(IBus bus, 
            IMediaService mediaService)
        {
            if (bus == null) throw new ArgumentNullException("bus");

            _bus = bus;
            _mediaService = mediaService;

            InitializeMessaging();
        }

        private void InitializeMessaging()
        {
            _consumer = _bus.RespondAsync<GetMediaItemByIdRequest, GetMediaItemByIdResponse>(GetMediaItemByIdAsync);
        }

        public async Task<GetMediaItemByIdResponse> GetMediaItemByIdAsync(GetMediaItemByIdRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            Logger.LogMessage("GetMediaItemById : {0}", request.Id);

            try
            {
                // Do work here...
                var mediaItem = await _mediaService.LoadAsync(request.Id);

                // Different users should beable to view media
                if (mediaItem.AccountId != request.User.AccountId)
                {
                    throw new SecurityException("Media item belongs to different account");
                }

                return new GetMediaItemByIdResponse
                {
                    MediaItem = Mapper.Map<MediaItemDto>(mediaItem)
                };
            }
            finally
            {
                stopwatch.Stop();
                Logger.LogMessage("GetMediaItemById took: {0}", stopwatch.ElapsedMilliseconds);
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