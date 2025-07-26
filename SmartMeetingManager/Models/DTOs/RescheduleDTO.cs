namespace SmartMeetingManager.Models.DTOs
{
	public class RescheduleDTO
	{
		public DateTime NewStartTime { get; set; }
		public DateTime NewEndTime { get; set; }
		public int? NewRoomID { get; set; }
	}
}
