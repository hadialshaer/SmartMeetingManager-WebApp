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
		private readonly IMeetingsRepository meetingsRepository;

		public MeetingsController( IMeetingsRepository meetingsRepository)
		{
			this.meetingsRepository = meetingsRepository;
		}

		// Get All Meetings
		[HttpGet]
		public async Task<IActionResult> GetAllMeetings()
		{
			// Get Data from Database - Domain Models
			var meetings = await meetingsRepository.GetAllMeetingsAsync();

			// Map Model to DTOs
			var meetingsDTO = meetings.Select(m => new MeetingDTO
			{
				Id = m.Id,
				Title = m.Title,
				StartTime = m.StartTime,
				EndTime = m.EndTime,
				Status = m.Status,
				OrganizerName = m.User != null
					? $"{m.User.FirstName} {m.User.LastName}"
					: "Unknown Organizer",
				RoomName = m.Room?.Name ?? "No Room Assigned",
				Attendees = m.Attendees?
							.Where(a => a.User != null)
							.Select(a => $"{a.User.FirstName} {a.User.LastName}")
							.ToList()
							?? [],

				Agendas = m.Agendas?
						.Select(a => a.Topic)
						.ToList()
						?? []

			}).ToList();
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
				RoomName = meeting.Room?.Name ?? "No Room Assigned",
				Attendees = meeting.Attendees?
							.Where(a => a.User != null)
							.Select(a => $"{a.User.FirstName} {a.User.LastName}")
							.ToList()
							?? [],
				Agendas = meeting.Agendas?
							.Select(a => a.Topic)
							.ToList()
							?? [],

			};

			return Ok(meetingDTO);
		}

		// Create New Meeting
		[HttpPost]
		public async Task<IActionResult> CreateMeeting([FromBody] CreateMeetingDTO createMeetingDTO)
		{

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

			catch (ArgumentNullException ex)
			{
				return BadRequest("Invalid data sent: " + ex.Message);
			}

			catch (ArgumentException ex)
			{ 
				return BadRequest(ex.Message);
			}
			catch (InvalidOperationException ex)
			{ 
				return Conflict(ex.Message); 
			}    
		}

		[HttpPut]
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

			catch (ArgumentException ex)
			{
				return BadRequest(ex.Message);
			}

			catch (InvalidOperationException ex)
			{
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
			var ok = await meetingsRepository.CancelMeetingAsync(id);
			return ok ? NoContent() : NotFound();
		}

		[HttpPost]
		[Route("{id:int}/reschedule")]
		public async Task<IActionResult> Reschedule(int id, [FromBody] RescheduleDTO rescheduleDTO)
		{
			// Validate the incoming DTO
			if (rescheduleDTO == null)
				return BadRequest("No data sent.");
			// Basic time validation
			if (rescheduleDTO.NewStartTime >= rescheduleDTO.NewEndTime)
				return BadRequest("Start time must be before end time.");
			try
			{
				var result = await meetingsRepository.RescheduleMeetingAsync(id, rescheduleDTO);
				return result ? NoContent() : Conflict("Cannot reschedule due to conflict or meeting cancelled.");
			}

			catch (ArgumentException ex)
			{
				return BadRequest(ex.Message);
			}

		}

		[HttpPost("{id:int}/attendees")]
		public async Task<IActionResult> AddAttendees(int id, [FromBody] AddAttendeesDTO dto)
		{
			if (dto == null || dto.UserIds == null || dto.UserIds.Count == 0)
				return BadRequest(new { message = "No user ids provided." });

			try
			{
				var ok = await meetingsRepository.AddAttendeesAsync(id, dto.UserIds);
				return ok ? NoContent() : Conflict("Could not add attendees for unknown reasons.");
			}
			catch (KeyNotFoundException ex)
			{
				return NotFound(new { message = ex.Message });
			}
			catch (ArgumentException ex)
			{
				return BadRequest(new { message = ex.Message });
			}
			catch (InvalidOperationException ex)
			{
				return Conflict(new { message = ex.Message });
			}
			catch (Exception ex)
			{
				// Generic handler for unexpected errors
				return StatusCode(StatusCodes.Status500InternalServerError,
					new { message = "An unexpected error occurred.", detail = ex.Message });
			}
		}

		[HttpGet("availability")]
		public async Task<IActionResult> CheckAvailability([FromQuery] CheckAvailabilityDTO dto)
		{
			// Validate input
			if (dto == null)
				return BadRequest("No data sent.");
			if (dto.StartTime >= dto.EndTime)
				return BadRequest("Start time must be before end time.");

			var rooms = await meetingsRepository.CheckAvailabilityAsync(
				dto.StartTime, dto.EndTime, dto.MinCapacity);

			var result = rooms.Select(r => new {
				r.Id,
				r.Name,
				r.Capacity,
				r.Location,
				Features = r.RoomFeatures
				.Select(f => f.Feature.Name)
				.ToList()
			});

			return Ok(result);
		}





	}
}
