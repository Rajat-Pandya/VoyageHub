using Microsoft.EntityFrameworkCore;

namespace TMS.Data.Repository
{
    // The only class that talks to EF Core directly. Everything above it uses IRepository<T>.
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly AppDbContext _appDbContext;

        public Repository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        // READ (all): SELECT * FROM TravelRequests
        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await _appDbContext.Set<TEntity>().ToListAsync();
        }

        // READ (one): SELECT ... WHERE TravelRequestId = @id   (returns null if not found)
        public async Task<TEntity?> GetByIdAsync(int id)
        {
            return await _appDbContext.Set<TEntity>().FindAsync(id);
        }

        // CREATE: marks the entity as "Added". The INSERT happens on SaveAsync().
        public async Task AddAsync(TEntity entity)
        {
            await _appDbContext.Set<TEntity>().AddAsync(entity);
        }

        // UPDATE: marks the entity as "Modified". The UPDATE happens on SaveAsync().
        public void Update(TEntity entity)
        {
            _appDbContext.Set<TEntity>().Update(entity);
        }

        // DELETE: marks the entity as "Deleted". The DELETE happens on SaveAsync().
        public void Delete(TEntity entity)
        {
            _appDbContext.Set<TEntity>().Remove(entity);
        }

        // Sends every pending change to SQL Server in one transaction.
        public async Task SaveAsync()
        {
            await _appDbContext.SaveChangesAsync();
        }
    }
}
