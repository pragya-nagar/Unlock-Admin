using System.Threading;
using System.Threading.Tasks;

namespace OKRAdminService.EF
{
    public interface IDataContextAsync : IDataContext
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        Task<int> SaveChangesAsync();
    }
}
