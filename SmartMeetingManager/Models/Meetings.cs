namespace SmartMeetingManager.Models
{
	public class Meetings
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public string Status { get; set; } // Resheduled, Scheduled, Ongoing, Completed, Cancelled
		public DateTime CreatedAt { get; set; }

		// Foreign Keys
		public int UserId { get; set; }
		public int RoomId { get; set; }

		// Navigation Properties
		public Users User { get; set; }
		public Rooms Room { get; set; }

		public ICollection<MeetingAttendees> Attendees { get; set; } = new List<MeetingAttendees>();
		public ICollection<Agendas> Agendas { get; set; } = new List<Agendas>();


	}
}
