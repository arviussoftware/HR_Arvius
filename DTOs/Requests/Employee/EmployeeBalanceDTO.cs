namespace HR_Arvius.DTOs.Requests.Employee
{
    public class EmployeeBalanceDTO
    {
        public decimal CasualUsed { get; set; }
        public decimal EarnedUsed { get; set; }
        public decimal SickUsed { get; set; }

        public decimal CasualRemaining { get; set; }
        public decimal EarnedRemaining { get; set; }
        public decimal SickRemaining { get; set; }

        public decimal CasualAllocated { get; set; }
        public decimal EarnedAllocated{ get; set; }
        public decimal SickAllocated{ get; set; }
    }
}
