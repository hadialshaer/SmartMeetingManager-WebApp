namespace SmartMeetingManager.Models
{
	public class Attachments
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Path { get; set; }
		public string Type { get; set; } // "image", "document"
		public DateTime UploadDate { get; set; } = DateTime.UtcNow;

		// Foreign Keys
		public int MinutesOfMeetingId { get; set; }

		// Navigation Properties
		public MinutesOfMeetings MinutesOfMeeting { get; set; }
	}
}
