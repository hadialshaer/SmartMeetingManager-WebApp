using SmartMeetingManager.Models;
using SmartMeetingManager.Models.DTOs;

namespace SmartMeetingManager.Repositories
{
	public interface IMeetingsRepository
	{
		Task <List<Meetings>> GetAllMeetingsAsync();
		Task<Meetings?> GetMeetingByIdAsync(int id);
		Task<Meetings> CreateMeetingAsync(Meetings meeting);
		Task<Meetings?> UpdateMeetingAsync(int id, UpdateMeetingDTO updateMeetingDTO);
		Task<Meetings?> DeleteMeetingAsync(int id);
		Task<bool> RoomHasConflictAsync(int roomId, DateTime startTime, DateTime endTime, int? excludeMeetingId = null);
		Task<bool> OrganizerHasConflictAsync(int userId, DateTime startTime, DateTime endTime, int? excludeMeetingId = null);
		Task<bool> CancelMeetingAsync(int meetingId);
		Task<bool> RescheduleMeetingAsync(int meetingId, RescheduleDTO dto);
		Task<bool> AddAttendeesAsync(int meetingId, List<int> userIds);
	}
}
