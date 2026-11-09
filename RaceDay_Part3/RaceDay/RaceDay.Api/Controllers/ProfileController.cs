using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Part2_Raceday_API.Data;
using Part2_Raceday_API.Filters;
using Part2_Raceday_API.Services;
using RaceDay.Contracts;
using Swashbuckle.AspNetCore.Annotations;

namespace Part2_Raceday_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[SwaggerTag("User Profile - View and update own profile + profile picture")]
public class ProfileController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public ProfileController(RaceDayDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get the currently logged-in user's profile.
    /// </summary>
    [HttpGet("me")]
    [AuthorizeRole]
    [SwaggerOperation(Summary = "Get my profile")]
    [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = HttpContext.Session.GetInt32("UserId")!.Value;

        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
            return NotFound(new { message = "User not found." });

        return Ok(new ProfileDto
        {
            UserId = user.UserId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role.Name,
            ProfileImageUrl = user.ProfileImageUrl
        });
    }

    /// <summary>
    /// Update the currently logged-in user's own profile information.
    /// </summary>
    [HttpPut("me")]
    [AuthorizeRole]
    [SwaggerOperation(Summary = "Update my profile")]
    [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = HttpContext.Session.GetInt32("UserId")!.Value;

        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
            return NotFound(new { message = "User not found." });

        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
            return BadRequest(new { message = "First name and last name are required." });

        if (!string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase))
        {
            var emailTaken = await _context.Users.AnyAsync(u => u.Email.ToLower() == request.Email.Trim().ToLower() && u.UserId != userId);
            if (emailTaken)
                return BadRequest(new { message = "Email is already in use by another account." });
        }

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.Email = request.Email.Trim().ToLower();

        await _context.SaveChangesAsync();

        return Ok(new ProfileDto
        {
            UserId = user.UserId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role.Name,
            ProfileImageUrl = user.ProfileImageUrl
        });
    }

    /// <summary>
    /// Upload profile picture. Goes through Azure Blob Storage.
    /// </summary>
    [HttpPost("me/picture")]
    [AuthorizeRole]
    [SwaggerOperation(Summary = "Upload profile picture")]
    [ProducesResponseType(typeof(BlobUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadProfilePicture(IFormFile file, [FromServices] BlobStorageService blobService)
    {
        var userId = HttpContext.Session.GetInt32("UserId")!.Value;

        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (user == null)
            return NotFound(new { message = "User not found." });

        try
        {
            var url = await blobService.UploadProfilePictureAsync(file, userId);
            user.ProfileImageUrl = url;
            await _context.SaveChangesAsync();

            return Ok(new BlobUploadResponse
            {
                Url = url,
                Message = "Profile picture uploaded successfully."
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Upload failed: " + ex.Message });
        }
    }
}
