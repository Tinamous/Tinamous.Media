using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AnalysisUK.Tinamous.Media.Domain.Documents;

namespace AnalysisUK.Tinamous.Media.DataAccess.Interfaces.Repositories
{
    public interface IUniqueNameRepository
    {
        Task<List<UniqueName>> ListAsync(Guid accountId, int start, int limit);
        Task InsertAsync(UniqueName name);
    }
}