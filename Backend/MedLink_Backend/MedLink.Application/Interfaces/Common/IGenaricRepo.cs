using System;
using System.Collections.Generic;
using System.Text;

namespace MedLink.Application.Interfaces.Common
{
    public interface IGenaricRepo<T> where T : class
    {
        Task<T?> GetByIdAsync(params object[] id);
        Task<IReadOnlyList<T>> GetAllAsync();
        Task<IReadOnlyList<T>> GetAllAsync(
            ISpecification<T> specification);
        Task<T?> FindAsync(ISpecification<T>
            specification);
        Task<int> CountAsync(ISpecification<T> specification);
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}
