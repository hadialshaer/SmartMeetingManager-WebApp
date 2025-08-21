using System.ComponentModel.DataAnnotations;

namespace SmartMeetingManager.Models.DTOs

{
	public class AgendaItemDTO
	{
		[Required(ErrorMessage = "Topic is required.")]
		public string Topic { get; set; } = string.Empty;


		[MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
		public string? Description { get; set; }
		[Range(1, 1440, ErrorMessage = "Duration must be between 1 minute and 24 hours (1440 minutes).")]
		public int Duration { get; set; }

		public int? Order { get; set; }


	}
}
