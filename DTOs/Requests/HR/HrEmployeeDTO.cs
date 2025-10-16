namespace HR_Arvius.DTOs.Requests.HR
{
    public class HrEmployeeDTO{
        public List<HrEmployeeRowDTO> Rows { get; set; } = new List<HrEmployeeRowDTO>();
    }
    public class HrEmployeeRowDTO
    {
        public string id { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string Department { get; set; }
        public string EmpId { get; set; }
        public int ManagerId { get; set; }
        public string Location { get; set; }
        public string Email { get; set; }
    }
}
