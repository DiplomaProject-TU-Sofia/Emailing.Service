using Emailing.Service.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Emailing.Service.Data
{
	public class EmailingDbContext(DbContextOptions<EmailingDbContext> options) : DbContext(options)
	{
		public DbSet<User> Users { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<User>(entity =>
			{
				entity.ToTable("AspNetUsers");
				entity.HasKey(u => u.Id);
			});

		}
	}
}
