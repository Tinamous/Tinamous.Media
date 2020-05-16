using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AnalysisUK.Tinamous.Media.Domain.Documents;

namespace AnalysisUK.Tinamous.Media.DataAccess.Interfaces.Repositories
{
    public interface IMediaRepository : IRepository<MediaItem>
    {
        Task<List<MediaItem>> LoadByUserAsync(Guid userId, DateTime startDate, DateTime endDate);
        Task<List<MediaItem>> LoadByUniqueNameAsync(Guid accountId, string uniqueName, bool decending);
        Task DeleteAsync(MediaItem item);
    }
}