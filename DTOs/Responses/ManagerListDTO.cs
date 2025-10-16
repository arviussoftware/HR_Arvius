using HR_Arvius.DTOs.Requests.HR;

namespace HR_Arvius.DTOs.Responses
{
    public class ManagerListDTO
    {
        public List<ManagerListRowDTO> Rows { get; set; } = new List<ManagerListRowDTO>();
    }

    public class ManagerListRowDTO
    {
        public decimal id { get; set; }
        public string name { get; set; }
    }
}
