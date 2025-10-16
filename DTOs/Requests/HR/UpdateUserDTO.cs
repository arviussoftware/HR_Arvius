namespace HR_Arvius.DTOs.Requests.HR
{
    public class UpdateUserDTO
    {
        public decimal UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public string? Department { get; set; }
        public string? Location { get; set; }
        public int? ManagerId { get; set; }
        public DateTime? HireDate { get; set; }
        public bool? IsActive { get; set; }
        public string? Skills { get; set; }
        public string? EduDegree { get; set; }
        public string? EduBranch { get; set; }
        public string? EduUniversity { get; set; }
        public string? EduYear { get; set; }
        public decimal? EduGrade { get; set; }
        public decimal? ContactNumber { get; set; }
        public string? AddressPermanent { get; set; }
        public string? AddressPresent { get; set; }
        public string? EmergencyContactName { get; set; }
        public decimal? EmergencyContactNumber { get; set; }
        public string? EmergencyContactRelationship { get; set; }
        public string? EmergencyContactAddress { get; set; }
    }
}
