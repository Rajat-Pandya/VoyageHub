using TMS.Data.Entity;
using TMS.Data.Repository;

namespace TMS.Business.Services
{
    // Business rules live here. This class never writes SQL and never knows about HTTP.
    public class TravelRequestService : ITravelRequestService
    {
        private readonly IRepository<TravelRequest> _travelRequestRepository;

        public TravelRequestService(IRepository<TravelRequest> travelRequestRepository)
        {
            _travelRequestRepository = travelRequestRepository;
        }

        public async Task<IEnumerable<TravelRequest>> GetAllAsync()
        {
            return await _travelRequestRepository.GetAllAsync();
        }

        public async Task<TravelRequest?> GetByIdAsync(int id)
        {
            return await _travelRequestRepository.GetByIdAsync(id);
        }

        public async Task<TravelRequest> CreateAsync(TravelRequest travelRequest)
        {
            ValidateDates(travelRequest);

            // Values the user must NOT control are set by the system.
            travelRequest.Status = "Draft";
            travelRequest.CreatedOn = DateTime.UtcNow;

            await _travelRequestRepository.AddAsync(travelRequest);
            await _travelRequestRepository.SaveAsync(); // INSERT runs here; the new Id is filled in

            return travelRequest;
        }

        public async Task<TravelRequest?> UpdateAsync(int id, TravelRequest travelRequest)
        {
            var existing = await _travelRequestRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return null;
            }

            ValidateDates(travelRequest);

            // Copy only the editable fields. Id, Status and CreatedOn stay as they are.
            existing.EmployeeName = travelRequest.EmployeeName;
            existing.Destination = travelRequest.Destination;
            existing.StartDate = travelRequest.StartDate;
            existing.EndDate = travelRequest.EndDate;
            existing.Purpose = travelRequest.Purpose;
            existing.EstimatedBudget = travelRequest.EstimatedBudget;

            _travelRequestRepository.Update(existing);
            await _travelRequestRepository.SaveAsync(); // UPDATE runs here

            return existing;
        }

        public async Task<TravelRequest?> UpdateStatusAsync(int id, string newStatus)
        {
            var existing = await _travelRequestRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return null;
            }

            existing.Status = newStatus;
            _travelRequestRepository.Update(existing);
            await _travelRequestRepository.SaveAsync();

            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _travelRequestRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return false;
            }

            _travelRequestRepository.Delete(existing);
            await _travelRequestRepository.SaveAsync(); // DELETE runs here

            return true;
        }

        // Business rule: a trip cannot end before it starts.
        private static void ValidateDates(TravelRequest travelRequest)
        {
            if (travelRequest.EndDate < travelRequest.StartDate)
            {
                throw new ArgumentException("EndDate cannot be before StartDate.");
            }
        }
    }
}
