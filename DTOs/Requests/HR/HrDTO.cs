namespace HR_Arvius.DTOs.Requests.HR
{
    public class HrDTO
    {
        public int TotalEmployees { get; set; }
        public int TotalLeaveTypes { get; set; }
        public int PendingApplications { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }
    }
}
