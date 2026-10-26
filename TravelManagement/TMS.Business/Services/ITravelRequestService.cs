using TMS.Data.Entity;

namespace TMS.Business.Services
{
    // What the business layer offers to the controller. The controller only knows this interface.
    public interface ITravelRequestService
    {
        Task<IEnumerable<TravelRequest>> GetAllAsync();

        Task<TravelRequest?> GetByIdAsync(int id);

        Task<TravelRequest> CreateAsync(TravelRequest travelRequest);

        Task<TravelRequest?> UpdateAsync(int id, TravelRequest travelRequest); // null = not found

        Task<TravelRequest?> UpdateStatusAsync(int id, string newStatus);

        Task<bool> DeleteAsync(int id); // false = not found
    }
}
