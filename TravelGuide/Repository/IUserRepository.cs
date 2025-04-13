using TravelGuide.Model;

namespace TravelGuide.Repository
{
    public interface IUserRepository
    {
        Task<UserModel> GetByEmailAsync(string email);
        Task AddUserAsync(UserModel user);

        Task<UserModel> GetByVerificationTokenAsync(string token);
        Task<UserModel> GetByRefreshTokenAsync(string token);
        Task<UserModel> GetByPasswordResetTokenAsync(int id);

        Task UpdateUserAsync(UserModel user);
    }
}
