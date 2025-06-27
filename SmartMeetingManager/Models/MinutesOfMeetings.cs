namespace SmartMeetingManager.Models
{
	public class MinutesOfMeetings
	{
		public int Id { get; set; }
		public string Status { get; set; } // Draft, Finalized, Archived
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime UpdatedAt { get; set; }

		// Foreign Keys
		public int MeetingId { get; set; }

		// Navigation Properties
		public Meetings Meeting { get; set; }

	}
}
