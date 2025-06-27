namespace SmartMeetingManager.Models
{
	public class Meetings
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public string Status { get; set; } // Scheduled, Ongoing, Completed, Cancelled
		public DateTime CreatedAt { get; set; }

		// Foreign Keys
		public int UserId { get; set; }
		public int RoomId { get; set; }

		// Navigation Properties
		public Users User { get; set; }
		public Rooms Room { get; set; }



	}
}
