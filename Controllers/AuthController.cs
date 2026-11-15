using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using AuthDemo.Data;
using AuthDemo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AuthDemo.Controllers;

public class RegisterDto
{
    [Required, StringLength(50, MinimumLength = 2)] public string Name { get; set; } = "";
    [Required, EmailAddress, StringLength(254)] public string Email { get; set; } = "";
    [Required, StringLength(128)] public string Password { get; set; } = "";
}

public class LoginDto
{
    [Required] public string Email { get; set; } = "";
    [Required] public string Password { get; set; } = "";
}

[ApiController]
[Route("api/auth")]
public class AuthController(AppDb db, IConfiguration cfg) : ControllerBase
{
    // min 8 chars, at least: 1 lowercase, 1 UPPERCASE, 1 digit, 1 special char (_ - ! @ # ...)
    static readonly Regex StrongPassword =
        new(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$", RegexOptions.Compiled);

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (!StrongPassword.IsMatch(dto.Password))
            return BadRequest(new { error = "Password must be 8+ chars with a-z, A-Z, 0-9 and a special character (e.g. _ or -)." });

        var email = dto.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(u => u.Email == email))
            return Conflict(new { error = "Email is already registered." });

        var user = new User
        {
            Name = dto.Name.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        return Ok(new { token = CreateToken(user), user = new { user.Name, user.Email, user.Role } });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);

        // Same message for "no such user" and "wrong password" so attackers can't probe emails
        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized(new { error = "Invalid email or password." });

        return Ok(new { token = CreateToken(user), user = new { user.Name, user.Email, user.Role } });
    }

    string CreateToken(User u)
    {
        var jwt = cfg.GetSection("Jwt");
        var claims = new[]
        {
            new Claim("sub", u.Id.ToString()),
            new Claim("name", u.Name),
            new Claim("email", u.Email),
            new Claim("role", u.Role)
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(jwt["ExpireMinutes"]!)),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
