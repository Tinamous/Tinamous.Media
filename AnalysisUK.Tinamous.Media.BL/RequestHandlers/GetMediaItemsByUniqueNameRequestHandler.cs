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
    public class GetMediaItemsByUniqueNameRequestHandler : IDisposable
    {
        private readonly IBus _bus;
        private readonly IMediaService _mediaService;
        private IDisposable _consumer;

        public GetMediaItemsByUniqueNameRequestHandler(IBus bus, IMediaService mediaService)
        {
            if (bus == null) throw new ArgumentNullException("bus");

            _bus = bus;
            _mediaService = mediaService;

            InitializeMessaging();
        }

        private void InitializeMessaging()
        {
            _consumer = _bus.RespondAsync<GetMediaItemsByUniqueNameRequest, GetMediaItemsByUniqueNameResponse>(GetMediaItemsByUniqueNameAsync);
        }

        public async Task<GetMediaItemsByUniqueNameResponse> GetMediaItemsByUniqueNameAsync(GetMediaItemsByUniqueNameRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            Logger.LogMessage("GetMediaItemsByUniqueName Unique Name: '{0}'", request.UniqueName);

            try
            {
                List<MediaItem> mediaItems = await GetMediaItems(request);

                return new GetMediaItemsByUniqueNameResponse
                {
                    MediaItems = Mapper.Map<List<MediaItemDto>>(mediaItems),
                };

            }
            finally
            {
                stopwatch.Stop();
                Logger.LogMessage("GetMediaItemsByUniqueName took: {0}", stopwatch.ElapsedMilliseconds);
            }
        }

        private async Task<List<MediaItem>> GetMediaItems(GetMediaItemsByUniqueNameRequest request)
        {
            Guid accountId = request.User.AccountId;
            Guid requestingUserId = request.User.UserId;

            if (request.LatestOnly)
            {
                var item = await _mediaService.LoadLatestByUniqueNameAsync(accountId, requestingUserId, request.UniqueName);
                return new List<MediaItem> { item };
            }
            return await _mediaService.LoadByUniqueNameAsync(accountId, requestingUserId, request.UniqueName, request.Start, request.Limit);
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