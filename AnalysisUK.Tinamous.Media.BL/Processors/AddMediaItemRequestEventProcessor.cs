using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AnalysisUK.Tinamous.Media.DataAccess.Interfaces;
using AnalysisUK.Tinamous.Media.Domain.Documents;
using AnalysisUK.Tinamous.Media.Messaging.Dtos;
using AnalysisUK.Tinamous.Media.Messaging.Events;
using AutoMapper;
using EasyNetQ;

namespace AnalysisUK.Tinamous.Media.BL.Processors
{
    /// <summary>
    /// First stage in the addition of media items workflow.
    /// 
    /// 1) AddMediaItemRequest -> Saves initial Media properties -> ProcessMediaItemRequestEvent
    /// 2) ProcessMediaItemRequestEvent -> Transforms media -> MediaItemProcessedEvent
    /// 3) MediaItemProcessedEvent -> Saves transformations -> INewMediaItemEvent (subclass based on media)
    /// </summary>
    public class AddMediaItemRequestEventProcessor : IDisposable
    {
        private readonly IBus _bus;
        private readonly IMembershipService _membershipService;
        private readonly IMediaService _mediaService;
        private ISubscriptionResult _consumer;

        public AddMediaItemRequestEventProcessor(IBus bus,
            IMembershipService membershipService,
            IMediaService mediaService)
        {
            _bus = bus;           
            _membershipService = membershipService;
            _mediaService = mediaService;
            InitializeMessaging();
        }

        private void InitializeMessaging()
        {
            _consumer = _bus.SubscribeAsync<AddMediaItemRequestEvent>("Media", OnRequest);
        }

        public async Task OnRequest(AddMediaItemRequestEvent request)
        {
            request.UniqueMediaName = request.UniqueMediaName ?? "";

            var mediaItem = new MediaItem
            {
                Id = request.Id,
                AccountId = request.User.AccountId,
                UserId = request.User.UserId,
                Caption = request.Caption,
                //ContentType = request. .ContentType,
                Location = Mapper.Map<LocationDetails>(request.Location),
                Tags = request.Tags,
                Description = request.Description,
                //OriginalStorageLocation = request.StorageLocation
                UniqueMediaName = request.UniqueMediaName,
                UniqueMediaKey = string.Format("{0}-{1}", request.User.AccountId, request.UniqueMediaName.ToLower()),
                StorageLocations = new List<MediaItemStorageLocation>
                {
                    Mapper.Map<MediaItemStorageLocation>(request.StorageLocation)
                },
                HistoryType = MediaHistoryType.Pending,
            };

            // After status post so the status post Id can be included for reference
            await _mediaService.SaveAsync(mediaItem);

            await PublishProcessMediaItemAsync(mediaItem, request.StorageLocation);
        }

        /// <summary>
        /// Publish a request to convert the media at the storage location
        /// into the appropriate media items (scaled down etc)
        /// </summary>
        /// <param name="mediaItem"></param>
        /// <param name="storageLocation"></param>
        /// <returns></returns>
        private async Task PublishProcessMediaItemAsync(MediaItem mediaItem, MediaItemStorageLocationDto storageLocation)
        {
            ProcessMediaItemRequestEvent requestEvent = new ProcessMediaItemRequestEvent
            {
                Id = mediaItem.Id,
                StorageLocation = storageLocation,
            };
            await _bus.PublishAsync(requestEvent);
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