using System.ComponentModel.DataAnnotations;
namespace SmartMeetingManager.Models.DTOs
{
	public class CreateMeetingDTO
	{
		[Required(ErrorMessage = "Title is required.")]
		public string Title { get; set; }

		[Required(ErrorMessage="Start Time is required")]
		public DateTime StartTime { get; set; }

		[Required(ErrorMessage = "End Time is required")]
		public DateTime EndTime { get; set; }

		[Required(ErrorMessage = "Room is required.")]
		public int UserId { get; set; }

		[Required(ErrorMessage = "Organizer is required.")]
		public int RoomId { get; set; }
	}
}
