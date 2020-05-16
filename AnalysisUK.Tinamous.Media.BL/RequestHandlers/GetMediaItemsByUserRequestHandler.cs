using System;
using System.Collections.Generic;
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
    public class GetMediaItemsByUserRequestHandler : IDisposable
    {
        private readonly IBus _bus;
        private readonly IMediaService _mediaService;
        private IDisposable _consumer;

        public GetMediaItemsByUserRequestHandler(IBus bus, IMediaService mediaService)
        {
            if (bus == null) throw new ArgumentNullException("bus");

            _bus = bus;
            _mediaService = mediaService;

            InitializeMessaging();
        }

        private void InitializeMessaging()
        {
            _consumer = _bus.RespondAsync<GetMediaItemsByUserRequest, GetMediaItemsByUserResponse>(GetMediaItemsByUserAsync);
        }

        public async Task<GetMediaItemsByUserResponse> GetMediaItemsByUserAsync(GetMediaItemsByUserRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            Logger.LogMessage("GetMediaItemsByUser : {0}", request.User.UserId);

            try
            {
                // Do work here...
                List<MediaItem> mediaItems = await _mediaService.LoadByUserAsync(request.User.UserId,
                    request.RequestingUser.UserId,
                    request.FromDate,
                    request.ToDate,
                    request.Start,
                    request.Limit);

                return new GetMediaItemsByUserResponse
                {
                    MediaItems = Mapper.Map<List<MediaItemDto>>(mediaItems),
                };
            }
            finally
            {
                stopwatch.Stop();
                Logger.LogMessage("GetMediaItemsByUser took: {0}", stopwatch.ElapsedMilliseconds);
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