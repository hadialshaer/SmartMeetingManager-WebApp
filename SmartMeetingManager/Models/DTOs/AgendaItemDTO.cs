namespace SmartMeetingManager.Models.DTOs
{
	public class AgendaItemDTO
	{
		public string Topic { get; set; } = string.Empty;
		public string? Description { get; set; }
		public int Duration { get; set; }
		public int Order { get; set; }


	}
}
