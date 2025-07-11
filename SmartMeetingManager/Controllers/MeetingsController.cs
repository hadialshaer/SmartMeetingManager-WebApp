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
			// Basic time validation
			if (createMeetingDTO.StartTime >= createMeetingDTO.EndTime)
				return BadRequest("Start time must be before end time.");

			// Check if User exists
			bool userExists = await dbContext.Users.AnyAsync(u => u.Id == createMeetingDTO.UserId);
			if (!userExists)
				return BadRequest("User does not exist.");

			// Check if Room exists
			bool roomExists = await dbContext.Rooms.AnyAsync(r => r.Id == createMeetingDTO.RoomId);
			if (!roomExists)
				return BadRequest("Selected room does not exist.");

			// Check for room booking conflicts
			bool roomConflict = await dbContext.Meetings.AnyAsync(m =>
				m.RoomId == createMeetingDTO.RoomId &&
				m.StartTime < createMeetingDTO.EndTime &&
				m.EndTime > createMeetingDTO.StartTime
			);
			if (roomConflict)
				return Conflict("The selected room is already booked for the specified time.");

			// Check if User has conflicting meeting
			bool userConflict = await dbContext.Meetings.AnyAsync(m =>
				m.UserId == createMeetingDTO.UserId &&
				m.StartTime < createMeetingDTO.EndTime &&
				m.EndTime > createMeetingDTO.StartTime
			);
			if (userConflict)
				return Conflict("Organizer has another meeting during this time.");

			// Map DTO to Model
			var meeting = new Meetings
			{
				Title = createMeetingDTO.Title,
				StartTime = createMeetingDTO.StartTime,
				EndTime = createMeetingDTO.EndTime,
				Status = "Scheduled",
				CreatedAt = DateTime.UtcNow,
				UserId = createMeetingDTO.UserId,
				RoomId = createMeetingDTO.RoomId
			};

			meeting = await meetingsRepository.CreateMeetingAsync(meeting);

			// Map to DTO for response
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

			return CreatedAtAction(nameof(GetMeetingById), new { id = meetingDTO.Id }, meetingDTO);
		}

		[HttpPut]
		[Route("{id:int}")]
		public async Task<IActionResult> UpdateMeeting([FromRoute] int id, [FromBody] UpdateMeetingDTO updateMeetingDTO)
		{
			// Basic time validation
			if (updateMeetingDTO.StartTime >= updateMeetingDTO.EndTime)
				return BadRequest("Start time must be before end time.");

			// Check if Meeting exists
			var existingMeeting = await meetingsRepository.GetMeetingByIdAsync(id);
			if (existingMeeting == null)
				return NotFound("Meeting not found.");

			// Check if Room exists
			bool roomExists = await dbContext.Rooms.AnyAsync(r => r.Id == updateMeetingDTO.RoomId);
			if (!roomExists)
				return BadRequest("Selected room does not exist.");

			// Check for room booking conflicts excluding this meeting
			bool roomConflict = await dbContext.Meetings.AnyAsync(m =>
				m.RoomId == updateMeetingDTO.RoomId &&
				m.Id != id && // Exclude current meeting
				m.StartTime < updateMeetingDTO.EndTime &&
				m.EndTime > updateMeetingDTO.StartTime
			);
			if (roomConflict)
				return Conflict("The selected room is already booked for the specified time.");

			// Check if User has conflicting meeting excluding this one
			bool userConflict = await dbContext.Meetings.AnyAsync(m =>
				m.UserId == existingMeeting.UserId &&
				m.Id != id &&
				m.StartTime < updateMeetingDTO.EndTime &&
				m.EndTime > updateMeetingDTO.StartTime
			);
			if (userConflict)
				return Conflict("Organizer has another meeting during this time.");

			var updatedMeeting = await meetingsRepository.UpdateMeetingAsync(id, updateMeetingDTO);

			if (updatedMeeting == null)
			{
				return NotFound("Meeting not found.");
			}

			// Convert Model to DTO
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
				OrganizerName = dbContext.Users
					.Where(u => u.Id == meeting.UserId)
					.Select(u => $"{u.FirstName} {u.LastName}")
					.FirstOrDefault() ?? "Unknown Organizer",
				RoomName = dbContext.Rooms
					.Where(r => r.Id == meeting.RoomId)
					.Select(r => r.Name)
					.FirstOrDefault() ?? "No Room Assigned"
			};
			return Ok(meetingDTO);
		}


	}
}
