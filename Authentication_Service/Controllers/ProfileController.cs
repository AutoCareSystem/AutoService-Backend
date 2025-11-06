using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using backend_EAD.Models;

namespace Authentication_Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;

        public ProfileController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        // GET api/profile/employee/{id}
        [HttpGet("employee/{id}")]
        [Authorize]
        public async Task<IActionResult> GetEmployeeProfile(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            return Ok(new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.PhoneNumber,
                user.Role,
                user.CreatedAt
            });
        }

        // PUT api/profile/employee/{id}
        [HttpPut("employee/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateEmployeeProfile(string id, [FromBody] DTOs.UpdateProfileDto dto)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            // Only allow updating basic fields here
            if (!string.IsNullOrEmpty(dto.UserName)) user.UserName = dto.UserName;
            if (!string.IsNullOrEmpty(dto.Email)) user.Email = dto.Email;
            if (!string.IsNullOrEmpty(dto.PhoneNumber)) user.PhoneNumber = dto.PhoneNumber;
            if (!string.IsNullOrEmpty(dto.Role)) user.Role = dto.Role;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return NoContent();
        }
    }
}
