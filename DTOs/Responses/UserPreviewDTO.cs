namespace HR_Arvius.DTOs.Responses
{
    public class UserPreviewDTO
    {
        public decimal Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public decimal ContactNumber { get; set; }
        public string Gender { get; set; }
        public string BloodGroup { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string AddressPermanent { get; set; }
        public string AddressPresent { get; set; }
        public string EmployeeId { get; set; }
        public DateTime HireDate { get; set; }
        public string Department { get; set; }
        public string Role { get; set; }
        public int? ManagerId { get; set; }
        public string ManagerName { get; set; }
        public string Location { get; set; }
        public string Skills { get; set; }
        public string EduDegree { get; set; }
        public string EduBranch { get; set; }
        public string EduUniversity { get; set; }
        public decimal? EduGrade { get; set; }
        public string EduYear { get; set; }
        public string EmergencyContactName { get; set; }
        public decimal EmergencyContactNumber { get; set; }
        public string EmergencyContactRelationship { get; set; }
        public string EmergencyContactAddress { get; set; }
        public bool IsActive { get; set; }
        public byte[]? ProfilePhoto { get; set; }
    }
}
