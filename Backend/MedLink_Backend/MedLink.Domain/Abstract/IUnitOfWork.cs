using System;
using System.Collections.Generic;
using System.Text;

namespace MedLink.Domain.Abstract
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
