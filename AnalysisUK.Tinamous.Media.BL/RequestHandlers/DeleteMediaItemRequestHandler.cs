using System;
using System.Diagnostics;
using System.Security;
using System.Threading.Tasks;
using AnalysisUK.Tinamous.Media.Domain.Logging;
using AnalysisUK.Tinamous.Media.Messaging.Requests;
using EasyNetQ;

namespace AnalysisUK.Tinamous.Media.BL.RequestHandlers
{
    public class DeleteMediaItemRequestHandler : IDisposable
    {
        private readonly IBus _bus;
        private readonly IMediaService _mediaService;
        private IDisposable _consumer;

        public DeleteMediaItemRequestHandler(IBus bus, IMediaService mediaService)
        {
            if (bus == null) throw new ArgumentNullException("bus");

            _bus = bus;
            _mediaService = mediaService;

            InitializeMessaging();
        }

        private void InitializeMessaging()
        {
            _consumer = _bus.RespondAsync<DeleteMediaItemRequest, DeleteMediaItemResponse>(DeleteMediaItemAsync);
        }

        public async Task<DeleteMediaItemResponse> DeleteMediaItemAsync(DeleteMediaItemRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            Logger.LogMessage("DeleteMediaItem : {0}", request.Id);

            try
            {
                // Do work here...
                var mediaItem = await _mediaService.LoadAsync(request.Id);

                if (mediaItem == null)
                {
                    throw new Exception("Media item not found");
                }

                // Only block deletion of media item if it's private.
                // as we will most likely wish to delete media items
                // posted by different devices.
                if (mediaItem.Private)
                {
                    if (mediaItem.UserId != request.User.UserId)
                    {
                        throw new SecurityException("Media item belongs to another user");
                    }
                }

                await _mediaService.DeleteAsync(mediaItem);

                return new DeleteMediaItemResponse();
            }
            finally
            {
                stopwatch.Stop();
                Logger.LogMessage("DeleteMediaItem took: {0}", stopwatch.ElapsedMilliseconds);
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