namespace HR_Arvius.DTOs.Requests.Employee
{
    public class AddTimeSheetDTO
    {
        public DateTime Date { get; set; } // Date portion
        public TimeSpan StartTime { get; set; } // Only time portion (start)
        public TimeSpan EndTime { get; set; } // Only time portion (end)
        public string Project { get; set; }
        public string Activity { get; set; }
        public string Task { get; set; }
    }
}