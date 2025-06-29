namespace SmartMeetingManager.Models
{
	public class Agendas
	{
		public int Id { get; set; }
		public string Topic { get; set; }
		public string Description { get; set; }
		public TimeOnly Duration { get; set; }
		public int Order { get; set; }

		// Foreign Keys
		public int MeetingId { get; set; }

		// Navigation Properties
		public Meetings Meeting { get; set; }
	}
}
