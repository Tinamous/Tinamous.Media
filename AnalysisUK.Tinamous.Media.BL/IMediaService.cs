using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AnalysisUK.Tinamous.Media.Domain.Documents;

namespace AnalysisUK.Tinamous.Media.BL
{
    public interface IMediaService
    {
        Task SaveAsync(MediaItem mediaItem);

        Task<MediaItem> LoadAsync(Guid id);

        Task DeleteAsync(MediaItem item);

        Task<List<MediaItem>> LoadByUserAsync(Guid userId, Guid requestingUserId, DateTime? requestFromDate,
            DateTime? requestToDate, int start,
            int limit);
        Task<MediaItem> LoadLatestByUserAsync(Guid publishedByUserId, Guid requestingUserId);
        
        Task<MediaItem> LoadLatestByUniqueNameAsync(Guid accountId, Guid requestingUserId, string uniqueName);
        Task<List<MediaItem>> LoadByUniqueNameAsync(Guid accountId, Guid requestingUserId, string uniqueName, int start, int limit);

        Task<List<UniqueName>> GetUniqueNamesAsync(Guid accountId);

        void UpdateUserCache(Guid id, Guid userId);

        void UpdateUniqueNameCache(Guid id, Guid accountId, string uniqueName);
    }
}