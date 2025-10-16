namespace HR_Arvius.DTOs.Requests.Employee
{
    public class EmployeeLeaveDTO
    {
        public List<EmployeeLeaveRowDTO> Rows { get; set; } = new List<EmployeeLeaveRowDTO>();
    }
    public class EmployeeLeaveRowDTO
    {
        public int id { get; set; }
        public string name { get; set; }
        public string code { get; set; }
        public bool requiresDocument { get; set; }
    }
}
