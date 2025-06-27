namespace SmartMeetingManager.Models
{
	public class DiscussionPoints
	{
		public int Id { get; set; }
		public string Point { get; set; }

		// Foreign Keys
		public int MinutesOfMeetingId { get; set; }

		// Navigation Properties
		public MinutesOfMeetings MinutesOfMeeting { get; set; }
	}
}
