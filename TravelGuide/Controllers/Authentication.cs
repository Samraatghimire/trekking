using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using TravelGuide.Data;
using TravelGuide.Model;
using TravelGuide.Model.Authentication;
using TravelGuide.Repository;
using TravelGuide.Services;

namespace TravelGuide.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Authentication : ControllerBase
    {
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IUserRepository _userRepo;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;
        public Authentication(
            IJwtTokenService jwtTokenService,
            IEmailService emailService,
            IConfiguration configuration,
            IUserRepository UserRepo,
            AppDbContext context)
        {
            _jwtTokenService = jwtTokenService;
            _emailService = emailService;
            _userRepo = UserRepo;
            _configuration = configuration;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel registerModel)
        {
            var user = await _userRepo.GetByEmailAsync(registerModel.Email);
            if (user != null)
                return BadRequest("User already exists");
            
            string hashedPassword = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(registerModel.Password)));

            var verificationToken = Guid.NewGuid().ToString();

            var newUser = new UserModel
            {
                UserName = registerModel.Username,
                Email = registerModel.Email,
                PasswordHash = hashedPassword,
                Role = "User",
                VerificationToken = verificationToken,
                IsEmailConfirmed = false
            };

            await _userRepo.AddUserAsync(newUser);

            //This is the code that I don't know but this will be used in the code in production 


            var verifyUrl = $"{Request.Scheme}://{Request.Host}/api/authentication/verify?token={verificationToken}";
            await _emailService.SendEmailAsync(registerModel.Email, "Verify Your Email", $"Click here to verify: <a href='{verifyUrl}'>Verify Email</a>");


            return Ok(new { Message = "Registration successfull. Please check your email to verify your account." });
        }

        [HttpGet("verify")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            var user = await _userRepo.GetByVerificationTokenAsync(token);
            if (user == null) return BadRequest("Invalid token");

            user.IsEmailConfirmed = true;
            user.VerificationToken = null;
            await _userRepo.UpdateUserAsync(user);

            return Ok("Email verified successfully");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            var user = await _userRepo.GetByEmailAsync(loginModel.Email);
            if (user == null)
                return Unauthorized("Invalid Credentials");

            if (!user.IsEmailConfirmed)
                return Unauthorized("Email is not verified");

            string hashedPassword = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(loginModel.Password)));

            if(user.PasswordHash != hashedPassword)
                return Unauthorized("Invalid Credentials");

            var token = _jwtTokenService.CreateToken(user);

            // Refresh token logic
            var refreshToken = Guid.NewGuid().ToString();
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                await _userRepo.UpdateUserAsync(user);

                return Ok(new { token, refreshToken });

        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] string refreshToken)
        {
            var user = await _userRepo.GetByRefreshTokenAsync(refreshToken);
            if (user == null || user.RefreshTokenExpiryTime < DateTime.UtcNow)
                return Unauthorized("Invalid or expired refresh token");

            var newToken = _jwtTokenService.CreateToken(user);
            var newRefreshToken = Guid.NewGuid().ToString();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userRepo.UpdateUserAsync(user);

            return Ok(new { token = newToken, refreshToken = newRefreshToken });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel model)
        {
            var user = await _userRepo.GetByEmailAsync(model.Email);
            if (user == null)
                return NotFound("Email not found.");

            user.PasswordHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(model.NewPassword)));

            await _userRepo.UpdateUserAsync(user);

            // Optional: Send confirmation email
             await _emailService.SendEmailAsync(user.Email, "Password Changed", "Your password has been successfully updated.");

            return Ok("Password has been reset successfully.");
        }



    }
}
