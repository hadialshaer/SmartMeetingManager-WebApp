namespace SmartMeetingManager.Models
{
	public class Decisions
	{
		public int Id { get; set; }
		public string Decision { get; set; }

		// Foreign Keys
		public int MinutesOfMeetingId { get; set; }

		// Navigation Properties
		public MinutesOfMeetings MinutesOfMeeting { get; set; }
	}
}
