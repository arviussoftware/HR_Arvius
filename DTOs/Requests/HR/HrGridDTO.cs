namespace HR_Arvius.DTOs.Requests.HR
{
    public class HrGridDTO
    {
        public List<HrGridRowDTO> Rows { get; set; } = new List<HrGridRowDTO>();
    }

    public class HrGridRowDTO
    {
        public string Employee { get; set; }
        public string EmployeeId { get; set; }
        public string Department { get; set; }
        public string LeaveType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Days { get; set; }
        public string Reason { get; set; }
        public string ManagerComments { get; set; }
        public string HrComments { get; set; }
        public string Status { get; set; }
        public DateTime AppliedOn { get; set; }
        public bool halfDay { get; set; }
        public string sessionHalfDay { get; set; }
    }
}
