namespace SmartMeetingManager.Models
{
	public class Notifications
	{
		public int Id { get; set; }
		public string Message { get; set; }
		public DateTime StartDate { get; set; } = DateTime.Now;
		public bool IsRead { get; set; }

		// Foreign Keys
		public int UserId { get; set; }

		// Navigation Properties
		public Users User { get; set; }
	}
}
