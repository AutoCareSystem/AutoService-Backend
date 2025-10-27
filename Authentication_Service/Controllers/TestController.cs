using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_EAD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [Authorize(Roles = "Admin,Employee")]
        [HttpGet("admin")]
        public IActionResult AdminData() => Ok("✅ Admins & Employees only");

        [Authorize(Roles = "User")]
        [HttpGet("user")]
        public IActionResult UserData() => Ok("✅ Users only");
    }
}
