using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnalysisUK.Tinamous.Media.DataAccess.Aws.Repositories;
using AnalysisUK.Tinamous.Media.DataAccess.Interfaces.Repositories;
using AnalysisUK.Tinamous.Media.Domain.Documents;
using AnalysisUK.Tinamous.Media.Domain.Helpers;
using AnalysisUK.Tinamous.Media.Domain.Logging;
using AnalysisUK.Tinamous.Media.Messaging.Events;
using EasyNetQ;

namespace AnalysisUK.Tinamous.Media.BL
{
    public class MediaService : IMediaService
    {
        private readonly IBus _bus;
        private readonly IMediaRepository _mediaRepository;
        private readonly UniqueNameRepository _uniquNameRepository;
        private readonly IFileStore _fileStore;
        private static readonly Dictionary<Guid, Guid> UserMediaItemsCache = new Dictionary<Guid, Guid>();
        private static readonly Dictionary<string, Guid> UniqueNameMediaItemsCache = new Dictionary<string, Guid>();
        private static readonly Dictionary<Guid, MediaItem> MediaItemsCache = new Dictionary<Guid, MediaItem>();

        public MediaService(IBus bus, 
            IMediaRepository mediaRepository, 
            UniqueNameRepository uniquNameRepository,
            IFileStore fileStore
            )
        {
            _bus = bus;
            _mediaRepository = mediaRepository;
            _uniquNameRepository = uniquNameRepository;
            _fileStore = fileStore;
        }


        public async Task SaveAsync(MediaItem mediaItem)
        {
            // If the media item doesn't have a delete after, then create one
            // to ensure images are purged 
            if (!mediaItem.DeleteAfter.HasValue)
            {
                DateTime.UtcNow.AddMonths(6).ToLongUnixSeconds();
            }

            await _mediaRepository.SaveAsync(mediaItem);
            
            if (!string.IsNullOrEmpty(mediaItem.UniqueMediaName))
            {
                await AddUniqueNameAsync(mediaItem);
            }

            Cache(mediaItem);

            // NewMediaItem event raised from event handler for processed item.
        }

        private async Task AddUniqueNameAsync(MediaItem item)
        {
            // TODO: Cache this and update on new media item events.
            List<UniqueName> existingUniqueNames = await GetUniqueNamesAsync(item.AccountId);

            // Case sensitive
            if (existingUniqueNames.Select(x => x.LowerName).Contains(item.UniqueMediaName.ToLower()))
            {
                // Unique name already exists.
                return;
            }

            UniqueName name = new UniqueName
            {
                AccountId = item.AccountId,
                Name = item.UniqueMediaName,
                LowerName = item.UniqueMediaName.ToLower(),
                DateAdded = DateTime.UtcNow,
            };
            await _uniquNameRepository.InsertAsync(name);
        }

        public async Task<MediaItem> LoadAsync(Guid id)
        {
            var item = GetMediaItemFromCache(id);
            if (item != null)
            {
                Logger.LogMessage("Found media item in cache. Id: {0}", id);
                return item;
            }

            Logger.LogMessage("Loading media item by Id: {0}", id);

            // Cache miss. Load from DB and cache it.
            return Cache(await _mediaRepository.LoadAsync(id));
        }

        public async Task DeleteAsync(MediaItem item)
        {
            item.Deleted = true;
            item.LastUpdated = DateTime.UtcNow;

            // Delete the files from S3!
            List<Task> tasks = new List<Task>();
            foreach (var mediaItemStorageLocation in item.StorageLocations)
            {
                tasks.Add(_fileStore.DeleteAsync(mediaItemStorageLocation));    
            }

            // Wait for all the delete requests.
            Task.WaitAll(tasks.ToArray(), 60000);
            
            //await SaveAsync(item);
            await _mediaRepository.DeleteAsync(item);

            RemoveMediaItemCache(item);

            await PublishMediaItemDeletedAsync(item);
        }

        public async Task<List<MediaItem>> LoadByUserAsync(Guid userId, 
            Guid requestingUserId,
            DateTime? requestFromDate, 
            DateTime? requestToDate, 
            int start, 
            int limit)
        {
            DateTime from = requestFromDate ?? DateTime.MinValue;
            DateTime to = requestToDate?? DateTime.UtcNow;

            var mediaItems = await _mediaRepository.LoadByUserAsync(userId, from, to);

            mediaItems = FilterPrivateMedia(mediaItems, requestingUserId);

            return mediaItems.Skip(start).Take(limit).ToList();
        }

        public async Task<MediaItem> LoadLatestByUserAsync(Guid userId, Guid requestingUserId)
        {
            Guid? id = GetItemIdFromUserId(userId);

            var item = await GetMediaItemByCachedIdAsync(requestingUserId, id);
            if (item != null)
            {
                return item;
            }

            Logger.LogWarn("Failed to load media item from cache.");

            // Limit range to the last 14 days (TODO: This should be device reporting days).
            // outsite that 
            DateTime endDate = DateTime.UtcNow;
            DateTime startDate = DateTime.MinValue; // endDate.AddDays(-14);
            var mediaItems = await _mediaRepository.LoadByUserAsync(userId, startDate, endDate);

            mediaItems = FilterPrivateMedia(mediaItems, requestingUserId);

            var lastMediaItem = mediaItems
                .Where(x=>x.Deleted == false)
                .OrderByDescending(x=>x.DateAdded)
                .FirstOrDefault();

            if (lastMediaItem == null)
            {
                Logger.LogWarn("No media item returned for LoadLatestByUser.");
                return null;
            }

            UpdateUserCache(lastMediaItem.Id, lastMediaItem.UserId);
            return Cache(lastMediaItem);
        }

        /// <summary>
        /// Get the list of unique member names
        /// </summary>
        /// <returns></returns>
        public async Task<List<UniqueName>> GetUniqueNamesAsync(Guid accountId)
        {
            return await _uniquNameRepository.ListAsync(accountId, 0, 10000);
        }

        #region Cache

        private MediaItem Cache(MediaItem mediaItem)
        {
            // Don't update cache on these because
            // they may come from a load and be older
            // than the latest.
            //UpdateUserCache(mediaItem.Id, mediaItem.UserId);
            //UpdateUniqueNameCache(mediaItem.Id, mediaItem.AccountId, mediaItem.UniqueMediaName);
            UpdateMediaItemCache(mediaItem);
            return mediaItem;
        }

        public void UpdateUserCache(Guid id, Guid userId)
        {
            lock (UserMediaItemsCache)
            {
                if (UserMediaItemsCache.ContainsKey(userId))
                {
                    UserMediaItemsCache[userId] = id;
                }
                else
                {
                    UserMediaItemsCache.Add(userId, id);
                }
            }
        }

        private Guid? GetItemIdFromUserId(Guid userId)
        {
            lock (UserMediaItemsCache)
            {
                if (UserMediaItemsCache.ContainsKey(userId))
                {
                    return UserMediaItemsCache[userId];
                }
            }
            return null;
        }

        public void UpdateUniqueNameCache(Guid id, Guid accountId, string uniqueName)
        {
            if (string.IsNullOrWhiteSpace(uniqueName))
            {
                return;
            }

            lock (UniqueNameMediaItemsCache)
            {
                string key = string.Format("{0}-{1}", accountId, uniqueName);
                if (UniqueNameMediaItemsCache.ContainsKey(key))
                {
                    UniqueNameMediaItemsCache[key] = id;
                }
                else
                {
                    UniqueNameMediaItemsCache.Add(key, id);
                }
            }
        }

        public Guid? GetItemIdFromUniqueName(Guid accountId, string uniqueName)
        {
            string key = string.Format("{0}-{1}", accountId, uniqueName);
            lock (UniqueNameMediaItemsCache)
            {
                if (UniqueNameMediaItemsCache.ContainsKey(key))
                {
                    return UniqueNameMediaItemsCache[key];
                }
            }
            return null;
        }

        private void UpdateMediaItemCache(MediaItem mediaItem)
        {
            lock (MediaItemsCache)
            {
                // Limit the cache to the latest 5000 items.
                if (MediaItemsCache.Count > 5000)
                {
                    MediaItemsCache.Remove(MediaItemsCache.FirstOrDefault().Key);
                }

                Guid key = mediaItem.Id;
                if (MediaItemsCache.ContainsKey(key))
                {
                    // Update
                    MediaItemsCache[key] = mediaItem;
                }
                else
                {
                    MediaItemsCache.Add(key, mediaItem);
                }
            }
        }

        private void RemoveMediaItemCache(MediaItem mediaItem)
        {
            lock (MediaItemsCache)
            {
                Guid key = mediaItem.Id;
                if (MediaItemsCache.ContainsKey(key))
                {
                    // Update
                    MediaItemsCache.Remove(mediaItem.Id);
                }
            }
        }

        private MediaItem GetMediaItemFromCache(Guid id)
        {
            lock (MediaItemsCache)
            {
                if (!MediaItemsCache.ContainsKey(id))
                {
                    return null;
                }
                return MediaItemsCache[id];
            }
        }

        #endregion

        /// <summary>
        /// Load history list of media items by unique name
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="requestingUserId"></param>
        /// <param name="uniqueName"></param>
        /// <param name="start"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async Task<List<MediaItem>> LoadByUniqueNameAsync(Guid accountId, Guid requestingUserId, string uniqueName, int start, int limit)
        {
            // Decending order (newest first).
            var mediaItems = await _mediaRepository
                .LoadByUniqueNameAsync(accountId, uniqueName, true);

            return FilterPrivateMedia(mediaItems, requestingUserId);
        }

        /// <summary>
        /// Load the latest from the uniqie name
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="requestingUserId"></param>
        /// <param name="uniqueName"></param>
        /// <returns></returns>
        public async Task<MediaItem> LoadLatestByUniqueNameAsync(Guid accountId, Guid requestingUserId, string uniqueName)
        {
            Guid? id = GetItemIdFromUniqueName(accountId, uniqueName);
    
            var item = await GetMediaItemByCachedIdAsync(requestingUserId, id);
            if (item != null)
            {
                return item;
            }

            var mediaItems = await _mediaRepository
                .LoadByUniqueNameAsync(accountId, uniqueName, true);

            return FilterPrivateMedia(mediaItems, requestingUserId).FirstOrDefault();
        }

        #region Helpers

        private List<MediaItem> FilterPrivateMedia(List<MediaItem> mediaItems, Guid requestingUserId)
        {
            int itemsRemoved = mediaItems.RemoveAll(x => x.Private && x.UserId != requestingUserId);
            if (itemsRemoved > 0)
            {
                Logger.LogMessage("Removed {0} media items marked private.", itemsRemoved);
            }
            return mediaItems;
        }

        private async Task<MediaItem> GetMediaItemByCachedIdAsync(Guid requestingUserId, Guid? id)
        {
            if (!id.HasValue)
            {
                return null;
            }

            // TODO: Check to see if item is in local cache first.
            // may not be due to distributed nature.
            MediaItem mediaItem = await LoadAsync(id.Value);

            if (mediaItem != null)
            {
                if (mediaItem.Deleted)
                {
                    return null;
                }

                if (mediaItem.Private && requestingUserId == mediaItem.UserId)
                {
                    // Media item is flagged as private, but not owned by 
                    return null;
                }
            }
            return mediaItem;
        }

        #endregion

        #region Publishing

        private Task PublishMediaItemDeletedAsync(MediaItem item)
        {
            var itemDeleted = new MediaItemDeletedEvent
            {
                id = item.Id,
                AccountId = item.AccountId
            };
            return _bus.PublishAsync(itemDeleted);
        }

        #endregion
    }
}