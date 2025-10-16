using HR_Arvius.DTOs.Requests.HR;

namespace HR_Arvius.DTOs.Responses
{
    public class PolicyResponseDTO
    {
        public long id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string type { get; set; }
        public string uploadedBy { get; set; }
        public DateTime uploadedAt { get; set; }
        public List<string> tags { get; set; }
        public string? content { get; set; }
        public string? pdfData { get; set; }
        public string? fileName { get; set; }
    }

}
