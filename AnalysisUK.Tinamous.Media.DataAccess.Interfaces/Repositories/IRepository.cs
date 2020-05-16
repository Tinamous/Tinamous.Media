using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnalysisUK.Tinamous.Media.DataAccess.Interfaces.Repositories
{
    public interface IRepository<T>
    {
        Task SaveAsync(T item);
        //Task<IList<T>> LoadAsync(IUnitOfWork unitOfWork);
        //Task<IQueryable<T>> LoadAllAsync(IUnitOfWork unitOfWork);
        Task<T> LoadAsync(Guid id);
    }
}