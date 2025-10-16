using HR_Arvius.DTOs.Requests.Employee;

namespace HR_Arvius.DTOs.Responses
{
    public class TimeSheetRespDTO
    {
        public List<TimeSheetRespRowDTO> Rows { get; set; } = new List<TimeSheetRespRowDTO>();
        public string ErrorMessage { get; set; }
    }
    public class TimeSheetRespRowDTO
    {
        public int Id { get; set; }  // Primary Key (Auto-Incremented)
        public int UserId { get; set; }
        public string ProjectName { get; set; }
        public string Activity { get; set; }
        public DateTime ColDate { get; set; }  // Store just the date part
        public TimeSpan StartTime { get; set; }  // Store just the time part
        public TimeSpan EndTime { get; set; }  // Store just the time part
        public string Task { get; set; }
    }
}
