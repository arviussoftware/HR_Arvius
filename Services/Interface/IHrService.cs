using HR_Arvius.DTOs.Requests.Employee;
using HR_Arvius.DTOs.Requests.HR;
using HR_Arvius.DTOs.Responses;
using System.Threading.Tasks;

namespace HR_Arvius.Services.Interface
{
    public interface IHrService
    {
        Task<HrDTO> GetHrData();
        Task<HrGridDTO> GetHrOverall();
        Task<HrPendingDTO> GetHrPendingGrid();
        Task<HrReportsDTO> GetHrReports(string? department, DateTime? startDate, DateTime? endDate);
        Task<ManagerListDTO> GetMangerList(); //to get all the currently available managers list.
        Task<bool> ApproveOrReject(int id, string action, string comments);
        Task<CancelLeaveDTO> CancelLeave(int id);
        Task<DeletePolicyDTO> DeletePolicy(int id);
        Task<int> CreateUser(CreateNewUserDTO dto);
        Task<HrEmployeeDTO> GetEmployeeList();
        Task<HrEmployeeDTO> GetInactiveEmployeeList();
        Task<UserPreviewDTO?> GetPreview(int userId);
        Task<bool> UpdateUser(UpdateUserDTO dto);
        Task<bool> UserStatusUpdate(UserStatusMarkDTO dto);
        Task<CompanyHolidayDTO> GetHolidays();
        List<PolicyResponseDTO> GetPolicies();
        Task<bool> CreateHoliday(CreateHolidayDTO dto);
        Task AddPolicy(PolicyRequestDTO request);
        MetaDataDTO GetMetaData();        
    }
}
