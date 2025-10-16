namespace HR_Arvius.DTOs.Responses
{
    public class LeaveApplyResultDTO
    {
        public string EmployeeName { get; set; }
        public string EmployeeEmail { get; set; }
        public string LeaveType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalDays { get; set; }
        public bool Success { get; set; }
    }
}
