using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VoyageHub.Business.Dtos;
using VoyageHub.Data.Entities;

namespace VoyageHub.Business.Services;

public interface ITravelRequestService
{
    Task<List<TravelRequestListDto>> GetRequestsAsync(string? status, string? search, Guid? employeeId, int? limit, CancellationToken cancellationToken = default);
    Task<DashboardStatsDto> GetStatsAsync(Guid? employeeId, CancellationToken cancellationToken = default);
    Task<TravelRequestDetailDto?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(bool Success, string Message, TravelRequestDetailDto? Request)> CreateRequestAsync(CreateTravelRequestDto dto, Guid? employeeId, CancellationToken cancellationToken = default);
    Task<(bool Success, string Message)> CancelRequestAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(int StatusCode, object Response)> SubmitDraftAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(bool Success, string Message, ItinerarySegment? Segment)> AddSegmentAsync(Guid id, AddSegmentDto dto, CancellationToken cancellationToken = default);
    Task<(bool Success, string Message, Stay? Stay)> AddStayAsync(Guid id, AddStayDto dto, CancellationToken cancellationToken = default);
    Task<(bool Success, string Message)> CloseRequestAsync(Guid id, CancellationToken cancellationToken = default);
}

