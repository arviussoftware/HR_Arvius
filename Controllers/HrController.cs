using HR_Arvius.DTOs.Requests;
using HR_Arvius.DTOs.Requests.HR;
using HR_Arvius.Services;
using HR_Arvius.Services.Class;
using HR_Arvius.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace HR_Arvius.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class HrController : ControllerBase
    {
        private readonly EmailService _emailService;
        private readonly IHrService _hrService;

        public HrController(EmailService emailService, IHrService hrService)
        {
            _emailService = emailService;
            _hrService = hrService;
        }

        [HttpGet("dashCount")]
        public async Task<IActionResult> GetHrDataAsync()
        {
            var fetchData = await _hrService.GetHrData();

            return Ok(fetchData);
        }

        [HttpGet("getHolidays")]
        public async Task<IActionResult> GetHolidaysAsync()
        {
            var fetchData = await _hrService.GetHolidays();

            return Ok(fetchData);
        }

        [HttpGet("policies")]
        public IActionResult GetPolicies()
        {
            var data = _hrService.GetPolicies();
            return Ok(new { success = true, data });
        }

        [HttpPost("addPolicy")]
        public async Task<IActionResult> AddPolicyAsync([FromBody] PolicyRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _hrService.AddPolicy(request);
            return Ok(new { success = true, data = request });
        }

        [HttpPost("addHoliday")]
        public async Task<IActionResult> AddHoliday([FromBody] CreateHolidayDTO req)
        {
            try
            {
                var success = await _hrService.CreateHoliday(req);
                if (success)
                    return Ok(new { message = "Holiday created successfully" });

                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to create holiday" });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while creating the holiday", error = ex.Message });
            }
        }


        [HttpDelete("deletePolicy/{policyId}")]
        public async Task<IActionResult> DeletePolicyAsync(int policyId)
        {
            var result = await _hrService.DeletePolicy(policyId);

            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpGet("metaData")]
        public IActionResult GetMetadataAsync()
        {
            var data = _hrService.GetMetaData();
            return Ok(new { success = true, data });
        }

        [HttpGet("overallCount")]
        public async Task<IActionResult> GetHrOverallAsync()
        {
            var fetchData = await _hrService.GetHrOverall();
            return Ok(fetchData);
        }

        //active employee data fetch
        [HttpGet("employeeData")]
        public async Task<IActionResult> GetEmployeeListAsync()
        {
            var fetchData = await _hrService.GetEmployeeList();

            return Ok(fetchData);
        }

        //inactive employee data fetch
        [HttpGet("employeeData/inactive")]
        public async Task<IActionResult> GetInactiveEmployeeListAsync()
        {
            var fetchData = await _hrService.GetInactiveEmployeeList();

            return Ok(fetchData);
        }

        [HttpGet("pendingCount")]
        public async Task<IActionResult> GetHrPendingApprovalsAsync()
        {
            var fetchData = await _hrService.GetHrPendingGrid();

            return Ok(fetchData);
        }

        [HttpGet("reports")]
        public async Task<IActionResult> GetHrReportsAsync([FromQuery] string? department,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
        {
            var data = await _hrService.GetHrReports(department, startDate, endDate);
            return Ok(data);
        }

        [HttpPost("{id}/action")]
        public async Task<IActionResult> HrAction(int id, [FromBody] ApproveRejectRequestDTO dto)
        {
            bool success = await _hrService.ApproveOrReject(id, dto.Action, dto.Comments);

            if (!success)
                return BadRequest("Invalid action or already processed.");

            return Ok(new { Success = true, Message = $"Leave has been {dto.Action.ToUpper()} successfully." });
        }

        //Used when the user wants to cancel the applied while in pending
        [HttpDelete("cancel/{leaveId}")]
        public async Task<IActionResult> CancelLeaveAsync(int leaveId)
        {
            var result = await _hrService.CancelLeave(leaveId);

            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        //New User Creation
        [HttpPost("createUser")]
        public async Task<IActionResult> CreateUserAsync([FromBody] CreateNewUserDTO dto)
        {
            try
            {
                // Ensure default password if none provided
                if (string.IsNullOrEmpty(dto.Password))
                {
                    dto.Password = "password123";
                }

                var userId = await _hrService.CreateUser(dto);
                return Ok(new { UserId = userId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        //User Previewing
        [HttpGet("userPreview/{id}")]
        public async Task<IActionResult> GetPreviewAsync(int id)
        {
            try
            {
                var preview = await _hrService.GetPreview(id);

                if (preview == null)
                    return NotFound(new { Message = $"User with ID {id} not found." });

                return Ok(preview);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An error occurred while fetching user preview.", Details = ex.Message });
            }
        }

        //Fetch manager list
        [HttpGet("managerList")]
        public async Task<IActionResult> GetManagerListAsync()
        {
            var fetchData = await _hrService.GetMangerList();

            return Ok(fetchData);
        }

        //User Updation
        [HttpPut("updateUser/{id}")]
        public async Task<IActionResult> UpdateUserAsync(int id, [FromBody] UpdateUserDTO dto)
        {
            try
            {
                dto.UserId = id;

                var success = await _hrService.UpdateUser(dto);

                if (!success)
                    return BadRequest(new { Message = $"User with ID {id} could not be updated." });

                return Ok(new { Message = $"User with ID {id} updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Error = "An error occurred while updating user.",
                    Details = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        //User status modification
        [HttpPost("markStatus")]
        public async Task<IActionResult> UserStatusUpdateAsync([FromBody] UserStatusMarkDTO dto)
        {
            if (dto == null || dto.UserId <= 0)
                return BadRequest(new { message = "Invalid data input" });

            try
            {
                var success = await _hrService.UserStatusUpdate(dto);
                if (success)
                    return Ok(new { message = "User status updated successfully" });

                return StatusCode(500, new { message = "Failed to update user status" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
