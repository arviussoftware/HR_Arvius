namespace HR_Arvius.DTOs.Requests.Manager
{
    public class ManagerDTO
    {
        public List<ManagerRowDTO> Rows { get; set; } = new List<ManagerRowDTO>();
    }
    public class ManagerRowDTO
    {
        public string Employee { get; set; }
        public string EmployeeId { get; set; }
        public string Department { get; set; }
        public string LeaveType { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public decimal Days { get; set; }
        public string Status { get; set; }
        public string Reason { get; set; }
        public string ManagerComments { get; set; }
        public string HrComments { get; set; }
        public bool HalfDay { get; set; }
        public string SessionHalfDay { get; set; }
        public DateTime AppliedOn { get; set; }
    }
}
