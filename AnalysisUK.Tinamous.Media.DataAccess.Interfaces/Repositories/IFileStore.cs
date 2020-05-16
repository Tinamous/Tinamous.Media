using System.IO;
using System.Threading.Tasks;
using AnalysisUK.Tinamous.Media.Domain.Documents;

namespace AnalysisUK.Tinamous.Media.DataAccess.Interfaces.Repositories
{
    public interface IFileStore
    {
        Task<Stream> LoadStreamAsync(MediaItemStorageLocation storageLocation);
        Task SaveAsync(MediaItemStorageLocation storageLocation, Stream stream);
        Task DeleteAsync(MediaItemStorageLocation mediaItemStorageLocation);
    }
}