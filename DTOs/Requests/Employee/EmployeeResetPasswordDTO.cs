namespace HR_Arvius.DTOs.Requests.Employee
{
    public class EmployeeResetPasswordDTO
    {
        public int userId { get; set; }
        public string oldPassword { get; set; } = string.Empty;
        public string newPassword { get; set; } = string.Empty;
    }
}
