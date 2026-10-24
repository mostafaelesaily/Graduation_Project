using MedLink.Application.Interfaces.Common;
using MedLink.infrastructure.Data;
using MedLink.infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace MedLink.infrastructure.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext appDbContext;
        private IDbContextTransaction? transaction;
    public UnitOfWork(AppDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
        }

        public async Task BeginTransactionAsync()
        {
            if (transaction is not null)
            {
                throw new InvalidOperationException(
                    "A transaction is already active.");
            }

            transaction = await appDbContext.Database
                .BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (transaction is null)
            {
                throw new InvalidOperationException(
                    "No active transaction.");
            }

            try
            {
                await appDbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
            finally
            {
                await transaction.DisposeAsync();
                transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (transaction is null)
            {
                return;
            }

            try
            {
                await transaction.RollbackAsync();
            }
            finally
            {
                await transaction.DisposeAsync();
                transaction = null;
            }
        }

        public IGenaricRepo<T> Repository<T>() where T : class
        {
            return new MainGenaricRepo<T>(appDbContext);
        }

        public Task<int> SaveChangesAsync()
        {
            return appDbContext.SaveChangesAsync();
        }

        public void Dispose()
        {
            appDbContext.Dispose();
            transaction?.Dispose();
        }
    }

}
