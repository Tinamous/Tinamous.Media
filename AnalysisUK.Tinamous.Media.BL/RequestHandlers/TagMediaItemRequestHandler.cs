using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using AnalysisUK.Tinamous.Media.Domain.Documents;
using AnalysisUK.Tinamous.Media.Domain.Logging;
using AnalysisUK.Tinamous.Media.Messaging.Dtos;
using AnalysisUK.Tinamous.Media.Messaging.Events;
using AnalysisUK.Tinamous.Media.Messaging.Requests;
using AutoMapper;
using EasyNetQ;

namespace AnalysisUK.Tinamous.Media.BL.RequestHandlers
{
    /// <summary>
    /// Update the tags (add/remove) from a media item.
    /// </summary>
    public class TagMediaItemRequestHandler : IDisposable
    {
        private readonly IBus _bus;
        private readonly IMediaService _mediaService;
        private IDisposable _consumer;

        public TagMediaItemRequestHandler(IBus bus, IMediaService mediaService)
        {
            if (bus == null) throw new ArgumentNullException("bus");

            _bus = bus;
            _mediaService = mediaService;

            InitializeMessaging();
        }

        private void InitializeMessaging()
        {
            string subscriptionId = "Media.TagMediaItem";
            _consumer = _bus.SubscribeAsync<TagMediaItemRequest>(subscriptionId, TagMediaItemAsync);
        }

        public async Task TagMediaItemAsync(TagMediaItemRequest request)
        {
            Logger.LogMessage("TagMediaItem for {0}", request.Id);

            var stopwatch = Stopwatch.StartNew();

            try
            {
                MediaItem mediaItem = await _mediaService.LoadAsync(request.Id);
                if (mediaItem.AccountId != request.User.AccountId)
                {
                    throw new Exception("Access Denied");
                }

                var tags = mediaItem.Tags ?? new List<string>();
                var tagsAdded = new List<string>();

                foreach (var tag in request.AddTags ?? new List<string>())
                {
                    if (!tags.Contains(tag))
                    {
                        tags.Add(tag);
                        tagsAdded.Add(tag);
                    }
                }

                foreach (var tag in request.RemoveTags ?? new List<string>())
                {
                    if (tags.Contains(tag))
                    {
                        tags.Remove(tag);
                    }
                }

                mediaItem.Tags = tags;

                await _mediaService.SaveAsync(mediaItem);

                await PublishTagsUpdated(mediaItem, tagsAdded);
            }
            finally
            {
                stopwatch.Stop();
                Logger.LogMessage("TagMediaItem took: {0}", stopwatch.ElapsedMilliseconds);
            }
        }

        private async Task PublishTagsUpdated(MediaItem mediaItem, List<string> tagsAdded)
        {
            MediaItemTaggedEvent taggedEvent = new MediaItemTaggedEvent
            {
                TagsAdded = tagsAdded,
                Date = DateTime.UtcNow,
                Item = Mapper.Map<MediaItemDto>(mediaItem)
            };
            await _bus.PublishAsync(taggedEvent);
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