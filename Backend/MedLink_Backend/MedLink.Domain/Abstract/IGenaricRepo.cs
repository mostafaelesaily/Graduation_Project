using System;
using System.Collections.Generic;
using System.Text;

namespace MedLink.Domain.Abstract
{
    public interface IGenaricRepo<T> where T : class
    {
        Task<T?> GetByIdAsync(params object[] id);
        Task<IReadOnlyList<T>> GetAllAsync();
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}
