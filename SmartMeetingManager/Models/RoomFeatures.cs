namespace SmartMeetingManager.Models
{
	public class RoomFeatures
	{
		public int Id { get; set; }

		// Foreign Keys
		public int RoomId { get; set; }
		public int FeatureId { get; set; }

		// Navigation Properties
		public Rooms Room { get; set; }
		public Features Feature { get; set; }
	}
}
