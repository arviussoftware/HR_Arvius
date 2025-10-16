using HR_Arvius.DTOs.Requests.Employee;
using HR_Arvius.DTOs.Responses;
using HR_Arvius.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Data;

namespace HR_Arvius.Services.Class
{
    public class EmployeeService : IEmployeeService
    {
        private readonly string _connectionString;
        private readonly EmailService _emailService;

        public EmployeeService(EmailService emailService, IConfiguration configuration)
        {
            _emailService = emailService;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<TimeSheetRespDTO> GetTimeSheet(int userId)
        {
            var timesheets = new TimeSheetRespDTO(); 

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var query = "SELECT * FROM view_timesheets(@userId)"; 

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@userId", userId);

                        using (var adapter = new NpgsqlDataAdapter(command))
                        {
                            DataSet ds = new DataSet();
                            adapter.Fill(ds);

                            if (ds.Tables.Count > 0)
                            {
                                foreach (DataRow dr in ds.Tables[0].Rows)
                                {
                                    var row = new TimeSheetRespRowDTO
                                    {
                                        Id = (int)dr["id"],
                                        UserId = (int)dr["user_id"], 
                                        ProjectName = dr["project_name"].ToString(),
                                        Activity = dr["activity"].ToString(),
                                        ColDate = Convert.ToDateTime(dr["col_date"]),  
                                        Task = dr["task"].ToString(),
                                        StartTime = dr["start_time"] != DBNull.Value
                                                    ? (TimeSpan)dr["start_time"]  
                                                    : TimeSpan.Zero,  
                                        EndTime = dr["end_time"] != DBNull.Value
                                                  ? (TimeSpan)dr["end_time"] 
                                                  : TimeSpan.Zero  
                                    };

                                    timesheets.Rows.Add(row);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle the error (e.g., log it)
                timesheets.ErrorMessage = ex.Message;  // You can return the error message in the DTO
            }
            return timesheets;  // Return the populated DTO
        }

        public async Task<bool> AddTimeSheetAsync(int userId, string project, string activity, DateTime date, TimeSpan startTime, TimeSpan endTime, string task)
        {
            try
            {
                // Validate that start time is earlier than end time
                if (startTime >= endTime)
                {
                    throw new ArgumentException("Start time must be earlier than end time.");
                }

                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Combine Date + Start Time to form the exact timestamp for start_time and similarly for end_time
                    var startDateTime = date.Date + startTime; // Combine Date + Time for start_time
                    var endDateTime = date.Date + endTime; // Combine Date + Time for end_time

                    var query = "SELECT add_timesheet(@userId, @project, @activity, @colDate, @startTime, @endTime, @task);";
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@userId", userId);
                        command.Parameters.AddWithValue("@project", project);
                        command.Parameters.AddWithValue("@activity", activity);
                        command.Parameters.Add("@colDate", NpgsqlTypes.NpgsqlDbType.Date).Value = date.Date;
                        command.Parameters.Add("@startTime", NpgsqlTypes.NpgsqlDbType.Time).Value = startTime;
                        command.Parameters.Add("@endTime", NpgsqlTypes.NpgsqlDbType.Time).Value = endTime;
                        command.Parameters.AddWithValue("@task", task);

                        await command.ExecuteScalarAsync();
                        return true; // successful execution if no exception
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Database operation failed: " + ex.Message, ex);
            }
        }

        //Fetch Employee Data
        public async Task<EmployeeDTO> GetEmployeeData(int userId)
        {
            var empDataGrid = new EmployeeDTO();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var query = "Select * from emp_leave_applications(@userId)";
                    DataSet ds = new DataSet();
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@userId", userId);
                        using (var adapter = new NpgsqlDataAdapter(command))
                        {
                            adapter.Fill(ds);
                        }
                    }
                    if (ds.Tables.Count > 0)
                    {
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            var row = new EmployeeRowDTO
                            {
                                LeaveType = dr["leave_type"].ToString(),
                                startDate = Convert.ToDateTime(dr["startDate"]),
                                endDate = Convert.ToDateTime(dr["endDate"]),
                                Days = (decimal)dr["days"],
                                Status = dr["status"].ToString(),
                                Applied = Convert.ToDateTime(dr["applied"]),
                                Reason = dr["reason"].ToString(),
                                ManagerComments = dr["m_comm"].ToString(),
                                HrComments = dr["hr_comm"].ToString(),
                                isHalfDay = (bool)dr["halfday"],
                                sessionHalfDay = dr["sessionhalfday"].ToString(),
                                ApplicationId = Convert.ToInt32(dr["applicationId"])
                            };

                            empDataGrid.Rows.Add(row);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error fetching the data...", ex);
            }
            return empDataGrid;
        }

        //Balance for leave application
        public async Task<EmployeeApplyBalanceDTO> GetLeaveBalance(int userId)
        {
            var leaveDataGrid = new EmployeeApplyBalanceDTO();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var query = "Select * From emp_leave_balance(@userId)";
                    DataSet ds = new DataSet();
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@userId", userId);
                        using (var adapter = new NpgsqlDataAdapter(command))
                        {
                            adapter.Fill(ds);
                        }
                        if (ds.Tables.Count > 0)
                        {
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                var row = new EmployeeApplyBalanceRowDTO
                                {
                                    leaveId = Convert.ToInt32(dr["leaveId"]),
                                    leaveName = dr["leaveName"].ToString(),
                                    remainingDays = (decimal)dr["remaining_days"],
                                    usedDays = (decimal)dr["used_days"]
                                };

                                leaveDataGrid.Rows.Add(row);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error fetching data..", ex);
            }
            return leaveDataGrid;
        }

        public async Task<bool> ResetPassword(int userId, string oldPassword, string newPassword)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var getQuery = "SELECT password FROM users WHERE id = @userId";
                using var getCmd = new NpgsqlCommand(getQuery, connection);
                getCmd.Parameters.AddWithValue("@userId", userId);

                var storedHash = (string?)await getCmd.ExecuteScalarAsync();
                if (storedHash == null) return false;

                if (!BCrypt.Net.BCrypt.Verify(oldPassword, storedHash))
                    return false;

                string newHashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword, 10);

                var updateQuery = "UPDATE users SET password = @hashPassword WHERE id = @userId";
                using var updateCmd = new NpgsqlCommand(updateQuery, connection);
                updateCmd.Parameters.AddWithValue("@hashPassword", newHashedPassword);
                updateCmd.Parameters.AddWithValue("@userId", userId);

                int rowsAffected = await updateCmd.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while resetting password: {ex.Message}");
                return false;
            }
        }

        public async Task<List<HolidayDTO>> GetRestrictedHolidaysAsync()
        {
            var holidays = new List<HolidayDTO>();
            var connStr = new NpgsqlConnection(_connectionString);

            await connStr.OpenAsync();

            var cmd = new NpgsqlCommand("SELECT * FROM restricted_holiday_list()", connStr);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                holidays.Add(new HolidayDTO
                {
                    id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    HolidayDate = reader.GetDateTime(2),
                    HolidayDay = reader.GetString(3)
                });
            }

            return holidays;
        }

        //Apply Leave
        public async Task<LeaveApplyResultDTO> AddLeaveAsync(int userId, EmployeeApplyDTO leave)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"
                    SELECT 
                        employee_name, 
                        user_email, 
                        leave_type, 
                        start_date, 
                        end_date, 
                        total_days, 
                        success,
                        manager_email,
                        manager_name,
                        hr_email,
                        hr_name
                    FROM emp_leave_apply(
                        @p_user_id,
                        @p_leave_type_id,
                        @p_start_date,
                        @p_end_date,
                        @p_total_days,
                        @p_reason,
                        @p_document_url,
                        @p_is_half_day,
                        @p_half_day_session
                    );";

                await using var cmd = new NpgsqlCommand(query, connection);

                cmd.Parameters.AddWithValue("@p_user_id", NpgsqlTypes.NpgsqlDbType.Integer, userId);
                cmd.Parameters.AddWithValue("@p_leave_type_id", NpgsqlTypes.NpgsqlDbType.Integer, leave.leaveType);
                cmd.Parameters.AddWithValue("@p_start_date", NpgsqlTypes.NpgsqlDbType.Date, leave.startDate.Date);
                cmd.Parameters.AddWithValue("@p_end_date", NpgsqlTypes.NpgsqlDbType.Date, leave.endDate.Date);
                cmd.Parameters.AddWithValue("@p_total_days", NpgsqlTypes.NpgsqlDbType.Numeric, leave.totalDays);
                cmd.Parameters.AddWithValue("@p_reason", NpgsqlTypes.NpgsqlDbType.Text, leave.reason ?? string.Empty);
                cmd.Parameters.AddWithValue("@p_document_url", NpgsqlTypes.NpgsqlDbType.Varchar, leave.documentUrl ?? string.Empty);
                cmd.Parameters.AddWithValue("@p_is_half_day", NpgsqlTypes.NpgsqlDbType.Boolean, leave.isHalfDay);
                cmd.Parameters.AddWithValue("@p_half_day_session", NpgsqlTypes.NpgsqlDbType.Varchar, (object?)leave.halfDaySession ?? DBNull.Value);

                await using var reader = await cmd.ExecuteReaderAsync();

                LeaveApplyResultDTO resultDto = null;
                string managerEmail = null, managerName = null, hrEmail = null, hrName = null;

                if (await reader.ReadAsync())
                {
                    resultDto = new LeaveApplyResultDTO
                    {
                        EmployeeName = reader.GetString(reader.GetOrdinal("employee_name")),
                        EmployeeEmail = reader.GetString(reader.GetOrdinal("user_email")),
                        LeaveType = reader.GetString(reader.GetOrdinal("leave_type")),
                        StartDate = reader.GetDateTime(reader.GetOrdinal("start_date")),
                        EndDate = reader.GetDateTime(reader.GetOrdinal("end_date")),
                        TotalDays = reader.GetDecimal(reader.GetOrdinal("total_days")),
                        Success = reader.GetBoolean(reader.GetOrdinal("success"))
                    };

                    managerEmail = reader["manager_email"]?.ToString() ?? "manager@yourcompany.com";
                    managerName = reader["manager_name"]?.ToString() ?? "Manager";
                    hrEmail = reader["hr_email"]?.ToString() ?? "hr@yourcompany.com";
                    hrName = reader["hr_name"]?.ToString() ?? "HR/Admin";
                }

                await reader.CloseAsync();

                if (resultDto == null)
                    return new LeaveApplyResultDTO { Success = false };

                string baseUrl = "http://localhost:3000";
                string actionLink = $"{baseUrl}";

                string emailTemplate(string recipientName, string note, string actionButtonHtml = "")
                {
                    return $@"
                <div style='font-family:Segoe UI, Tahoma, Geneva, Verdana, sans-serif; background-color:#f5f7fa; padding:20px;'>
                    <div style='max-width:600px; margin:auto; background-color:#ffffff; border-radius:10px; box-shadow:0 4px 8px rgba(0,0,0,0.1); padding:30px;'>
                        <h2 style='color:#2c3e50; text-align:center; border-bottom:2px solid #3498db; padding-bottom:10px;'>Leave Application</h2>
                        <p style='font-size:14px; color:#34495e;'>Dear <b>{recipientName}</b>,</p>
                        <p style='font-size:14px; color:#34495e;'>{note}</p>

                        <table style='width:100%; border-collapse:collapse; margin-top:20px; font-size:14px; color:#2c3e50;'>
                            <tr>
                                <td style='padding:8px; border:1px solid #ddd; font-weight:bold;'>Employee Name:</td>
                                <td style='padding:8px; border:1px solid #ddd;'>{resultDto.EmployeeName}</td>
                            </tr>
                            <tr>
                                <td style='padding:8px; border:1px solid #ddd; font-weight:bold;'>Leave Type:</td>
                                <td style='padding:8px; border:1px solid #ddd;'>{resultDto.LeaveType}</td>
                            </tr>
                            <tr>
                                <td style='padding:8px; border:1px solid #ddd; font-weight:bold;'>From:</td>
                                <td style='padding:8px; border:1px solid #ddd;'>{resultDto.StartDate:dd MMM yyyy}</td>
                            </tr>
                            <tr>
                                <td style='padding:8px; border:1px solid #ddd; font-weight:bold;'>To:</td>
                                <td style='padding:8px; border:1px solid #ddd;'>{resultDto.EndDate:dd MMM yyyy}</td>
                            </tr>
                            <tr>
                                <td style='padding:8px; border:1px solid #ddd; font-weight:bold;'>Total Days:</td>
                                <td style='padding:8px; border:1px solid #ddd; color:#e74c3c; font-weight:bold;'>
                                    {(resultDto.TotalDays == 0.5m ? "Half Day" : resultDto.TotalDays == 1m ? "1 day" : Math.Round(resultDto.TotalDays) + " days")}
                                </td>
                            </tr>
                            <tr>
                                <td style='padding:8px; border:1px solid #ddd; font-weight:bold;'>Reason:</td>
                                <td style='padding:8px; border:1px solid #ddd;'>{leave.reason}</td>
                            </tr>
                        </table>

                        {actionButtonHtml}

                        <p style='margin-top:20px; font-size:14px; color:#34495e;'>Thank you!</p>
                        <p style='font-size:14px; color:#34495e;'>Regards,<br/><b>Support Team</b><br/><b>ARVIUS SOFTWARE Pvt Ltd</b></p>
                        <div style='text-align:center; margin-top:20px; font-size:12px; color:#95a5a6;'>
                            <p>This is an automated email. Please do not reply directly.</p>
                        </div>
                    </div>
                </div>";
                }

                string actionButtonHtml = $@"
                <div style='margin-top:25px; text-align:center;'>
                    <a href='{actionLink}' style='background-color:#3498db; color:#fff; padding:10px 20px; border-radius:5px; text-decoration:none;'>Take Action!</a>
                </div>";

                _emailService.SendEmailFireAndForget(
                    managerEmail,
                    $"New Leave Application from {resultDto.EmployeeName}",
                    emailTemplate(managerName,
                        $"{resultDto.EmployeeName} has applied for leave.<br/>Please review and take necessary action.",
                        actionButtonHtml),
                    cc: new[] { hrEmail }
                );

                _emailService.SendEmailFireAndForget(
                    resultDto.EmployeeEmail,
                    $"Your Leave Application has been submitted successfully!",
                    emailTemplate(resultDto.EmployeeName,
                        $"Your leave request from <b>{resultDto.StartDate:dd MMM yyyy}</b> to <b>{resultDto.EndDate:dd MMM yyyy}</b> has been successfully submitted for approval.")
                );

                return resultDto;
            }
            catch (PostgresException pgEx)
            {
                throw new ApplicationException(pgEx.MessageText, pgEx);
            }
        }

        //Fetch Leave Count
        public async Task<EmployeeBalanceDTO> GetLeaveCount(int userId)
        {
            var leaveData = new EmployeeBalanceDTO();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var query = "Select * From emp_leave_dash(@userId)";
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@userId", userId);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                leaveData.SickAllocated = reader.GetDecimal(reader.GetOrdinal("sick_allocated"));
                                leaveData.SickUsed = reader.GetDecimal(reader.GetOrdinal("sick_used"));
                                leaveData.SickRemaining = reader.GetDecimal(reader.GetOrdinal("sick_remaining"));
                                leaveData.CasualAllocated = reader.GetDecimal(reader.GetOrdinal("casual_allocated"));
                                leaveData.CasualUsed = reader.GetDecimal(reader.GetOrdinal("casual_used"));
                                leaveData.CasualRemaining = reader.GetDecimal(reader.GetOrdinal("casual_remaining"));
                                leaveData.EarnedAllocated = reader.GetDecimal(reader.GetOrdinal("earned_allocated"));
                                leaveData.EarnedUsed = reader.GetDecimal(reader.GetOrdinal("earned_used"));
                                leaveData.EarnedRemaining = reader.GetDecimal(reader.GetOrdinal("earned_remaining"));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error fetching data..", ex);
            }
            return leaveData;
        }

        public async Task<EmployeeLeaveDTO> GetLeaveTypeAsync(int userId)
        {
            var data = new EmployeeLeaveDTO();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var query = @"
                        SELECT lt.id, lt.name, lt.code, lt.requires_document
                        FROM leave_types lt
                        JOIN users u ON u.id = @UserId
                        WHERE lt.is_active = true
                            AND (
                                (LOWER(u.gender) = 'male'   AND lt.id IN (1,2,3,5,6))
                                OR (LOWER(u.gender) = 'female' AND lt.id IN (1,2,3,4,6))
                                OR (LOWER(u.gender) = 'other'  AND lt.id IN (1,2,3,6))
                            );";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", userId);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var row = new EmployeeLeaveRowDTO
                                {
                                    id = reader.GetInt32(0),
                                    name = reader.GetString(1),
                                    code = reader.GetString(2),
                                    requiresDocument = reader.GetBoolean(3)
                                };

                                data.Rows.Add(row);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return data;
        }

        public async Task<EmployeeCancelLeaveDTO> CancelLeave(int leaveId)
        {
            var response = new EmployeeCancelLeaveDTO();

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

        public async Task<bool> UploadProfilePhotoBytes(int userId, byte[] imageBytes)
        {
            if (imageBytes == null || imageBytes.Length == 0)
                return false;

            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = "UPDATE users SET profile_photo = @photo WHERE id = @userId";
                await using var cmd = new NpgsqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@photo", NpgsqlTypes.NpgsqlDbType.Bytea, imageBytes);
                cmd.Parameters.AddWithValue("@userId", userId);

                var rowsAffected = await cmd.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error uploading profile photo", ex);
            }
        }

        public async Task<byte[]> GetProfilePhoto(int userId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = "SELECT profile_photo FROM users WHERE id = @userId";
                using var cmd = new NpgsqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@userId", userId);

                var result = await cmd.ExecuteScalarAsync();
                if (result != DBNull.Value && result != null)
                    return (byte[])result;

                return null;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error fetching profile photo", ex);
            }
        }
    } 
}
