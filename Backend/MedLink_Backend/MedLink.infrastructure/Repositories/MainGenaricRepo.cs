using MedLink.Application.Interfaces.Common;
using MedLink.infrastructure.Data;
using MedLink.infrastructure.Specifications;
using Microsoft.EntityFrameworkCore;

namespace MedLink.infrastructure.Repositories
{
    public class MainGenaricRepo<T> : IGenaricRepo<T>
        where T : class
    {
        private readonly DbSet<T> dbset;

        public MainGenaricRepo(AppDbContext appDbContext)
        {
            dbset = appDbContext.Set<T>();
        }

        public async Task<IReadOnlyList<T>> GetAllAsync()
        {
            return await dbset
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IReadOnlyList<T>> GetAllAsync(
            ISpecification<T> specification)
        {
            var query = SpecificationEvaluator<T>.GetQuery(
                dbset.AsNoTracking(),
                specification);

            return await query.ToListAsync();
        }

        public async Task<T?> FindAsync(
            ISpecification<T> specification)
        {
            var query = SpecificationEvaluator<T>.GetQuery(
                dbset.AsNoTracking(),
                specification);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<int> CountAsync(ISpecification<T> specification)
        {
            var query = SpecificationEvaluator<T>.GetCountQuery(
                dbset.AsNoTracking(),
                specification);

            return await query.CountAsync();
        }

        public async Task<T?> GetByIdAsync(params object[] id)
        {
            return await dbset.FindAsync(id);
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