using Microsoft.EntityFrameworkCore;
using SmartMeetingManager.Models;

namespace SmartMeetingManager.Data
{
	public class SmartMeetingManagerDbContext : DbContext
	{
		public SmartMeetingManagerDbContext(DbContextOptions<SmartMeetingManagerDbContext> options): base(options)
		{

			
		}
		public DbSet<Users> Users { get; set; }
		public DbSet<Rooms> Rooms { get; set; }
		public DbSet<Features> Features { get; set; }
		public DbSet<RoomFeatures> RoomFeatures { get; set; }
		public DbSet<Meetings> Meetings { get; set; }
		public DbSet<MinutesOfMeetings> MinutesOfMeetings { get; set; }
		public DbSet<MeetingAttendees> MeetingAttendees { get; set; }
		public DbSet<Agendas> Agendas { get; set; }
		public DbSet<Attachments> Attachments { get; set; }
		public DbSet<ActionItems> ActionItems { get; set; }
		public DbSet<DiscussionPoints> DiscussionPoints { get; set; }
		public DbSet<Decisions> Decisions { get; set; }
		public DbSet<Notifications> Notifications { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// Prevent delete for Users when a MeetingAttendees referencing it
			modelBuilder.Entity<MeetingAttendees>()
				.HasOne(ma => ma.User)
				.WithMany()
				.HasForeignKey(ma => ma.UserId)
				.OnDelete(DeleteBehavior.Restrict);

			// Prevent delete users when action items referencing it
			modelBuilder.Entity<ActionItems>()
				.HasOne(ai => ai.User)
				.WithMany()
				.HasForeignKey(ai => ai.UserId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
