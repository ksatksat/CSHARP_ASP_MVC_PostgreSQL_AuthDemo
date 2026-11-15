using AuthDemo.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthDemo.Controllers;

[ApiController]
[Route("api/account")]
[Authorize] // every action here requires a valid JWT -> otherwise 401
public class AccountController(AppDb db) : ControllerBase
{
    int CurrentUserId => int.Parse(User.FindFirst("sub")!.Value);

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var u = await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == CurrentUserId);
        return u is null
            ? Unauthorized()
            : Ok(new { u.Name, u.Email, u.Role, u.CreatedAt });
    }

    // Authorization: you can only ever delete YOUR OWN account (id comes from the token, not the URL)
    [HttpDelete]
    public async Task<IActionResult> Delete()
    {
        var deleted = await db.Users.Where(x => x.Id == CurrentUserId).ExecuteDeleteAsync();
        return deleted == 0 ? NotFound() : NoContent();
    }

    // Role-based authorization example: only users with Role = "Admin" get in
    [HttpGet("admin-only")]
    [Authorize(Roles = "Admin")]
    public IActionResult AdminOnly() => Ok(new { message = "Hello, admin!" });
}
