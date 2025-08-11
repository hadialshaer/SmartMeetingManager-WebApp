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
		public List<string> Attendees { get; set; } = new List<string>();
		public List<string> Agendas { get; set; } = new List<string>();
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	}
}
