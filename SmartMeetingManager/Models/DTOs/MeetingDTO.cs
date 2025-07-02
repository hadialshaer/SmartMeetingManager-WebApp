namespace SmartMeetingManager.Models.DTOs
{
	public class MeetingDTO
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public string Status { get; set; }

		public string OrganizerName { get; set; }
		public string RoomName { get; set; }
	}
}
