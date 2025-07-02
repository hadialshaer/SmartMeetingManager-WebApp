namespace SmartMeetingManager.Models.DTOs
{
	public class CreateMeetingDTO
	{
		public string Title { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }

		public int UserId { get; set; }
		public int RoomId { get; set; }
	}
}
