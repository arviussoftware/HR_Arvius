using HR_Arvius.DTOs.Requests.Employee;
using HR_Arvius.Services;
using HR_Arvius.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HR_Arvius.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly EmailService _emailService;
        private readonly IEmployeeService _employeeService;

        public EmployeeController(EmailService emailService, IEmployeeService employeeService)
        {
            _emailService = emailService;
            _employeeService = employeeService;
        }

        //to fetch restricted holidays for the particular calendar year
        [HttpGet("restricted")]
        public async Task<IActionResult> GetRestrictedHolidays()
        {
            try
            {
                var holidays = await _employeeService.GetRestrictedHolidaysAsync();
                return Ok(new { rows = holidays });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to fetch restricted holidays", details = ex.Message });
            }
        }

        //Used to fetch the leave balance for a particular user, for displaying it on the dash - stats
        [HttpGet("leaveBalance/{userId}")]
        public async Task<IActionResult> GetLeaveCountAsync(int userId)
        {
            var fetchData = await _employeeService.GetLeaveCount(userId);

            return Ok(fetchData);
        }

        //Used for fetching all the applications ever made by the employee
        [HttpGet("myApplications/{userId}")]
        public async Task<IActionResult> GetEmployeeDataAsync(int userId)
        {
            var fetchData = await _employeeService.GetEmployeeData(userId);

            return Ok(fetchData);
        }

        //Used for applying leaves by the employee
        [HttpPost("Apply/{userId}")]
        public async Task<IActionResult> AddLeave(int userId, [FromBody] EmployeeApplyDTO newLeave)
        {
            var result = await _employeeService.AddLeaveAsync(userId, newLeave);

            if (result.Success)
            {
                string toEmail = result.EmployeeEmail ?? "hr@yourcompany.com";
                string subject = "Leave Application Submitted";
                string htmlBody = $@"
                <h3>Hi {result.EmployeeName},</h3>
                <p>Your leave application has been submitted successfully.</p>
                <p><b>Leave Type:</b> {result.LeaveType}</p>
                <p><b>From:</b> {result.StartDate:dd MMM yyyy}</p>
                <p><b>To:</b> {result.EndDate:dd MMM yyyy}</p>
                <p><b>Total Days:</b> {result.TotalDays}</p>
                <br/>
                <p>Best Regards,<br/>HR Team</p>";

                _emailService.SendEmailFireAndForget(toEmail, subject, htmlBody);
            }

            return Ok(result);
        }


        //Used for fetching the type of leaves available
        [HttpGet("leaveType/names/{userId}")]
        public async Task<IActionResult> GetLeaveType(int userId)
        {
            var data = await _employeeService.GetLeaveTypeAsync(userId);

            return Ok(data);
        }

        //Used when the user wants to cancel the applied while in pending
        [HttpDelete("cancel/{leaveId}")]
        public async Task<IActionResult> CancelLeaveAsync(int leaveId)
        {
            var result = await _employeeService.CancelLeave(leaveId);

            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        //Used while applying for leave
        [HttpGet("leaveApplyBalance/{userId}")]
        public async Task<IActionResult> GetLeaveBalanceAsync(int userId)
        {
            var data = await _employeeService.GetLeaveBalance(userId);
            return Ok(data);
        }

        [HttpPost("resetPassword")]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] EmployeeResetPasswordDTO dto)
        {
            if (dto == null || dto.userId <= 0 || string.IsNullOrWhiteSpace(dto.oldPassword) || string.IsNullOrWhiteSpace(dto.newPassword))
            {
                return BadRequest("Invalid request data.");
            }

            var success = await _employeeService.ResetPassword(dto.userId, dto.oldPassword, dto.newPassword);

            if (success)
                return Ok(new { message = "Password updated successfully." });
            else
                return StatusCode(500, new { message = "Failed to update password." });
        }

        [HttpPost("{userId}/uploadPhoto")]
        public async Task<IActionResult> UploadPhoto(int userId, [FromBody] UploadPhotoDTO dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Base64Image))
                return BadRequest("No image provided.");

            byte[] imageBytes;
            try
            {
                imageBytes = Convert.FromBase64String(dto.Base64Image);
            }
            catch (FormatException)
            {
                return BadRequest("Invalid Base64 string.");
            }

            var success = await _employeeService.UploadProfilePhotoBytes(userId, imageBytes);

            if (success) return Ok(new { message = "Photo uploaded successfully." });
            return StatusCode(500, new { message = "Failed to upload photo." });
        }

        [HttpGet("{userId}/photo")]
        public async Task<IActionResult> GetPhoto(int userId)
        {
            var photoBytes = await _employeeService.GetProfilePhoto(userId);
            if (photoBytes == null) return NotFound("No profile photo found.");

            // You can dynamically detect MIME type if needed. For simplicity, using image/png.
            return File(photoBytes, "image/png");
        }

        [HttpGet("timesheets/{userId}")]
        public async Task<IActionResult> GetTimeSheetAsync(int userId)
        {
            try
            {
                var timesheet = await _employeeService.GetTimeSheet(userId);
                if (timesheet == null || timesheet.Rows.Count == 0)
                {
                    return NotFound(new { message = "No timesheets found for this user." });
                }
                return Ok(new { rows = timesheet.Rows });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to fetch timesheets", details = ex.Message });
            }
        }

        [HttpPost("{userId}/addTimesheet")]
        public async Task<IActionResult> AddTimeSheet([FromRoute] int userId, [FromBody] AddTimeSheetDTO addTimeSheetDTO)
        {
            if (addTimeSheetDTO == null || !ModelState.IsValid)
            {
                return BadRequest("Invalid request data.");
            }

            try
            {
                // Call the service method to add the timesheet
                bool success = await _employeeService.AddTimeSheetAsync(userId, addTimeSheetDTO.Project, addTimeSheetDTO.Activity, addTimeSheetDTO.Date, addTimeSheetDTO.StartTime, addTimeSheetDTO.EndTime, addTimeSheetDTO.Task);

                // Return success or failure response
                if (success)
                {
                    return Ok(new { message = "Timesheet added successfully." });
                }
                else
                {
                    return StatusCode(500, new { message = "Failed to add timesheet." });
                }
            }
            catch (Exception ex)
            {
                // Return detailed error information
                return StatusCode(500, new { error = "Error adding timesheet", details = ex.Message });
            }
        }
    }
}
