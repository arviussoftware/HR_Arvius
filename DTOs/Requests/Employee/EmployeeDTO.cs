namespace HR_Arvius.DTOs.Requests.Employee
{
    public class EmployeeDTO
    {
        public List<EmployeeRowDTO> Rows { get; set; } = new List<EmployeeRowDTO>();
    }
    public class EmployeeRowDTO
    {
        public string LeaveType { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public decimal Days { get; set; }
        public string Status { get; set; }
        public DateTime Applied { get; set; }
        public string Reason { get; set; }
        public string ManagerComments { get; set; }
        public string HrComments { get; set; }
        public bool isHalfDay { get; set; }
        public string sessionHalfDay {get; set;}
        public int ApplicationId { get; set; }
    }
}
