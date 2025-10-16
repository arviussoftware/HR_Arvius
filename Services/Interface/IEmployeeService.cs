using HR_Arvius.DTOs.Requests.Employee;
using HR_Arvius.DTOs.Responses;
namespace HR_Arvius.Services.Interface
{
    public interface IEmployeeService
    {
        Task<EmployeeDTO> GetEmployeeData(int userId);
        Task<LeaveApplyResultDTO> AddLeaveAsync(int id, EmployeeApplyDTO newLeave);
        Task<EmployeeBalanceDTO> GetLeaveCount(int id);
        Task<EmployeeLeaveDTO> GetLeaveTypeAsync(int id);
        Task<EmployeeCancelLeaveDTO> CancelLeave(int id);
        Task<EmployeeApplyBalanceDTO> GetLeaveBalance(int id);
        Task<bool> ResetPassword(int id, string hPass, string pass);
        Task<List<HolidayDTO>> GetRestrictedHolidaysAsync();
        Task<bool> UploadProfilePhotoBytes(int userId, byte[] imageBytes);
        Task<byte[]> GetProfilePhoto(int userId);
        Task<TimeSheetRespDTO> GetTimeSheet(int userId);
        Task<bool> AddTimeSheetAsync(int userId, string project, string activity, DateTime date, TimeSpan startTime, TimeSpan endTime, string task);
    }
}
