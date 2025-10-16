namespace HR_Arvius.DTOs.Responses
{
    public class CompanyHolidayDTO
    {
        public List<CompanyHolidayRowDTO> Rows { get; set; } = new List<CompanyHolidayRowDTO>();
    }

    public class CompanyHolidayRowDTO
    {
        public string HolidayName { get; set; }
        public DateTime HolidayDate { get; set; }
        public string HolidayDay { get; set; }
        public bool HolidayType { get; set; }
    }
}
