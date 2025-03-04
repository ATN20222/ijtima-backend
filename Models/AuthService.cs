using MeetingPlatform.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MeetingPlatform.Models
{
   
    public class AuthService
    {
        private readonly IConfiguration _config;
        private readonly UserManager<User> _userManager; // Use your custom User class

        public AuthService(IConfiguration config, UserManager<User> userManager)
        {
            _config = config;
            _userManager = userManager;
        }

        public async Task<AuthResponseDto> GenerateJwtToken(User user) // Use your custom User class
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["JwtSettings:Key"]);

            // Fetch user roles asynchronously
            var roles = await _userManager.GetRolesAsync(user);
            Console.WriteLine(string.Join(", ", roles));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName)
            };

            // Add role claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            foreach (var claim in claims)
            {
                Console.WriteLine($"Claim: {claim.Type} - Value: {claim.Value}");
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(_config["JwtSettings:DurationInMinutes"])),
                Issuer = _config["JwtSettings:Issuer"],
                Audience = _config["JwtSettings:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var refreshToken = GenerateRefreshToken();

            return new AuthResponseDto
            {
                Token = tokenHandler.WriteToken(token),
                RefreshToken = refreshToken,
                Expires = tokenDescriptor.Expires.Value
            };
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public async Task SendActivationEmail(string email, string activationLink)
        {
            //var message = new MimeMessage();
            //message.From.Add(new MailboxAddress("EasyLife", "info@ihoneyherb.com"));
            //message.To.Add(new MailboxAddress("", email));
            //message.Subject = "Activate your account";
            //message.Body = new TextPart("plain")
            //{
            //    Text = $"Please activate your account by clicking this link: {activationLink}"
            //};

            //using (var client = new SmtpClient())
            //{
            //    await client.ConnectAsync("host.ihoneyherb.com", 587, SecureSocketOptions.StartTls);
            //    await client.AuthenticateAsync("info@ihoneyherb.com", "iherb@info");
            //    await client.SendAsync(message);
            //    await client.DisconnectAsync(true);
            //}
        }
    
    }

}
