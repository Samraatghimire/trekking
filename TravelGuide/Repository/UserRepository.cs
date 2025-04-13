using Microsoft.EntityFrameworkCore;
using TravelGuide.Data;
using TravelGuide.Model;

namespace TravelGuide.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        public UserRepository(AppDbContext context)
        {
            _context = context;
        }


        public async Task<UserModel> GetByEmailAsync(string email)  // this is the method where the user are taken from the email.
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task AddUserAsync(UserModel user)  // this is the method to add a new user in the database
        {
            _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task<UserModel> GetByVerificationTokenAsync(string token)  //this is the method to get user by verification token
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.VerificationToken == token);
        }

        public async Task<UserModel> GetByRefreshTokenAsync(string token)  // Get user by refresh token
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == token);
        }

        public async Task<UserModel> GetByPasswordResetTokenAsync(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.PasswordResetToken != null);
        }

        public async Task UpdateUserAsync(UserModel user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}
