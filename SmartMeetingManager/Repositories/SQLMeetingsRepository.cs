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


	}
}
