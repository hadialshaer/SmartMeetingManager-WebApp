using System.ComponentModel.DataAnnotations;

namespace SmartMeetingManager.Models.DTOs
{
	public class CheckAvailabilityDTO
	{
		[Required(ErrorMessage = "Start time is required.")]
		public DateTime StartTime { get; set; }
		[Required(ErrorMessage = "End time is required.")]
		public DateTime EndTime { get; set; }

		[Range(1, 1000, ErrorMessage = "Minimum capacity must be between 1 and 1000.")]
		public int? MinCapacity { get; set; }
	}
}
