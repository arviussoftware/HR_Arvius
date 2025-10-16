namespace HR_Arvius.DTOs.Requests.HR
{
    public class HrPendingDTO
    {
        public List<HrPendingRowDTO> Rows { get; set; } = new List<HrPendingRowDTO>();
    }
    public class HrPendingRowDTO
    {
        public string Employee { get; set; }
        public string EmployeeId { get; set; }
        public string Department { get; set; }
        public string LeaveType { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public decimal Days { get; set; }
        public string Status { get; set; }
        public DateTime Applied { get; set; }
        public int ApplicationId { get; set; }
        public string Reason { get; set; }
        public bool halfDay { get; set; }
        public string sessionHalfDay { get; set; }
    }
}
