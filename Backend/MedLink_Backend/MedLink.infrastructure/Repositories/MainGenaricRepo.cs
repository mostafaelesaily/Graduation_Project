using MedLink.Domain.Abstract;
using MedLink.infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace MedLink.infrastructure.Repositories
{
    public class MainGenaricRepo<T> : IGenaricRepo<T> where T : class
    {
        public MainGenaricRepo(AppDbContext appDbContext)
        {
            dbset = appDbContext.Set<T>();
        }
        private readonly DbSet<T> dbset;
        public async Task<IReadOnlyList<T>> GetAllAsync()
        {
            return await dbset.AsNoTracking().ToListAsync();
        }

        public async Task<T?> GetByIdAsync(params object[] id)
        {
            return await dbset.FindAsync(new object?[] { id });
        }
        public async Task AddAsync(T entity)
        {
             await dbset.AddAsync(entity);
        }
        public void Update(T entity)
        {
            dbset.Update(entity);
        }
        public void Delete(T entity)
        {
            dbset.Remove(entity);
        }      
    }
}
