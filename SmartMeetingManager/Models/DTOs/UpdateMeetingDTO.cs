namespace SmartMeetingManager.Models.DTOs
{
	public class UpdateMeetingDTO
	{
		public string Title { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public string Status { get; set; }

		public int RoomId { get; set; }
	}
}
