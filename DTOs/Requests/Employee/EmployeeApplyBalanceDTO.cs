namespace HR_Arvius.DTOs.Requests.Employee
{
    public class EmployeeApplyBalanceDTO
    {
        public List<EmployeeApplyBalanceRowDTO> Rows { get; set; } = new List<EmployeeApplyBalanceRowDTO>();
    }

    public class EmployeeApplyBalanceRowDTO
    {
        public int leaveId { get; set; }
        public string leaveName { get; set; }
        public decimal remainingDays { get; set; }
        public decimal usedDays { get; set; }
    }
}
