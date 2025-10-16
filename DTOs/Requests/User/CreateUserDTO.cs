namespace HR_Arvius.DTOs.Requests.User
{
    public class CreateUserDTO
    {
        public string FirstName { get; set; }
        public string Password { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string EmployeeId { get; set; }
        public string Role { get; set; }
        public string Department { get; set; }
        public string Location { get; set; }
        public int ManagerId { get; set; }
        public DateTime HireDate  { get; set; }
    }
}
