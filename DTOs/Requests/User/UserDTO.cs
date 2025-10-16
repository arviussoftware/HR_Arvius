namespace HR_Arvius.DTOs.Requests.User
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string EmployeeId { get; set; }
        public string Role { get; set; }
        public string Department { get; set; }
    }
}