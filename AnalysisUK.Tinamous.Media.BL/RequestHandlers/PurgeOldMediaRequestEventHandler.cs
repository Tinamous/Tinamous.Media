using System;
using System.Threading.Tasks;
using AnalysisUK.Tinamous.Media.DataAccess.Interfaces.Repositories;
using AnalysisUK.Tinamous.Media.Domain.Logging;
using AnalysisUK.Tinamous.Media.Messaging.Events;
using EasyNetQ;

namespace AnalysisUK.Tinamous.Media.BL.RequestHandlers
{
    public class PurgeOldMediaRequestEventHandler : IDisposable
    {
        private readonly IBus _bus;
        private readonly IMediaService _mediaService;
        private readonly IFileStore _fileStore;
        private readonly string _rawImageBucket;
        private ISubscriptionResult _consumer;

        public PurgeOldMediaRequestEventHandler(IBus bus,
            IMediaService mediaService,
            IFileStore fileStore)
        {
            _bus = bus;
            _mediaService = mediaService;
            _fileStore = fileStore;
            InitializeMessaging();
        }

        // CreateImageRequestEvent
        private void InitializeMessaging()
        {
            _consumer = _bus.SubscribeAsync<PurgeOldMediaRequestEvent>("Media", OnRequest);
        }

        public async Task OnRequest(PurgeOldMediaRequestEvent request)
        {
            Logger.LogMessage("Purge media update: {0}, User: {1}", request.UpTo, request.User.UserId);

            var userId = request.User.UserId;

            // Load all the media items before the specific date to delete.
            var mediaItems = await _mediaService.LoadByUserAsync(userId, userId, DateTime.MinValue, request.UpTo, 0, 1000);
            Logger.LogMessage("Found {0} media items to purge for user: {1}", mediaItems.Count, userId);

            if (mediaItems.Count > 1000)
            {
                Logger.LogWarn("Found a lot ({0} of historical media items purging for user: {1}", mediaItems.Count, userId);
            }

            foreach (var mediaItem in mediaItems)
            {
                Logger.LogMessage("Deleting item {0}. User: {1}", mediaItem.Id, mediaItem.UserId);
                await _mediaService.DeleteAsync(mediaItem);
            }

            // This limits the max items that can be deleted to 100,000, which hopefully is enough.
            // the prevents a possible never ending situation of purging if it's going wrong.
            if (mediaItems.Count >= 1000 && request.Attempt < 100)
            {
                request.Attempt++;

                Logger.LogWarn("Found to many media items to delete in one go. Sending another request... Attempt: {0}", request.Attempt);
                // Republish the request as theirs still more to delete!
                await _bus.PublishAsync(request);
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