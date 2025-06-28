using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SmartMeetingManager.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class MeetingsController : ControllerBase
	{
		[HttpGet]
		public IActionResult GetAllMeetings()
		{
			string[] MeetingNames = { "Project Kickoff", "Weekly Sync", "Client Presentation" };
			return Ok(MeetingNames);
		}
	}
}
