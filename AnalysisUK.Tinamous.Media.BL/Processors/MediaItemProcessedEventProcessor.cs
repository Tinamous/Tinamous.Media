using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AnalysisUK.Tinamous.Media.DataAccess.Interfaces.Repositories;
using AnalysisUK.Tinamous.Media.Domain.Documents;
using AnalysisUK.Tinamous.Media.Domain.Logging;
using AnalysisUK.Tinamous.Media.Messaging.Dtos;
using AnalysisUK.Tinamous.Media.Messaging.Events;
using AnalysisUK.Tinamous.Messaging.Common.Dtos;
using AutoMapper;
using EasyNetQ;

namespace AnalysisUK.Tinamous.Media.BL.Processors
{
    /// <summary>
    /// Final stage in the addition of media items workflow.
    /// 
    /// 1) AddMediaItemRequest -> Saves initial Media properties -> ProcessMediaItemRequestEvent
    /// 2) ProcessMediaItemRequestEvent -> Transforms media -> MediaItemProcessedEvent (calls this class.)
    /// 3) (this class) MediaItemProcessedEvent -> Saves transformations -> INewMediaItemEvent (subclass based on media)
    /// </summary>
    public class MediaItemProcessedEventProcessor : IDisposable
    {
        private readonly IBus _bus;
        private readonly IMediaService _mediaService;
        private readonly IFileStore _fileStore;
        private ISubscriptionResult _consumer;

        /// <summary>
        /// Just need S3 interface to load/save the item
        /// </summary>
        /// <param name="bus"></param>
        /// <param name="mediaService"></param>
        public MediaItemProcessedEventProcessor(IBus bus,
            IMediaService mediaService,
            IFileStore fileStore)
        {
            _bus = bus;
            _mediaService = mediaService;
            _fileStore = fileStore;

            InitializeMessaging();
        }

        private void InitializeMessaging()
        {
            _consumer = _bus.SubscribeAsync<MediaItemProcessedEvent>("Media", OnRequestAsync);
        }

        public async Task OnRequestAsync(MediaItemProcessedEvent request)
        {
            Logger.LogMessage("MediaItemProcessedEventProcessor Media Item Request. Id: {0}", request.Id);

            Stopwatch stopWatch = Stopwatch.StartNew();
            try
            {
                MediaItem mediaItem = await _mediaService.LoadAsync(request.Id);

                if (mediaItem == null)
                {
                    throw new Exception("Did not find media item to update");
                }

                await DeleteUploadedImageAsync(mediaItem);

                foreach (var mediaItemStorageLocationDto in request.StorageLocations)
                {
                    mediaItem.StorageLocations.Add(Mapper.Map<MediaItemStorageLocation>(mediaItemStorageLocationDto));
                }

                // Update the history type
                mediaItem.HistoryType = MediaHistoryType.History;
                await _mediaService.SaveAsync(mediaItem);

                // TODO: Also add/update a "Latest" type for the device
                // to allow quick access to the devices latest media?

                await RaiseNewMediaItem(mediaItem);

            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error updating media item with processed locations");
            }
            finally
            {
                stopWatch.Stop();
                Logger.LogMessage("Metid Item Processed Event for {0} took {1}ms", request.Id, stopWatch.ElapsedMilliseconds);
            }
        }

        private async Task DeleteUploadedImageAsync(MediaItem mediaItem)
        {
            MediaItemStorageLocation storageLocation = mediaItem.StorageLocations.First();
            Logger.LogMessage("Deleting original uploaded image: {0}", storageLocation.Filename);
            await _fileStore.DeleteAsync(storageLocation);
            mediaItem.StorageLocations.Clear();
        }

        /// <summary>
        /// Raise a new media item event
        /// </summary>
        /// <param name="mediaItem"></param>
        /// <returns></returns>
        private async Task RaiseNewMediaItem(MediaItem mediaItem)
        {
            // TODO: Figure out media type image/audio/video/pdf/other?
            INewMediaItemEvent newMediaEvent = new NewImageEvent
            {
                MediaItemId = mediaItem.Id,
                UniqueMediaName = mediaItem.UniqueMediaName,
                Date = DateTime.UtcNow,
                PublishedBy = new UserSummaryDto { AccountId = mediaItem.AccountId, UserId = mediaItem.UserId},
                Item = Mapper.Map<MediaItemDto>(mediaItem),
            };
            await _bus.PublishAsync(newMediaEvent);
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