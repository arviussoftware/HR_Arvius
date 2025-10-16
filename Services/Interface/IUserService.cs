using HR_Arvius.DTOs.Requests.User;

namespace HR_Arvius.Services.Interface
{
    public interface IUserService
    {
        Task<UserDTO> GetUserByEmailAsync(string email);
        Task<List<UserDTO>> GetActiveUsersAsync();
        Task<CreateUserDTO> AddUserAsync(CreateUserDTO user);
        Task<UpdateUserDTO> UpdateUserAsync(UpdateUserDTO user);
    }
}
