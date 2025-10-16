namespace HR_Arvius.DTOs.Requests.HR
{
    public class HrReportsDTO
    {
        public List<HrReportsRowDTO> Rows { get; set; } = new List<HrReportsRowDTO>();
    }
    public class HrReportsRowDTO
    {
        public string Employee { get; set; }
        public string EmployeeId { get; set; }
        public string Department { get; set; }
        public string LeaveType { get; set; }
        public int Applications { get; set; }
        public decimal TotalDays { get; set; }        
        public decimal AverageDays { get; set; }
        public DateTime StartLeaveDate { get; set; }
        public DateTime EndLeaveDate { get; set; }
    }
}
