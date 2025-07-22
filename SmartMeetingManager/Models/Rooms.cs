namespace SmartMeetingManager.Models
{
	public class Rooms
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public int Capacity { get; set; }
		public string Location { get; set; }
		public string Status { get; set; } // "Available", "Booked", "Under Maintenance"
		public DateTime CreatedAt { get; set; } = DateTime.Now;
		public ICollection<RoomFeatures> RoomFeatures { get; set; } = new List<RoomFeatures>();
	}
}
