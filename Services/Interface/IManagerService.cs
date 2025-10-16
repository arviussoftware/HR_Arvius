using HR_Arvius.DTOs.Requests.HR;
using HR_Arvius.DTOs.Requests.Manager;
namespace HR_Arvius.Services.Interface
{
    public interface IManagerService
    {
        Task<ManagerDTO> GetManagerData(int userId);

        Task<ManagerPendingDTO> GetPending(int userId);
        Task<bool?> ActionOnApplication(int appId, ManagerLeaveActionDTO dto);
        Task<HrEmployeeDTO> GetEmployeeListManager(int id);

    }
}
