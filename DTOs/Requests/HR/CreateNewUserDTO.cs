using System;
using System.ComponentModel.DataAnnotations;

namespace HR_Arvius.DTOs.Requests.HR
{
    public class CreateNewUserDTO
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        public string? LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string? Password { get; set; }

        public long ContactNumber { get; set; }

        [Required]
        public string Gender { get; set; }
        public string BloodGroup { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        public string? PermanentAddress { get; set; }
        public string? PresentAddress { get; set; }

        [Required]
        public string EmployeeId { get; set; }

        [Required]
        public DateTime HireDate { get; set; }

        public string? Department { get; set; }

        [Required]
        public string Role { get; set; }

        // Profile photo base64
        public string? ProfilePhotoBase64 { get; set; }

        // Store filename of uploaded profile photo
        public string? ProfilePhotoFileName { get; set; }

        public int? ManagerId { get; set; }
        public string? Location { get; set; }
        public string? Skills { get; set; }

        public string EduDegree { get; set; }

        public string? EduBranch { get; set; }

        public string EduUniversity { get; set; }

        public string? EduYear { get; set; } // Nullable now

        public decimal? EduGrade { get; set; }

        public string? EmergencyName { get; set; }

        [Required]
        public long EmergencyNumber { get; set; }

        public string? EmergencyRelationship { get; set; }
        public string? EmergencyAddress { get; set; }
    }
}
