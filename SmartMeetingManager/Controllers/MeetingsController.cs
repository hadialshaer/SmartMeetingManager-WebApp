using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartMeetingManager.Data;
using SmartMeetingManager.Models;
using SmartMeetingManager.Models.DTOs;

namespace SmartMeetingManager.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class MeetingsController : ControllerBase
	{
		private readonly SmartMeetingManagerDbContext dbContext;
		public MeetingsController(SmartMeetingManagerDbContext dbContext)
		{
			this.dbContext = dbContext;
		}

		// Get All Meetings
		[HttpGet]
		public IActionResult GetAllMeetings()
		{
			// Get Data from Database - Domain Models
			var meetings = dbContext.Meetings
				.Include(m => m.User)
				.Include(m => m.Room)
				.ToList();

			if (meetings.Count == 0)
			{
				return NotFound();
			}

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
		public IActionResult GetMeetingById([FromRoute] int id)
		{
			// Get Data from Database - Domain Models
			var meeting = dbContext.Meetings
				.Include(m => m.User)
				.Include(m => m.Room)
				.FirstOrDefault(m => m.Id == id);

			if (meeting == null)
			{
				return NotFound();
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
		public IActionResult CreateMeeting([FromBody] CreateMeetingDTO createMeetingDTO)
		{
			// Map CreateMeetingDTO to Meetings Model
			var meeting = new Meetings
			{
				Title = createMeetingDTO.Title,
				StartTime = createMeetingDTO.StartTime,
				EndTime = createMeetingDTO.EndTime,
				Status = "Scheduled", // Default status
				CreatedAt = DateTime.UtcNow, // Set created at to current time
				UserId = createMeetingDTO.UserId,
				RoomId = createMeetingDTO.RoomId
			};

			// Use Model to create new Meeting and Add to Database
			dbContext.Meetings.Add(meeting);
			dbContext.SaveChanges();

			var organizerName = dbContext.Users
				.Where(u => u.Id == meeting.UserId)
				.Select(u => $"{u.FirstName} {u.LastName}")
				.FirstOrDefault() ?? "Unknown Organizer";

			var roomName = dbContext.Rooms
				.Where(r => r.Id == meeting.RoomId)
				.Select(r => r.Name)
				.FirstOrDefault() ?? "No Room Assigned";

			// Map Meetings Model to MeetingDTO
			var meetingDTO = new MeetingDTO
			{
				Id = meeting.Id,
				Title = meeting.Title,
				StartTime = meeting.StartTime,
				EndTime = meeting.EndTime,
				Status = meeting.Status,
				OrganizerName = organizerName,
				RoomName = roomName
			};
			return CreatedAtAction(nameof(GetMeetingById), new { id = meetingDTO.Id }, meetingDTO);

		}

		// Update Meeting
		[HttpPut]
		[Route("{id:int}")]
		public IActionResult UpdateMeeting([FromRoute] int id, [FromBody] UpdateMeetingDTO updateMeetingDTO)
		{
			var meeting = dbContext.Meetings.FirstOrDefault(m => m.Id == id);
			if (meeting == null)
			{
				return NotFound();
			}
			// Map MeetingDTO to Meeting Model
			meeting.Title = updateMeetingDTO.Title;
			meeting.StartTime = updateMeetingDTO.StartTime;
			meeting.EndTime = updateMeetingDTO.EndTime;
			meeting.Status = updateMeetingDTO.Status;
			meeting.RoomId = updateMeetingDTO.RoomId;

			dbContext.SaveChanges();

			var organizerName = dbContext.Users
				.Where(u => u.Id == meeting.UserId)
				.Select(u => $"{u.FirstName} {u.LastName}")
				.FirstOrDefault() ?? "Unkown Organizer";

			var roomName = dbContext.Rooms
				.Where(r => r.Id == meeting.RoomId)
				.Select(r => r.Name)
				.FirstOrDefault() ?? "No Room Assigned";

			// Convert Meeting Model to MeetingDTO
			var meetingDTO = new MeetingDTO
			{
				Id = meeting.Id,
				Title = meeting.Title,
				StartTime = meeting.StartTime,
				EndTime = meeting.EndTime,
				Status = meeting.Status,
				OrganizerName = organizerName,
				RoomName = roomName
			};

			// Return MeetingDTO to Client
			return Ok(meetingDTO);

		}

		// Delete Meeting
		[HttpDelete]
		[Route("{id:int}")]
		public IActionResult DeleteMeeting([FromRoute] int id)
		{
			var meeting = dbContext.Meetings.FirstOrDefault(m => m.Id == id);

			if (meeting == null)
			{
				return NotFound();
			}

			// Remove Meeting from Database
			dbContext.Meetings.Remove(meeting);
			dbContext.SaveChanges();

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
