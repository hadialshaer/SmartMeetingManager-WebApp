namespace SmartMeetingManager.Models
{
	public class MeetingAttendees
	{
		public int Id { get; set; }
		public string Role { get; set; } // Participent, Speaker, Organizer, NoteTaker
		public bool AttendanceStatus { get; set; } = false;

		// Foreign Keys
		public int UserId { get; set; }
		public int MeetingId { get; set; }

		// Navigation Properties
		public Users User { get; set; }
		public Meetings Meeting { get; set; }
	}
}
