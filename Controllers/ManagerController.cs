using HR_Arvius.DTOs.Requests.Manager;
using HR_Arvius.Services.Class;
using HR_Arvius.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HR_Arvius.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class ManagerController : ControllerBase
    {
        private readonly IManagerService _mgService;

        public ManagerController(IManagerService mgService)
        {
            _mgService = mgService;
        }

        //active employee data fetch
        [HttpGet("team/{userId}")]
        public async Task<IActionResult> GetEmployeeListAsync(int userId)
        {
            var fetchData = await _mgService.GetEmployeeListManager(userId);

            return Ok(fetchData);
        }

        [HttpGet("applications/{userId}")]
        public async Task<IActionResult> GetManagerDataAsync(int userId)
        {
            var fetchData = await _mgService.GetManagerData(userId);

            return Ok(fetchData);
        }

        [HttpGet("pending/{userId}")]
        public async Task<IActionResult> GetManagerPendingDataAsync(int userId)
        {
            var data = await _mgService.GetPending(userId);

            return Ok(data);
        }

        [HttpPost("{appId}/action")]
        public async Task<IActionResult> ActionOnApplicationAsync(int appId, [FromBody] ManagerLeaveActionDTO dto)
        {
            var success = await _mgService.ActionOnApplication(appId, dto);

            if (success == true)
                return Ok(new { Success = true, Message = $"Leave has been {dto.Action.ToUpper()} successfully." });

            return BadRequest("Invalid action or already processed.");
        }
    }
}
