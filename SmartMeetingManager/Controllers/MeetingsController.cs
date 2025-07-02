using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartMeetingManager.Data;
using SmartMeetingManager.Models;

namespace SmartMeetingManager.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class MeetingsController : ControllerBase
	{
		private readonly SmartMeetingManagerDbContext dbContext;
		public MeetingsController(SmartMeetingManagerDbContext dbContext)
		{
			this.dbContext = dbContext;
		}

		[HttpGet]
		public IActionResult GetAllMeetings()
		{
			var meetings = dbContext.Meetings.ToList();

			return Ok(meetings);
		}

		// Get Single Meeting
		[HttpGet]
		[Route("{id:int}")]
		public IActionResult GetMeetingById([FromRoute] int id)
		{
			var meeting = dbContext.Meetings.Find(id);

			if (meeting == null)
			{
				return NotFound();
			}
			return Ok(meeting);
		}
	}
}
