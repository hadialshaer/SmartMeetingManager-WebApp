namespace SmartMeetingManager.Models.DTOs
{
	public class CheckAvailabilityDTO
	{
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public int? MinCapacity { get; set; }
	}
}
