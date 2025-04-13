using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace TravelGuide.Model
{
    public class UserModel
    {
        public int Id { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public string Role { get; set; } = "User";

        public bool IsEmailConfirmed { get; set; } = false;
        public string? VerificationToken { get; set; }
        public string? PasswordResetToken { get; set; }

        public string? RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiryTime { get; set; }

        [Required, MaxLength(70)]
        public string UserName { get; set; }




    }
}
