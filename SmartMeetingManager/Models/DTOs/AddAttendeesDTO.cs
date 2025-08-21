using System.ComponentModel.DataAnnotations;

namespace SmartMeetingManager.Models.DTOs
{
	public class AddAttendeesDTO
	{
		[Required(ErrorMessage = "At least one User is required.")]
		public List<int> UserIds { get; set; } = [];
	}
}
