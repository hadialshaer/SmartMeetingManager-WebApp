namespace SmartMeetingManager.Models
{
	public class Users
	{
		public int Id { get; set; }
		public string UserName { get; set; }
		public string Email { get; set; }
		public string PasswordHash { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Role { get; set; } // "Admin", "User", "Guest"
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public ICollection<Meetings> Meetings { get; set; } = new List<Meetings>();
		public ICollection<Rooms> Rooms { get; set; } = new List<Rooms>();
	}
}
