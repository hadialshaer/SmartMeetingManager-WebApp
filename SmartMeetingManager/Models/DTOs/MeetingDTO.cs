using System.ComponentModel.DataAnnotations;

namespace SmartMeetingManager.Models.DTOs
{
	public class MeetingDTO
	{

		public int Id { get; set; }

		[Required(ErrorMessage = "Title is required.")]
		public string Title { get; set; }

		[Required(ErrorMessage = "Start Time is required.")]
		public DateTime StartTime { get; set; }

		[Required(ErrorMessage = "End Time is required.")]
		public DateTime EndTime { get; set; }

		public string Status { get; set; } = "Scheduled";

		[Required(ErrorMessage = "Organizer is required.")]
		public string OrganizerName { get; set; }

		[Required(ErrorMessage = "Room is required.")]
		public string RoomName { get; set; }

		public List<string> Attendees { get; set; } = new List<string>();
		public List<string> Agendas { get; set; } = new List<string>();
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	}
}
