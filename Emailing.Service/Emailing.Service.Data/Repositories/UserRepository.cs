using Emailing.Service.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Emailing.Service.Data.Repositories
{
	public class UserRepository(EmailingDbContext dbContext)
    {
		public async Task<User> GetUserAsync(Guid userId)
		{
			return await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
		}
    }
}
