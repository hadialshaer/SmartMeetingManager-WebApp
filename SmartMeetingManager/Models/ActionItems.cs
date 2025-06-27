namespace SmartMeetingManager.Models
{
	public class ActionItems
	{
		public int Id { get; set; }
		public string Description { get; set; }
		public DateTime DueDate { get; set; }
		public string Status { get; set; } // Pending, In Progress, Completed
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime UpdatedAt { get; set; }

		// Foreign Keys
		public int UserId { get; set; }
		public int MinutesOfMeetingId { get; set; }

		// Navigation Properties
		public Users User { get; set; }
		public MinutesOfMeetings MinutesOfMeeting { get; set; }

	}
}