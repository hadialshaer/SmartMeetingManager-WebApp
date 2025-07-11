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
	}
}
