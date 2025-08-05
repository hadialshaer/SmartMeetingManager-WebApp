using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartMeetingManager.Data;
using SmartMeetingManager.Models;
using SmartMeetingManager.Models.DTOs;
using SmartMeetingManager.Repositories;

namespace SmartMeetingManager.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class MeetingsController : ControllerBase
	{
		private readonly SmartMeetingManagerDbContext dbContext;
		private readonly IMeetingsRepository meetingsRepository;

		public MeetingsController(SmartMeetingManagerDbContext dbContext, IMeetingsRepository meetingsRepository)
		{
			this.dbContext = dbContext;
			this.meetingsRepository = meetingsRepository;
		}

		// Get All Meetings
		[HttpGet]
		public async Task<IActionResult> GetAllMeetings()
		{
			// Get Data from Database - Domain Models
			var meetings = await meetingsRepository.GetAllMeetingsAsync();

			// Map Model to DTOs
			var meetingsDTO = new List<MeetingDTO>();
			foreach (var meeting in meetings)
			{
				meetingsDTO.Add(new MeetingDTO
				{
					Id = meeting.Id,
					Title = meeting.Title,
					StartTime = meeting.StartTime,
					EndTime = meeting.EndTime,
					Status = meeting.Status,
					OrganizerName = meeting.User != null
						? $"{meeting.User.FirstName} {meeting.User.LastName}"
						: "Unknown Organizer",
					RoomName = meeting.Room?.Name ?? "No Room Assigned"
				});

			}
			// Return DTOs to Client
			return Ok(meetingsDTO);
		}

		// Get Single Meeting
		[HttpGet]
		[Route("{id:int}")]
		public async Task<IActionResult> GetMeetingById([FromRoute] int id)
		{
			// Get Data from Database - Domain Models
			var meeting = await meetingsRepository.GetMeetingByIdAsync(id);

			if (meeting == null)
			{
				return NotFound("Meeting Not Found");
			}

			// Map Meetings Model to MeetingDTO
			var meetingDTO = new MeetingDTO
			{
				Id = meeting.Id,
				Title = meeting.Title,
				StartTime = meeting.StartTime,
				EndTime = meeting.EndTime,
				Status = meeting.Status,
				OrganizerName = meeting.User != null
					? $"{meeting.User.FirstName} {meeting.User.LastName}"
					: "Unknown Organizer",
				RoomName = meeting.Room?.Name ?? "No Room Assigned"
			};

			return Ok(meetingDTO);
		}

		// Create New Meeting
		[HttpPost]
		public async Task<IActionResult> CreateMeeting([FromBody] CreateMeetingDTO createMeetingDTO)
		{
			// Validate the incoming DTO
			if (createMeetingDTO == null)
				return BadRequest("No data sent.");

			// Basic time validation
			if (createMeetingDTO.StartTime >= createMeetingDTO.EndTime)
				return BadRequest("Start time must be before end time.");

			try
			{
				var meetingEntity = new Meetings
				{
					Title = createMeetingDTO.Title,
					StartTime = createMeetingDTO.StartTime,
					EndTime = createMeetingDTO.EndTime,
					Status = "Scheduled",
					CreatedAt = DateTime.UtcNow,
					UserId = createMeetingDTO.UserId,
					RoomId = createMeetingDTO.RoomId
				};

				var createdMeeting = await meetingsRepository.CreateMeetingAsync(meetingEntity);

				// Map DTO to Model
				var responseMeetingDTO = new MeetingDTO
				{
					Id = createdMeeting.Id,
					Title = createdMeeting.Title,
					StartTime = createdMeeting.StartTime,
					EndTime = createdMeeting.EndTime,
					Status = createdMeeting.Status,
					OrganizerName = createdMeeting.User != null
						? $"{createdMeeting.User.FirstName} {createdMeeting.User.LastName}"
						: "Unknown Organizer",
					RoomName = createdMeeting.Room?.Name ?? "No Room Assigned"
				};
				return CreatedAtAction(nameof(GetMeetingById), new { id = responseMeetingDTO.Id }, responseMeetingDTO);
			}
			catch (InvalidOperationException ex)
			{
				// Handle other exceptions
				return Conflict(ex.Message);
			}
		}

		[HttpPut]
		[Route("{id:int}")]
		public async Task<IActionResult> UpdateMeeting([FromRoute] int id, [FromBody] UpdateMeetingDTO updateMeetingDTO)
		{
			// Validate the incoming DTO
			if (updateMeetingDTO == null) 
				return BadRequest("No data sent.");

			// Basic time validation
			if (updateMeetingDTO.StartTime >= updateMeetingDTO.EndTime) 
				return BadRequest("Start time must be before end time.");

			try
			{
				var updatedMeeting = await meetingsRepository.UpdateMeetingAsync(id, updateMeetingDTO);
				if (updatedMeeting == null)
				{
					return NotFound("Meeting not found.");
				}

				// Map updated model to DTO
				var meetingDTO = new MeetingDTO
				{
					Id = updatedMeeting.Id,
					Title = updatedMeeting.Title,
					StartTime = updatedMeeting.StartTime,
					EndTime = updatedMeeting.EndTime,
					Status = updatedMeeting.Status,
					OrganizerName = updatedMeeting.User != null
						? $"{updatedMeeting.User.FirstName} {updatedMeeting.User.LastName}"
						: "Unknown Organizer",
					RoomName = updatedMeeting.Room?.Name ?? "No Room Assigned"
				};
				return Ok(meetingDTO);

			}
			catch (InvalidOperationException ex)
			{
				// Handle other exceptions
				return Conflict(ex.Message);
			}
		}

		// Delete Meeting
		[HttpDelete]
		[Route("{id:int}")]
		public async Task<IActionResult> DeleteMeeting([FromRoute] int id)
		{
			var meeting = await meetingsRepository.DeleteMeetingAsync(id);

			if (meeting == null)
			{
				return NotFound();
			}

			// Return the deleted meeting back
			// map model to MeetingDTO
			var meetingDTO = new MeetingDTO
			{
				Id = meeting.Id,
				Title = meeting.Title,
				StartTime = meeting.StartTime,
				EndTime = meeting.EndTime,
				Status = meeting.Status,
				OrganizerName = meeting.User != null
					? $"{meeting.User.FirstName} {meeting.User.LastName}"
					: "Unknown Organizer",
				RoomName = meeting.Room?.Name ?? "No Room Assigned"
			};
			return Ok(meetingDTO);
		}

		// Cancel Meeting
		[HttpPost("{id:int}/cancel")]
		public async Task<IActionResult> CancelMeeting(int id)
		{
			var result = await meetingsRepository.CancelMeetingAsync(id);
			return result ? NoContent() : NotFound();
		}

		[HttpPost]
		[Route("{meetingId:int}/reschedule")]
		public async Task<IActionResult> Reschedule(int id, [FromBody] RescheduleDTO rescheduleDTO)
		{
			// Validate the incoming DTO
			if (rescheduleDTO == null)
				return BadRequest("No data sent.");
			// Basic time validation
			if (rescheduleDTO.NewStartTime >= rescheduleDTO.NewEndTime)
				return BadRequest("New start time must be before new end time.");
			// Check if the meeting exists

			var result = await meetingsRepository.RescheduleMeetingAsync(id, rescheduleDTO);
			return result ? NoContent() : Conflict("Cannot reschedule due to conflict or meeting cancelled.");
		}

		[HttpPost("{id:int}/attendees")]
		public async Task<IActionResult> AddAttendees(int id, [FromBody] AddAttendeesDTO addAttendeesDTO)
		{
			if (addAttendeesDTO?.UserIds == null || addAttendeesDTO.UserIds.Count == 0)
				return BadRequest("No user ids provided.");

			var success = await meetingsRepository.AddAttendeesAsync(id, addAttendeesDTO.UserIds);
			return success ? NoContent() : Conflict("Cannot add attendees (room at capacity or meeting not found).");
		}

		[HttpGet("availability")]
		public async Task<IActionResult> CheckAvailability
			(
				[FromQuery] DateTime startTime,
				[FromQuery] DateTime endTime,
				[FromQuery] int? minCapacity,
				[FromQuery] int? excludeMeetingId
			)
		{
			// Validate time range
			if (startTime >= endTime)
				return BadRequest("Invalid time range.");

			// Call repository to get available rooms
			var rooms = await meetingsRepository
				.CheckAvailabilityAsync(startTime, endTime, minCapacity, excludeMeetingId);



			var result = rooms.Select(r => new {
				r.Id,
				r.Name,
				r.Capacity,
				r.Location,
				Features = r.RoomFeatures.Select(f => f.Feature.Name).ToList()
			});

			return Ok(result);
		}




	}
}
