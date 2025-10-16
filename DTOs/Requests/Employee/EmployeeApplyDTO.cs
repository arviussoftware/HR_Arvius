using System.ComponentModel.DataAnnotations;

namespace HR_Arvius.DTOs.Requests.Employee
{
    public class EmployeeApplyDTO
    {
        public int leaveType { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public decimal totalDays { get; set; }
        public string reason { get; set; }
        public string documentUrl { get; set; }
        public bool isHalfDay { get; set; }
        public string halfDaySession { get; set; }
    }
}

