namespace HR_Arvius.DTOs.Requests.Manager
{
    public class ManagerPendingDTO
    {
        public List<ManagerPendingRowDTO> Rows { get; set; } = new List<ManagerPendingRowDTO>();
    }
    public class ManagerPendingRowDTO
    {
        public string employee { get; set; }
        public long employeeId { get; set; }
        public string department { get; set; }
        public string leaveType { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public decimal days { get; set; }
        public string managerStatus { get; set; }
        public DateTime appliedOn{ get; set; }
        public int applicationId { get; set; }
        public string Reason { get; set; }
        public bool halfDay { get; set; }
        public string sessionHalfDay { get; set; }
    }
}
