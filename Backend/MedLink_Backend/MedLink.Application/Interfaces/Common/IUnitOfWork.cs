

namespace MedLink.Application.Interfaces.Common
{
    public interface IUnitOfWork : IDisposable
    {
        IGenaricRepo<T> Repository<T>() where T : class;

        Task<int> SaveChangesAsync();

        Task BeginTransactionAsync(System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.ReadCommitted);

        Task CommitTransactionAsync();

        Task RollbackTransactionAsync();
    }
}
