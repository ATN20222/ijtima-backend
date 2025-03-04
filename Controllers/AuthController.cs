using MeetingPlatform.Data;
using MeetingPlatform.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly AuthService _authService;
    private readonly UserManager<User> _userManager;


    public AuthController(UserManager<User> userManager, ApplicationDbContext context, AuthService authService)
    {
        _context = context;
        _userManager = userManager;
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
        if (existingUser != null)
        {
            var errorResponse = new
            {
                Status = "Error",
                description = "Email already in use."
            };
            return BadRequest(errorResponse);
        }

        var user = new User
        {
            UserName = registerDto.Username,
            Email = registerDto.Email
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return Ok("User registered successfully.");
    }


    [HttpPost("registerWithConfirm")]
    public async Task<IActionResult> RegisterWithConfirm([FromBody] RegisterDto registerDto)
    {
        var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
        if (existingUser != null)
        {
            var errorResponse = new
            {
                Status = "Error",
                description = "Email already in use."
            };
            return BadRequest(errorResponse);
        }

        var user = new User
        {
            UserName = registerDto.Username,
            Email = registerDto.Email,
            EmailConfirmed = false
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }
        // Send activation email
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var activationLink = Url.Action("ActivateAccount", "Auth", new { userId = user.Id, token }, Request.Scheme);
        await _authService.SendActivationEmail(user.Email, activationLink);

        return Ok("User registered successfully. Please check your email to activate your account.");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null)
        {
            return Unauthorized("Invalid credentials.");
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);
        if (!isPasswordValid)
        {
            return Unauthorized("Invalid credentials.");
        }

        if (!user.EmailConfirmed)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var activationLink = Url.Action("ActivateAccount", "Auth", new { userId = user.Id, token }, Request.Scheme);
            await _authService.SendActivationEmail(user.Email, activationLink);

            return BadRequest("Email not activated. Please check your inbox for a confirmation email.");
        }

        // Generate JWT and Refresh Token
        var authResponse = await _authService.GenerateJwtToken(user); // Ensure this uses the custom User

        // Return the JWT token along with the UserName

        var response = new
        {
            userName = user.UserName,
            token = authResponse.Token,
            refreshToken = authResponse.RefreshToken

        };

        return Ok(response);
    }


    [HttpGet("Role")]
    public ActionResult<string> GetRole()
    {
        var user = User;
        var roleClaim = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");

        if (roleClaim != null)
        {
            return Ok(roleClaim.Value);
        }
        var name = User.Identity.Name;
        return BadRequest(name);
    }

    [HttpGet("activate")]
    public async Task<IActionResult> ActivateAccount(string userId, string token)
    {
        // Logic to confirm email and activate the account
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return BadRequest("Invalid user.");
        }

        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (result.Succeeded)
        {
            return Ok("Account activated successfully.");
        }

        return BadRequest("Error activating account.");
    }



}

public class RegisterDto
{
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class LoginDto
{
        public string Email { get; set; }
        public string Password { get; set; }
    }
