using HR_Arvius.DTOs.Requests.Employee;
using HR_Arvius.DTOs.Requests.HR;
using HR_Arvius.DTOs.Responses;
using HR_Arvius.Services.Interface;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
namespace HR_Arvius.Services.Class
{
    public class HrService : IHrService
    {
        private readonly EmailService _emailService;
        private readonly string _connectionString;

        public HrService(EmailService emailService, IConfiguration configuration)
        {
            _emailService = emailService;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<HrDTO> GetHrData() 
        {
            var hrData = new HrDTO();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var query = "Select * From hr_dash()";

                    using (var command = new NpgsqlCommand(query, connection))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            hrData.PendingApplications = reader.GetInt32(reader.GetOrdinal("pending_applications"));
                            hrData.Approved = reader.GetInt32(reader.GetOrdinal("approved"));
                            hrData.Rejected = reader.GetInt32(reader.GetOrdinal("rejected"));
                            hrData.TotalLeaveTypes = reader.GetInt32(reader.GetOrdinal("total_leave_types"));
                            hrData.TotalEmployees = reader.GetInt32(reader.GetOrdinal("total_employees"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching Data.", ex);
            }
            return hrData;
        }

        public async Task<CompanyHolidayDTO> GetHolidays()
        {
            var data = new CompanyHolidayDTO();
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var query = "SELECT * FROM public.get_holidays();";
                    DataSet ds = new DataSet();
                    using (var command = new NpgsqlCommand(query, connection))
                    using (var adapter = new NpgsqlDataAdapter(command))
                    {
                        adapter.Fill(ds);
                    }
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        var row = new CompanyHolidayRowDTO
                        {
                            HolidayName = dr["name"].ToString(),
                            HolidayDate = Convert.ToDateTime(dr["holiday_date"]),
                            HolidayDay = dr["day"].ToString(),
                            HolidayType = (bool)dr["is_mandatory"]                            
                        };

                        data.Rows.Add(row);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching data.", ex);
            }
            return data;
        }

        public List<PolicyResponseDTO> GetPolicies()
        {
            var policies = new List<PolicyResponseDTO>();

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT id, title, description, type, uploaded_by, uploaded_at, tags, content, pdf_data,filename FROM policies", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        policies.Add(new PolicyResponseDTO
                        {
                            id = reader.GetInt64(0),
                            title = reader.GetString(1),
                            description = reader.IsDBNull(2) ? null : reader.GetString(2),
                            type = reader.IsDBNull(3) ? null : reader.GetString(3),
                            uploadedBy = reader.IsDBNull(4) ? null : reader.GetString(4),
                            uploadedAt = reader.IsDBNull(5) ? DateTime.MinValue : reader.GetDateTime(5),
                            tags = reader.IsDBNull(6) ? new List<string>() : reader.GetFieldValue<string[]>(6).ToList(),
                            content = reader.IsDBNull(7) ? null : reader.GetString(7),
                            pdfData = reader.IsDBNull(8) ? null : Convert.ToBase64String(reader.GetFieldValue<byte[]>(8)),
                            fileName = reader.IsDBNull(9)? null: reader.GetString(9),

                        });
                    }
                }
            }

            return policies;
        }


        public async Task AddPolicy(PolicyRequestDTO request)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        // Insert policies
                        foreach (var policy in request.policies)
                        {
                            var cmd = new NpgsqlCommand(@"
                            INSERT INTO policies (title, description, type, uploaded_by, uploaded_at, tags, content, pdf_data,filename)
                            VALUES (@title, @description, @type, @uploadedBy, @uploadedAt, @tags, @content, @pdfData,@fileName)
                            RETURNING id", conn);

                            cmd.Parameters.AddWithValue("title", policy.title ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("description", policy.description ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("type", policy.type ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("uploadedBy", policy.uploadedBy ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("uploadedAt", policy.uploadedAt);
                            cmd.Parameters.AddWithValue("tags", policy.tags?.ToArray() ?? new string[0]);
                            cmd.Parameters.AddWithValue("content", policy.content ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("pdfData", string.IsNullOrEmpty(policy.pdfData)? (object)DBNull.Value : Convert.FromBase64String(policy.pdfData));
                            cmd.Parameters.AddWithValue("filename", policy.fileName ?? (object)DBNull.Value);

                            var newId = (long)await cmd.ExecuteScalarAsync();
                        }

                        var meta = request.metadata;

                        var metaCmd = new NpgsqlCommand(@"
                            INSERT INTO metadata (last_updated, version, total_policies)
                            VALUES (@lastUpdated, @version, @totalPolicies)", conn);

                        metaCmd.Parameters.AddWithValue("lastUpdated", meta.lastUpdated);
                        metaCmd.Parameters.AddWithValue("version", meta.version ?? (object)DBNull.Value);
                        metaCmd.Parameters.AddWithValue("totalPolicies", meta.totalPolicies);

                        await metaCmd.ExecuteNonQueryAsync();

                        await tran.CommitAsync();
                    }
                    catch
                    {
                        await tran.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        public MetaDataDTO GetMetaData()
        {
            var meta = new MetaDataDTO();
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT last_updated, version, total_policies FROM metadata ORDER BY last_updated DESC LIMIT 1", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        meta.lastUpdated = reader.GetDateTime(0);
                        meta.version = reader.IsDBNull(1) ? null : reader.GetString(1);
                        meta.totalPolicies = reader.GetInt32(2);
                    }
                }
            }
            return meta;
        }


        public async Task<bool> UserStatusUpdate(UserStatusMarkDTO dto)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new NpgsqlCommand("SELECT user_status_action(@p_user_id, @action);", connection);
                command.Parameters.AddWithValue("p_user_id", dto.UserId);   
                command.Parameters.AddWithValue("action", dto.Action);

                await command.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error executing user_mark_inactive function", ex);
            }
        }
        public async Task<HrGridDTO> GetHrOverall()
        {
            var hrDataGrid = new HrGridDTO();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var query = "SELECT * FROM hr_all_applications();";
                    DataSet ds = new DataSet();
                    using (var command = new NpgsqlCommand(query, connection))
                    using (var adapter = new NpgsqlDataAdapter(command))
                    {
                        adapter.Fill(ds);
                    }
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        var row = new HrGridRowDTO
                        {
                            Employee = dr["employee"].ToString(),
                            EmployeeId = dr["empId"].ToString(),
                            Department = dr["department"].ToString(),
                            LeaveType = dr["leave_type"].ToString(),
                            StartDate = Convert.ToDateTime(dr["start_date"]),
                            EndDate = Convert.ToDateTime(dr["end_date"]),
                            Days = (decimal)dr["days"],
                            Reason = dr["reason"].ToString(),
                            ManagerComments = dr["m_comm"].ToString(),
                            HrComments = dr["hr_comm"].ToString(),
                            Status = dr["status"].ToString(),
                            AppliedOn = Convert.ToDateTime(dr["applied_date"]),
                            halfDay = (bool)dr["halfDay"],
                            sessionHalfDay = dr["sessionHalfDay"].ToString()
                        };

                        hrDataGrid.Rows.Add(row);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching data.", ex);
            }
            return hrDataGrid;
        }

        public async Task<HrEmployeeDTO> GetEmployeeList()
        {
            var empList = new HrEmployeeDTO();
            try
            {
                using(var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var query = "SELECT * FROM hr_emp_list()";
                    DataSet ds = new DataSet();
                    using(var command = new NpgsqlCommand(query, connection))
                    using(var adapter = new NpgsqlDataAdapter(command))
                    {
                        adapter.Fill(ds);
                    }
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        var row = new HrEmployeeRowDTO
                        {
                            id = dr["id"].ToString(),
                            Name = dr["name"].ToString(),
                            Role = dr["role"].ToString(),
                            Department = dr["department"].ToString(),
                            EmpId = dr["employee_id"].ToString(),
                            ManagerId = (Int32)(dr["manager_id"]),
                            Location = dr["location"].ToString()
                        };
                        empList.Rows.Add(row);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching data.", ex);
            }
            return empList;
        }

        public async Task<HrEmployeeDTO> GetInactiveEmployeeList()
        {
            var empList = new HrEmployeeDTO();
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var query = "SELECT * FROM hr_emp_list_inactive()";
                    DataSet ds = new DataSet();
                    using (var command = new NpgsqlCommand(query, connection))
                    using (var adapter = new NpgsqlDataAdapter(command))
                    {
                        adapter.Fill(ds);
                    }
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        var row = new HrEmployeeRowDTO
                        {
                            id = dr["id"].ToString(),
                            Name = dr["name"].ToString(),
                            Role = dr["role"].ToString(),
                            Department = dr["department"].ToString(),
                            EmpId = dr["employee_id"].ToString(),
                            ManagerId = (Int32)(dr["manager_id"]),
                            Location = dr["location"].ToString()
                        };
                        empList.Rows.Add(row);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching data.", ex);
            }
            return empList;
        }
        public async Task<HrPendingDTO> GetHrPendingGrid()
        {
            var hrPending = new HrPendingDTO();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var query = "SELECT * FROM hr_pending_applications()";
                    DataSet ds = new DataSet();
                    using (var command = new NpgsqlCommand(query, connection))
                    using (var adapter = new NpgsqlDataAdapter(command))
                    {
                        adapter.Fill(ds);
                    }
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        var row = new HrPendingRowDTO
                        {
                            Employee = dr["employee"].ToString(),
                            EmployeeId = dr["empId"].ToString(),
                            Department = dr["department"].ToString(),
                            LeaveType = dr["leave_type"].ToString(),
                            startDate = Convert.ToDateTime(dr["start_date"]),
                            endDate = Convert.ToDateTime(dr["end_date"]),
                            Days = (decimal)dr["days"],
                            Status = dr["status"].ToString(),
                            Applied = Convert.ToDateTime(dr["applied_date"]),
                            ApplicationId = (int)dr["application_id"],
                            Reason = dr["reason"].ToString(),
                            halfDay = (bool)dr["halfDay"],
                            sessionHalfDay = dr["sessionHalfDay"].ToString()
                        };
                        hrPending.Rows.Add(row);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching data.", ex);
            }
            return hrPending;
        }


        public async Task<bool> ApproveOrReject(int id, string action, string comments)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                // Execute hr_action function
                using (var command = new NpgsqlCommand("SELECT hr_action(@id, @action, @comments);", connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@action", action);
                    command.Parameters.AddWithValue("@comments", comments ?? string.Empty);

                    var result = await command.ExecuteScalarAsync();
                    bool success = result != null && (bool)result;

                    if (!success)
                        return false;
                }

                // 2️⃣ Fetch employee details for email
                string detailsQuery = @"
                        SELECT 
                            u.email AS employee_email,
                            u.first_name || ' ' || u.last_name AS employee_name,
                            lt.name AS leave_type,
                            la.start_date,
                            la.end_date,
                            la.total_days
                        FROM leave_applications la
                        JOIN users u ON la.user_id = u.id
                        JOIN leave_types lt ON la.leave_type_id = lt.id
                        WHERE la.id = @appId;
                    ";

                EmployeeLeaveEmailDTO emailData = null;
                using (var cmd = new NpgsqlCommand(detailsQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@appId", id);

                    using var reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        emailData = new EmployeeLeaveEmailDTO
                        {
                            EmployeeEmail = reader["employee_email"].ToString(),
                            EmployeeName = reader["employee_name"].ToString(),
                            LeaveType = reader["leave_type"].ToString(),
                            StartDate = Convert.ToDateTime(reader["start_date"]),
                            EndDate = Convert.ToDateTime(reader["end_date"]),
                            TotalDays = Convert.ToDecimal(reader["total_days"])
                        };
                    }
                }

                if (emailData == null)
                    return true; // nothing to send if details missing

                // 3️⃣ Prepare email content
                string toEmail = emailData.EmployeeEmail ?? "hr@yourcompany.com";
                string subject = $"Leave Application {action.ToUpper()}";

                string statusColor = action.ToLower() == "approved" ? "#4CAF50" : "#F44336";
                string htmlBody = $@"
            <div style='font-family:Segoe UI, sans-serif;'>
                <h3 style='color:{statusColor};'>Your Leave has been {action.ToUpper()}!</h3>
                <p>Dear {emailData.EmployeeName},</p>
                <p>Your leave application has been <b>{action.ToUpper()}</b> by HR.</p>
                <table style='border-collapse: collapse; margin-top:10px;'>
                    <tr><td><b>Leave Type:</b></td><td>{emailData.LeaveType}</td></tr>
                    <tr><td><b>From:</b></td><td>{emailData.StartDate:dd MMM yyyy}</td></tr>
                    <tr><td><b>To:</b></td><td>{emailData.EndDate:dd MMM yyyy}</td></tr>
                    <tr><td><b>Total Days:</b></td><td>{emailData.TotalDays}</td></tr>
                    <tr><td><b>HR Comments:</b></td><td>{comments}</td></tr>
                </table>
                <br/>
                <p>Regards,<br/>HR Team</p>
            </div>";

                _emailService.SendEmailFireAndForget(toEmail, subject, htmlBody);

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating approval status.", ex);
            }
        }

        //Is being used for cancellation
        public async Task<CancelLeaveDTO> CancelLeave(int leaveId)
        {
            var response = new CancelLeaveDTO();

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = "SELECT emp_leave_delete(@p_leave_id)";
                using var command = new NpgsqlCommand(query, connection);

                command.Parameters.AddWithValue("@p_leave_id", leaveId);

                var result = await command.ExecuteScalarAsync();

                if (result != null && bool.TryParse(result.ToString(), out bool isSuccess) && isSuccess)
                {
                    response.Success = true;
                    response.Message = "Leave cancelled successfully.";
                }
                else
                {
                    response.Success = false;
                    response.Message = "Failed to cancel the leave. It may have already been approved or not exist.";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error occurred while cancelling the leave: {ex.Message}";
            }

            return response;
        }

        public async Task<DeletePolicyDTO> DeletePolicy(int policyId)
        {
            var response = new DeletePolicyDTO();

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = "DELETE FROM policies WHERE id = @id";
                using var command = new NpgsqlCommand(query, connection);

                command.Parameters.AddWithValue("@id", policyId);

                var rowsAffected = await command.ExecuteNonQueryAsync();

                if (rowsAffected > 0)
                {
                    response.Success = true;
                    response.Message = "Policy deleted successfully.";
                }
                else
                {
                    response.Success = false;
                    response.Message = "Policy not found or could not be deleted.";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error occurred while deleting the policy: {ex.Message}";
            }

            return response;
        }

        public async Task<HrReportsDTO> GetHrReports(string? department, DateTime? startDate, DateTime? endDate)
        {
            var hrReports = new HrReportsDTO();
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using var command = new NpgsqlCommand("SELECT * FROM hr_generate_reports(@dep, @start, @end);", connection);
                    command.Parameters.AddWithValue("dep", (object?)department ?? DBNull.Value);
                    command.Parameters.AddWithValue("start", (object?)startDate ?? DBNull.Value);
                    command.Parameters.AddWithValue("end", (object?)endDate ?? DBNull.Value);

                    DataSet ds = new DataSet();

                    using (var adapter = new NpgsqlDataAdapter(command))
                    {
                        adapter.Fill(ds);
                    }
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        var row = new HrReportsRowDTO
                        {
                            Employee = dr["employee_name"].ToString(),
                            EmployeeId = dr["employee_id"].ToString(),
                            Department = dr["department"].ToString(),
                            LeaveType = dr["leave_type"].ToString(),
                            Applications = (int)dr["total_applications"],
                            TotalDays = (decimal)dr["total_days_taken"],
                            AverageDays = (decimal)dr["avg_days_per_application"],
                            StartLeaveDate = (DateTime)dr["min_leave_date"],
                            EndLeaveDate = (DateTime)dr["max_leave_date"]
                        };
                        hrReports.Rows.Add(row);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching data.", ex);
            }
            return hrReports;
        }

        //All manager's List
        public async Task<ManagerListDTO> GetMangerList()
        {
            var list = new ManagerListDTO();

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = "SELECT id, first_name || ' ' || last_name AS name FROM users WHERE role != 'employee' AND is_active != false";
                using var command = new NpgsqlCommand(query, connection);

                DataSet ds = new DataSet();

                using (var adapter = new NpgsqlDataAdapter(command))
                {
                    adapter.Fill(ds);
                }
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    var row = new ManagerListRowDTO
                    {
                        name = dr["name"].ToString(),
                        id = (decimal)dr["id"]
                    };
                    list.Rows.Add(row);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching data.", ex);
            }
            return list;
        }

        //User Preview
        public async Task<UserPreviewDTO?> GetPreview(int userId)
        {
            UserPreviewDTO? preview = null;

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"
                    SELECT *, u.profile_photo
                    FROM user_preview(@p_user_id) up
                    LEFT JOIN users u ON u.id = @p_user_id
                ";

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("p_user_id", userId);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    preview = new UserPreviewDTO
                    {
                        Id = reader.GetDecimal(reader.GetOrdinal("id")),
                        FirstName = reader["first_name"].ToString(),
                        LastName = reader["last_name"].ToString(),
                        Email = reader["email"].ToString(),
                        ContactNumber = reader.GetDecimal(reader.GetOrdinal("contact_number")),
                        Gender = reader["gender"].ToString(),
                        BloodGroup = reader["blood_group"].ToString(),
                        DateOfBirth = reader.GetDateTime(reader.GetOrdinal("date_of_birth")),
                        AddressPermanent = reader["address_permanent"].ToString(),
                        AddressPresent = reader["address_present"].ToString(),
                        EmployeeId = reader["employee_id"].ToString(),
                        HireDate = reader.GetDateTime(reader.GetOrdinal("hire_date")),
                        Department = reader["department"].ToString(),
                        Role = reader["role"].ToString(),
                        ManagerId = reader.IsDBNull(reader.GetOrdinal("manager_id")) ? null : reader.GetInt32(reader.GetOrdinal("manager_id")),
                        ManagerName = reader["manager_name"].ToString(),
                        Location = reader["location"].ToString(),
                        Skills = reader["skills"].ToString(),
                        EduDegree = reader["edu_degree"].ToString(),
                        EduBranch = reader["edu_branch"].ToString(),
                        EduUniversity = reader["edu_university"].ToString(),
                        EduGrade = reader.IsDBNull(reader.GetOrdinal("edu_grade")) ? null : reader.GetDecimal(reader.GetOrdinal("edu_grade")),
                        EduYear = reader["edu_year"].ToString(),
                        EmergencyContactName = reader["emergency_contact_name"].ToString(),
                        EmergencyContactNumber = reader.GetDecimal(reader.GetOrdinal("emergency_contact_number")),
                        EmergencyContactRelationship = reader["emergency_contact_relationship"].ToString(),
                        EmergencyContactAddress = reader["emergency_contact_address"].ToString(),
                        IsActive = reader.GetBoolean(reader.GetOrdinal("is_active")),

                        // Fetch the profile photo directly
                        ProfilePhoto = reader.IsDBNull(reader.GetOrdinal("profile_photo"))
                            ? null
                            : (byte[])reader["profile_photo"]
                    };
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching preview data.", ex);
            }

            return preview;
        }

        public async Task<bool> CreateHoliday(CreateHolidayDTO dto)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            try
            {
                using var command = new NpgsqlCommand("SELECT add_holiday(@p_name, @p_holiday_date, @p_is_mandatory, @p_is_optional);", connection);

                bool isMandatory = dto.holidayType.ToLower() == "company";
                bool isOptional = dto.holidayType.ToLower() == "restricted";

                command.Parameters.AddWithValue("p_name", dto.holidayName);
                command.Parameters.AddWithValue("p_holiday_date", NpgsqlDbType.Date, dto.holidayDate.Date);
                command.Parameters.AddWithValue("p_is_mandatory", isMandatory);
                command.Parameters.AddWithValue("p_is_optional", isOptional);

                await command.ExecuteNonQueryAsync();
                return true;
            }
            catch (PostgresException ex) when (ex.MessageText.Contains("Holiday already exists"))
            {
                throw new InvalidOperationException("Holiday already exists", ex);
            }
        }

        public async Task<int> CreateUser(CreateNewUserDTO dto)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            string plainPassword = string.IsNullOrEmpty(dto.Password) ? "password123" : dto.Password;
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(plainPassword, 10);

            using var cmd = new NpgsqlCommand(@"
            SELECT public.user_create(
                @FirstName,
                @LastName,
                @Email,
                @Password,
                @ContactNumber,
                @Gender,
                @BloodGroup,
                @DateOfBirth,
                @PermanentAddress,
                @PresentAddress,
                @EmployeeId,
                @HireDate,
                @Department,
                @Role,
                @ManagerId,
                @Location,
                @Skills,
                @EduDegree,
                @EduBranch,
                @EduUniversity,
                @EduGrade,
                @EduYear,
                @EmergencyName,
                @EmergencyNumber,
                @EmergencyRelationship,
                @EmergencyAddress,
                @ProfilePhoto,
                @File_Name_Profile_Photo
            )", conn);

            cmd.Parameters.AddWithValue("FirstName", dto.FirstName);
            cmd.Parameters.AddWithValue("LastName", dto.LastName ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("Email", dto.Email);
            cmd.Parameters.AddWithValue("Password", hashedPassword);
            cmd.Parameters.AddWithValue("BloodGroup", dto.BloodGroup);
            cmd.Parameters.AddWithValue("ContactNumber", NpgsqlTypes.NpgsqlDbType.Numeric, dto.ContactNumber);
            cmd.Parameters.AddWithValue("Gender", dto.Gender);
            cmd.Parameters.AddWithValue("DateOfBirth", NpgsqlTypes.NpgsqlDbType.Date, dto.DateOfBirth);
            cmd.Parameters.AddWithValue("PermanentAddress", dto.PermanentAddress ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("PresentAddress", dto.PresentAddress ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("EmployeeId", dto.EmployeeId);
            cmd.Parameters.AddWithValue("HireDate", NpgsqlTypes.NpgsqlDbType.Date, dto.HireDate);
            cmd.Parameters.AddWithValue("Department", dto.Department ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("Role", dto.Role);
            cmd.Parameters.AddWithValue("ManagerId", dto.ManagerId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("Location", dto.Location ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("Skills", dto.Skills ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("EduDegree", dto.EduDegree);
            cmd.Parameters.AddWithValue("EduBranch", dto.EduBranch ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("EduUniversity", dto.EduUniversity);
            cmd.Parameters.AddWithValue("EduGrade", dto.EduGrade ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("EduYear", dto.EduYear ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("EmergencyName", dto.EmergencyName ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("EmergencyNumber", NpgsqlTypes.NpgsqlDbType.Numeric, dto.EmergencyNumber);
            cmd.Parameters.AddWithValue("EmergencyRelationship", dto.EmergencyRelationship ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("EmergencyAddress", dto.EmergencyAddress ?? (object)DBNull.Value);

            byte[]? profilePhotoBytes = null;
            if (!string.IsNullOrEmpty(dto.ProfilePhotoBase64))
            {
                // Strip prefix if any
                var base64Data = dto.ProfilePhotoBase64;
                if (base64Data.Contains(","))
                    base64Data = base64Data.Substring(base64Data.IndexOf(',') + 1);

                profilePhotoBytes = Convert.FromBase64String(base64Data);
            }

            cmd.Parameters.AddWithValue("ProfilePhoto",
                profilePhotoBytes != null && profilePhotoBytes.Length > 0
                    ? (object)profilePhotoBytes
                    : DBNull.Value);


            // Profile photo filename
            cmd.Parameters.AddWithValue("File_Name_Profile_Photo",
                !string.IsNullOrEmpty(dto.ProfilePhotoFileName)
                    ? (object)dto.ProfilePhotoFileName
                    : DBNull.Value);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<bool> UpdateUser(UpdateUserDTO dto)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(@"
            SELECT public.update_user_details(
                @p_id, @p_first_name, @p_last_name, @p_email, @p_role, @p_department, @p_location,
                @p_manager_id, @p_hire_date::date, @p_is_active, @p_skills, @p_edu_degree, @p_edu_branch,
                @p_edu_university, @p_edu_year, @p_edu_grade, @p_contact_number, @p_address_permanent,
                @p_address_present, @p_emergency_contact_name, @p_emergency_contact_number,
                @p_emergency_contact_relationship, @p_emergency_contact_address
            )", conn);

            cmd.Parameters.AddWithValue("p_id", dto.UserId);
            cmd.Parameters.AddWithValue("p_first_name", (object?)dto.FirstName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p_last_name", (object?)dto.LastName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p_email", (object?)dto.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p_role", (object?)dto.Role ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p_department", (object?)dto.Department ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p_location", (object?)dto.Location ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p_manager_id", (object?)dto.ManagerId ?? DBNull.Value);

            // key fix: PostgreSQL expects DATE type
            cmd.Parameters.AddWithValue("p_hire_date", (object?)dto.HireDate?.Date ?? DBNull.Value);

            cmd.Parameters.AddWithValue("p_is_active", (object?)dto.IsActive ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p_skills", (object?)dto.Skills ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p_edu_degree", (object?)dto.EduDegree ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p_edu_branch", (object?)dto.EduBranch ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p_edu_university", (object?)dto.EduUniversity ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p_edu_year", (object?)dto.EduYear ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p_edu_grade", (object?)dto.EduGrade ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p_contact_number", (object?)dto.ContactNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p_address_permanent", (object?)dto.AddressPermanent ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p_address_present", (object?)dto.AddressPresent ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p_emergency_contact_name", (object?)dto.EmergencyContactName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p_emergency_contact_number", (object?)dto.EmergencyContactNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p_emergency_contact_relationship", (object?)dto.EmergencyContactRelationship ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p_emergency_contact_address", (object?)dto.EmergencyContactAddress ?? DBNull.Value);

            var result = await cmd.ExecuteScalarAsync();
            return result != null && (bool)result;
        }
    }
}