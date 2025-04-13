using TravelGuide.Model;

namespace TravelGuide.Services
{
    public interface IJwtTokenService
    {
        string CreateToken(UserModel user);
    }
}
