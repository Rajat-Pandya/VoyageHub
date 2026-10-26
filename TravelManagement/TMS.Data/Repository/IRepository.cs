namespace TMS.Data.Repository
{
    // Generic repository contract. "TEntity" is a placeholder for any entity class,
    // so the same interface works for TravelRequest, Expense, Itinerary, ...
    public interface IRepository<TEntity> where TEntity : class
    {
        Task<IEnumerable<TEntity>> GetAllAsync();

        Task<TEntity?> GetByIdAsync(int id); // "?" = may return null when the id does not exist

        Task AddAsync(TEntity entity);

        void Update(TEntity entity); // Only marks the entity as changed (no DB call yet)

        void Delete(TEntity entity); // Only marks the entity as deleted (no DB call yet)

        Task SaveAsync(); // Actually sends INSERT / UPDATE / DELETE to the database
    }
}
