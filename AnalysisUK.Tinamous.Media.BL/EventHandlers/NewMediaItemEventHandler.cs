using System;
using AnalysisUK.Tinamous.Media.Domain.Configuration;
using AnalysisUK.Tinamous.Media.Domain.Logging;
using AnalysisUK.Tinamous.Media.Messaging.Events;
using EasyNetQ;

namespace AnalysisUK.Tinamous.Media.BL.EventHandlers
{
    public class NewMediaItemEventHandler : IDisposable
    {
        private readonly IBus _bus;
        private readonly IMediaService _mediaService;
        private IDisposable _consumer;

        public NewMediaItemEventHandler(IBus bus, IMediaService mediaService)
        {
            if (bus == null) throw new ArgumentNullException("bus");

            _bus = bus;
            _mediaService = mediaService;

            InitializeMessaging();
        }

        private void InitializeMessaging()
        {
            string subscriptionId = string.Format("Media.{0}", ServerSettings.ServerName);
            _consumer = _bus.Subscribe<INewMediaItemEvent>(subscriptionId, OnMessage);
        }

        /// <summary>
        /// Update cache across multiple servers.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public void OnMessage(INewMediaItemEvent obj)
        {
            Logger.LogMessage("New Media to cache : {0}", obj.MediaItemId);

            var mediaItemDto = obj.Item;
            _mediaService.UpdateUserCache(mediaItemDto.Id, mediaItemDto.User.UserId);
            _mediaService.UpdateUniqueNameCache(mediaItemDto.Id, mediaItemDto.User.AccountId, mediaItemDto.UniqueMediaName);
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