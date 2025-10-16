using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using HR_Arvius.Services.Interface;
using HR_Arvius.DTOs.Requests.User;

namespace HR_Arvius.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetActiveUsers()
        {
            var users = await _userService.GetActiveUsersAsync();

            if (users == null || users.Count == 0)
                return NotFound("No active users found.");

            return Ok(users);
        }

        [HttpPost("addUser")]
        public async Task<IActionResult> AddNewData([FromBody] CreateUserDTO newUser)
        {
            if (newUser == null) 
                return BadRequest("User Data is required.");
            try
            {
                var user = await _userService.AddUserAsync(newUser);
                return Ok(new { message = "User added successfully.", user = user });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return StatusCode(500, new { message = "New user could not be added.", error = ex.Message });
            }
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDTO updateUser)
        {
            if( id != updateUser.Id)
            {
                return BadRequest("User Id mismatch.");
            }
            try
            {
                var updatedUser = await _userService.UpdateUserAsync(updateUser);
                if(updatedUser == null)
                {
                    return NotFound("User not found.");
                }
                return Ok(updatedUser);
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
