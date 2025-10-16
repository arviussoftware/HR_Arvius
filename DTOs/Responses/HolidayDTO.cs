using HR_Arvius.DTOs.Requests.HR;

namespace HR_Arvius.DTOs.Responses
{
    public class HolidayDTO
    {
        public int id { get; set; }
        public string Name { get; set; }
        public DateTime HolidayDate { get; set; }
        public string HolidayDay { get; set; }
    }
}
