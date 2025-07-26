using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
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
				.Include(m => m.User)
				.Include(m => m.Room)
				.ToListAsync();

			return meetings;
		}
		public async Task<Meetings?> GetMeetingByIdAsync(int id)
		{
			var meeting = await dbContext.Meetings
				.Include(m => m.User)
				.Include(m => m.Room)
				.FirstOrDefaultAsync(m => m.Id == id);

			return meeting;

		}
		public async Task<Meetings> CreateMeetingAsync(Meetings meeting)
		{

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
			{
				return null;
			}

			existingMeeting.Title = updateMeetingDTO.Title;
			existingMeeting.StartTime = updateMeetingDTO.StartTime;
			existingMeeting.EndTime = updateMeetingDTO.EndTime;
			existingMeeting.Status = updateMeetingDTO.Status;
			existingMeeting.RoomId = updateMeetingDTO.RoomId;

			await dbContext.SaveChangesAsync();
			return existingMeeting;

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
				(excludeId == null || m.Id != excludeId) &&
				m.StartTime < endTime && m.EndTime > startTime);
		}

		public async Task<bool> CancelMeetingAsync(int meetingId)
		{
			var meeting = await dbContext.Meetings.FindAsync(meetingId);
			if (meeting == null) return false;
			meeting.Status = "Cancelled";
			await dbContext.SaveChangesAsync();
			return true;
		}

		public async Task<bool> RescheduleMeetingAsync(int meetingId, RescheduleDTO dto)
		{
			var m = await dbContext.Meetings.FindAsync(meetingId);
			if (m == null) return false;

			// check conflicts
			if (await RoomHasConflictAsync(dto.NewRoomId ?? m.RoomId, dto.NewStartTime, dto.NewEndTime, meetingId))
				return false;
			if (await OrganizerHasConflictAsync(m.UserId, dto.NewStartTime, dto.NewEndTime, meetingId))
				return false;

			m.StartTime = dto.NewStartTime;
			m.EndTime = dto.NewEndTime;
			if (dto.NewRoomId.HasValue) m.RoomId = dto.NewRoomId.Value;
			m.Status = "Rescheduled";
			await dbContext.SaveChangesAsync();
			return true;
		}

		public async Task<bool> AddAttendeesAsync(int meetingId, List<int> userIds)
		{
			// ignore duplicates already present
			var existingAttendees = await dbContext.MeetingAttendees
				.Where(meetingAttendees => meetingAttendees.MeetingId == meetingId)
				.Select(meetingAttendees => meetingAttendees.UserId)
				.ToListAsync();

			var newIds = userIds.Except(existingAttendees).ToList();
			foreach (var uid in newIds)
			{
				dbContext.MeetingAttendees.Add(new MeetingAttendees
				{
					MeetingId = meetingId,
					UserId = uid,
					Role = "Participant",
					AttendanceStatus = false // Default attendance status
				});
			}
			await dbContext.SaveChangesAsync();
			return true;
		}





	}
}
