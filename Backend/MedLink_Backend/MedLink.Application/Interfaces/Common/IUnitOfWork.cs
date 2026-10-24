

namespace MedLink.Application.Interfaces.Common
{
    public interface IUnitOfWork : IDisposable
    {
        IGenaricRepo<T> Repository<T>() where T : class;

        Task<int> SaveChangesAsync();

        Task BeginTransactionAsync();

        Task CommitTransactionAsync();

        Task RollbackTransactionAsync();
    }
}
