using HotelManagement.Application.Booking.Queries.GetAvailableRooms;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Identity.Domain;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoomController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<RoomController> _logger;

    public RoomController(
        IMediator mediator,
        ICurrentUserService currentUserService,
        ILogger<RoomController> logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    [HttpGet("available")]
    [ProducesResponseType(typeof(List<AvailableRoomDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAvailableRooms(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            _logger.LogInformation(
                "User {UserId} requesting available rooms from {StartDate} to {EndDate}",
                _currentUserService.UserId,
                startDate,
                endDate);

            if (startDate == default || endDate == default)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid date parameters",
                    Detail = "Start date and end date are required",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var query = new GetAvailableRoomsQuery
            {
                StartDate = startDate,
                EndDate = endDate,
            };

            var result = await _mediator.Send(query);

            if (result.IsFailure)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Failed to get available rooms",
                    Detail = result.Error,
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var rooms = result.Value;

            if (_currentUserService.Role == UserRole.Customer)
            {
                rooms = rooms.Select(r => new AvailableRoomDto
                {
                    RoomId = r.RoomId,
                    RoomNumber = r.RoomNumber,
                    RoomType = r.RoomType,
                }).ToList();
            }

            _logger.LogInformation(
                "Found {RoomCount} available rooms",
                rooms.Count,
                _currentUserService.UserId);

            return Ok(rooms);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting available rooms for user {UserId}",
                _currentUserService.UserId);

            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "An error occurred",
                Detail = "An unexpected error occurred while processing your request",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }
}