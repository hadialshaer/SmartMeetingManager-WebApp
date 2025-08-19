using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SmartMeetingManager.Data;
using SmartMeetingManager.Models;
using SmartMeetingManager.Models.DTOs;

namespace SmartMeetingManager.Repositories
{
	public class SQLMeetingsRepository : IMeetingsRepository
	{
		private readonly SmartMeetingManagerDbContext dbContext;

		public SQLMeetingsRepository(SmartMeetingManagerDbContext dbContext)
		{
			this.dbContext = dbContext;
		}
		public async Task<List<Meetings>> GetAllMeetingsAsync()
		{
			var meetings = await dbContext.Meetings
				.AsNoTracking()
				.Include(m => m.User)
				.Include(m => m.Room)
				.Include(m => m.Attendees)
					.ThenInclude(a => a.User) // Include user details for attendees
				.Include(m => m.Agendas)
				.ToListAsync();

			return meetings;
		}
		public async Task<Meetings?> GetMeetingByIdAsync(int id)
		{
			var meeting = await dbContext.Meetings
				.AsNoTracking()
				.Include(m => m.User)
				.Include(m => m.Room)
				.Include(m => m.Attendees)
					.ThenInclude(a => a.User) // Include user details for attendees
				.Include(m => m.Agendas)
				.FirstOrDefaultAsync(m => m.Id == id);

			return meeting;

		}
		public async Task<Meetings> CreateMeetingAsync(Meetings meeting)
		{
			if (meeting == null)
				throw new ArgumentNullException(nameof(meeting), "Meeting cannot be null.");

			if (!await dbContext.Rooms.AnyAsync(r => r.Id == meeting.RoomId))
				throw new ArgumentException("Invalid RoomId.");

			if (!await dbContext.Users.AnyAsync(u => u.Id == meeting.UserId))
				throw new ArgumentException("Invalid UserId.");

			if (meeting.StartTime >= meeting.EndTime)
				throw new ArgumentException("Start time must be before end time.");

			// Check for room conflicts
			if (await RoomHasConflictAsync(meeting.RoomId, meeting.StartTime, meeting.EndTime))
				throw new InvalidOperationException("Room is already booked at this time.");
			// Check for organizer conflicts
			if (await OrganizerHasConflictAsync(meeting.UserId, meeting.StartTime, meeting.EndTime))
				throw new InvalidOperationException("Organizer has another meeting at this time.");

			await dbContext.Meetings.AddAsync(meeting);
			await dbContext.SaveChangesAsync();
			return await dbContext.Meetings
				.Include(m => m.User)
				.Include(m => m.Room)
				.FirstAsync(m => m.Id == meeting.Id);
		}
		public async Task<Meetings?> UpdateMeetingAsync(int id, UpdateMeetingDTO updateMeetingDTO)
		{
			var existingMeeting = await dbContext.Meetings
				.Include(m => m.User)
				.Include(m => m.Room)
				.FirstOrDefaultAsync(m => m.Id == id);

			if (existingMeeting == null)
				return null;

			// Validate room exists if changed
			if (updateMeetingDTO.RoomId != existingMeeting.RoomId &&
				!await dbContext.Rooms.AnyAsync(r => r.Id == updateMeetingDTO.RoomId))
				throw new ArgumentException("Invalid RoomId.");

			// Only check for conflicts if something is changing
			if (existingMeeting.StartTime != updateMeetingDTO.StartTime ||
				existingMeeting.EndTime != updateMeetingDTO.EndTime ||
				existingMeeting.RoomId != updateMeetingDTO.RoomId)
			{
				if (await RoomHasConflictAsync(updateMeetingDTO.RoomId, updateMeetingDTO.StartTime, updateMeetingDTO.EndTime, id))
					throw new InvalidOperationException("Room is already booked at this time.");

				if (await OrganizerHasConflictAsync(existingMeeting.UserId, updateMeetingDTO.StartTime, updateMeetingDTO.EndTime, id))
					throw new InvalidOperationException("Organizer has another meeting at this time.");
			}

			existingMeeting.Title = updateMeetingDTO.Title;
			existingMeeting.StartTime = updateMeetingDTO.StartTime;
			existingMeeting.EndTime = updateMeetingDTO.EndTime;
			existingMeeting.Status = updateMeetingDTO.Status;
			existingMeeting.RoomId = updateMeetingDTO.RoomId;

			await dbContext.SaveChangesAsync();
			return await dbContext.Meetings
					.Include(m => m.User)
					.Include(m => m.Room)
					.FirstAsync(m => m.Id == id);
		}
		public async Task<Meetings?> DeleteMeetingAsync(int id)
		{
			var existingMeeting = await dbContext.Meetings
				.Include(m => m.User)
				.Include(m => m.Room)
				.FirstOrDefaultAsync(m => m.Id == id);

			if (existingMeeting == null)
			{
				return null;
			}
			dbContext.Meetings.Remove(existingMeeting);
			await dbContext.SaveChangesAsync();
			return existingMeeting;
		}
		public async Task<bool> RoomHasConflictAsync(int roomId, DateTime startTime, DateTime endTime, int? excludeId = null)
		{
			return await dbContext.Meetings.AnyAsync(m =>
				m.RoomId == roomId &&
				m.Status != "Cancelled" && // Exclude cancelled meetings
				(excludeId == null || m.Id != excludeId) &&
				m.StartTime < endTime && m.EndTime > startTime);
		}

		public async Task<bool> OrganizerHasConflictAsync(int userId, DateTime startTime, DateTime endTime, int? excludeId = null)
		{
			return await dbContext.Meetings.AnyAsync(m =>
				m.UserId == userId &&
				m.Status != "Cancelled" && // Exclude cancelled meetings
				(excludeId == null || m.Id != excludeId) &&
				m.StartTime < endTime && m.EndTime > startTime);
		}

		public async Task<bool> CancelMeetingAsync(int meetingId)
		{
			var meeting = await dbContext.Meetings.FindAsync(meetingId);
			if (meeting == null || meeting.Status == "Cancelled") return false;
			meeting.Status = "Cancelled";
			await dbContext.SaveChangesAsync();
			return true;
		}

		public async Task<bool> RescheduleMeetingAsync(int meetingId, RescheduleDTO dto)
		{
			var m = await dbContext.Meetings.FindAsync(meetingId);
			if (m == null || m.Status == "Cancelled")
				throw new ArgumentException("Meeting not found or already cancelled.");
			if (dto.NewStartTime >= dto.NewEndTime)
				throw new ArgumentException("New start time must be before new end time.");

				// check conflicts
				if (await RoomHasConflictAsync(dto.NewRoomId ?? m.RoomId, dto.NewStartTime, dto.NewEndTime, meetingId))
				return false;
			if (await OrganizerHasConflictAsync(m.UserId, dto.NewStartTime, dto.NewEndTime, meetingId))
				return false;

			// Validate room exists if changed
			if (dto.NewRoomId != m.RoomId &&
				!await dbContext.Rooms.AnyAsync(r => r.Id == dto.NewRoomId))
				throw new ArgumentException("Invalid RoomId.");

			m.StartTime = dto.NewStartTime;
			m.EndTime = dto.NewEndTime;
			if (dto.NewRoomId.HasValue) m.RoomId = dto.NewRoomId.Value;
			m.Status = "Rescheduled";
			await dbContext.SaveChangesAsync();
			return true;
		}

		public async Task<bool> AddAttendeesAsync(int meetingId, List<int> userIds)
		{
			if (userIds == null || userIds.Count == 0)
				throw new ArgumentException("No user ids provided.");

			// Load meeting with its attendees and room
			var meeting = await dbContext.Meetings
				.Include(m => m.Attendees)
				.Include(m => m.Room)
				.FirstOrDefaultAsync(m => m.Id == meetingId) ??
					throw new KeyNotFoundException($"Meeting with ID {meetingId} not found.");
			
			if (meeting.Status == "Cancelled")
				throw new InvalidOperationException("Cannot add attendees to a cancelled meeting.");

			if (meeting.Status == "Completed")
				throw new InvalidOperationException("Cannot add attendees to a completed meeting.");

			if (meeting.Room == null)
				throw new InvalidOperationException("Meeting has no assigned room.");

			// Ensure users exist (avoid foreign key failures)
			var existingUserIdsInDb = await dbContext.Users
				.Where(u => userIds.Contains(u.Id))
				.Select(u => u.Id)
				.ToListAsync();

			if (existingUserIdsInDb.Count == 0)
				throw new ArgumentException("None of the provided user IDs exist.");

			var existingUserIds = meeting.Attendees.Select(a => a.UserId).ToList();
			var newIds = existingUserIdsInDb.Except(existingUserIds).ToList();

			// Check room capacity
			int totalAfterAdd = existingUserIds.Count + newIds.Count;
			int capacity = meeting.Room.Capacity;
			if (totalAfterAdd > capacity)
				throw new InvalidOperationException($"Room capacity exceeded. Available: {capacity - existingUserIds.Count}");

			// Add new attendees
			var newAttendees = newIds.Select(uid => new MeetingAttendees
			{
				MeetingId = meetingId,
				UserId = uid,
				Role = "Participant",
				AttendanceStatus = false
			}).ToList();

			dbContext.MeetingAttendees.AddRange(newAttendees);
			await dbContext.SaveChangesAsync();

			return true;
		}
		public async Task<List<Rooms>> CheckAvailabilityAsync(DateTime startTime, DateTime endTime, int? minCapacity = null)
		{
			if (startTime >= endTime)
				throw new ArgumentException("Start time must be before end time.");

			var query = dbContext.Rooms
				.AsNoTracking();

			if (minCapacity.HasValue)
				query = query.Where(r => r.Capacity >= minCapacity.Value);

			query = query.Where(room =>
				!dbContext.Meetings.Any(m =>
					m.RoomId == room.Id &&
					m.Status != "Cancelled" &&
					m.StartTime < endTime &&   // overlap test
					m.EndTime > startTime
				)
			);

			return await query
				.Include(r => r.RoomFeatures)
					.ThenInclude(rf => rf.Feature)
				.ToListAsync();
		}


	}
}
