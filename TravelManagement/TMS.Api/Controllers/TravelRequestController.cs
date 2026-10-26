using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TMS.Api.Dtos;
using TMS.Business.Services;
using TMS.Data.Entity;

namespace TMS.Api.Controllers
{
    [Route("api/[controller]")] // [controller] = class name without "Controller" -> api/TravelRequest
    [ApiController]             // auto DTO validation (400), auto [FromBody] for complex types
    public class TravelRequestController : ControllerBase
    {
        private readonly ITravelRequestService _travelRequestService;
        private readonly IMapper _mapper;

        // ASP.NET Core's DI container creates the service and the mapper and passes them in here.
        public TravelRequestController(ITravelRequestService travelRequestService, IMapper mapper)
        {
            _travelRequestService = travelRequestService;
            _mapper = mapper;
        }

        // READ ALL -> GET api/TravelRequest
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var travelRequests = await _travelRequestService.GetAllAsync();
            var response = _mapper.Map<IEnumerable<TravelRequestResponseDto>>(travelRequests);
            return Ok(response); // 200 + JSON array (empty array if nothing exists)
        }

        // READ ONE -> GET api/TravelRequest/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var travelRequest = await _travelRequestService.GetByIdAsync(id);
            if (travelRequest == null)
            {
                return NotFound(); // 404
            }

            return Ok(_mapper.Map<TravelRequestResponseDto>(travelRequest)); // 200
        }

        // CREATE -> POST api/TravelRequest   (JSON body)
        [HttpPost]
        public async Task<IActionResult> Create(TravelRequestInputDto input)
        {
            try
            {
                var entity = _mapper.Map<TravelRequest>(input);              // Input DTO -> Entity
                var created = await _travelRequestService.CreateAsync(entity);
                var response = _mapper.Map<TravelRequestResponseDto>(created); // Entity -> Response DTO

                // 201 Created + "Location: api/TravelRequest/{newId}" header + the new record
                return CreatedAtAction(nameof(GetById), new { id = response.TravelRequestId }, response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message); // 400 when a business rule fails
            }
        }

        // UPDATE -> PUT api/TravelRequest/5   (JSON body)
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, TravelRequestInputDto input)
        {
            try
            {
                var entity = _mapper.Map<TravelRequest>(input);
                var updated = await _travelRequestService.UpdateAsync(id, entity);
                if (updated == null)
                {
                    return NotFound(); // 404
                }

                return Ok(_mapper.Map<TravelRequestResponseDto>(updated)); // 200
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // UPDATE STATUS -> PATCH api/TravelRequest/5/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] StatusUpdateDto dto)
        {
            var updated = await _travelRequestService.UpdateStatusAsync(id, dto.Status);
            if (updated == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<TravelRequestResponseDto>(updated));
        }

        // DELETE -> DELETE api/TravelRequest/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _travelRequestService.DeleteAsync(id);
            return deleted ? NoContent() : NotFound(); // 204 (success, no body) or 404
        }
    }
}
