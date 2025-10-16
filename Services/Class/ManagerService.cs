using HR_Arvius.DTOs.Requests.HR;
using HR_Arvius.DTOs.Requests.Manager;
using HR_Arvius.DTOs.Responses;
using HR_Arvius.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Data;

namespace HR_Arvius.Services.Class
{
    public class ManagerService : IManagerService
    {
        private readonly string _connectionString;
        private readonly EmailService _emailService;

        public ManagerService(EmailService emailService, IConfiguration configuration)
        {
            _emailService = emailService;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<ManagerDTO> GetManagerData(int userId)
        {
            var managerData = new ManagerDTO();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var query = "SELECT * FROM manager_all_applications(@userId);";
                    DataSet ds = new DataSet();
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("userId", userId);
                        using (var adapter = new NpgsqlDataAdapter(command))
                        {
                            adapter.Fill(ds);
                        }
                    }
                    if (ds.Tables.Count > 0) 
                    { 
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            var row = new ManagerRowDTO
                            {
                                Employee = dr["employee_name"].ToString(),
                                EmployeeId   = dr["emp_id"].ToString(),
                                Department = dr["department"].ToString(),
                                LeaveType = dr["leave_type"].ToString(),
                                startDate = Convert.ToDateTime(dr["start_date"]),
                                endDate = Convert.ToDateTime(dr["end_date"]),
                                Days = (decimal)dr["days"],
                                Status = dr["status"].ToString(),
                                Reason = dr["reason"].ToString(),
                                ManagerComments = dr["m_comm"].ToString(),
                                HrComments = dr["hr_comm"].ToString(),
                                HalfDay = (bool)dr["halfday"],
                                SessionHalfDay = dr["sessionhalfday"].ToString(),
                                AppliedOn = Convert.ToDateTime(dr["applied"]),
                            };

                            managerData.Rows.Add(row);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                throw new ApplicationException("Error fetching data..", ex);
            }
            return managerData;
        }

        public async Task<HrEmployeeDTO> GetEmployeeListManager(int userId)
        {
            var empList = new HrEmployeeDTO();
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var query = "SELECT * FROM manager_team_list(@userId)";
                    DataSet ds = new DataSet();
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("userId", userId);

                        using (var adapter = new NpgsqlDataAdapter(command))
                        {
                            adapter.Fill(ds);
                        }
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            var row = new HrEmployeeRowDTO
                            {
                                Name = dr["name"].ToString(),
                                Role = dr["role"].ToString(),
                                Department = dr["department"].ToString(),
                                EmpId = dr["employee_id"].ToString(),
                                Location = dr["location"].ToString(),
                                Email = dr["email"].ToString()
                            };
                            empList.Rows.Add(row);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching data.", ex);
            }
            return empList;
        }
        public async Task<ManagerPendingDTO> GetPending(int userId)
        {
            var data = new ManagerPendingDTO();

            try
            {
                using(var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var query = "Select * from manager_pending_applications(@userId)";
                    DataSet ds = new DataSet();
                    using(var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("userId", userId);
                        using(var adapter = new NpgsqlDataAdapter(command))
                        {
                            adapter.Fill(ds);
                        }
                    }
                    if(ds.Tables.Count > 0)
                    {
                        foreach(DataRow dr in ds.Tables[0].Rows)
                        {
                            var row = new ManagerPendingRowDTO
                            {
                                employee = dr["employee_name"].ToString(),
                                employeeId = Convert.ToInt64(dr["empId"]),
                                department = dr["department"].ToString(),
                                leaveType = dr["leave_type"].ToString(),
                                startDate = Convert.ToDateTime(dr["startDate"]),
                                endDate = Convert.ToDateTime(dr["endDate"]),
                                days = (Decimal)dr["days"],
                                managerStatus = dr["status"].ToString(),
                                appliedOn = Convert.ToDateTime(dr["applied"]),
                                applicationId = Convert.ToInt32(dr["applicationId"]),
                                Reason = dr["reason"].ToString(),
                                halfDay = (bool)dr["halfDay"],
                                sessionHalfDay = dr["sessionHalfDay"].ToString()
                            };
                            data.Rows.Add(row);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                throw new ApplicationException("Error fetching data...", ex);
            }
            return data;
        }

        public async Task<bool?> ActionOnApplication(int appId, ManagerLeaveActionDTO dto)
        {
            if (dto.Action != "approved" && dto.Action != "rejected")
                throw new ArgumentException("Invalid action. Must be 'approved' or 'rejected'.");

            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                // 1️⃣ Call manager_action function
                await using (var cmd = new NpgsqlCommand("SELECT manager_action(@id, @action, @comments)", conn))
                {
                    cmd.Parameters.AddWithValue("id", appId);
                    cmd.Parameters.AddWithValue("action", dto.Action);
                    cmd.Parameters.AddWithValue("comments", dto.Comments ?? (object)DBNull.Value);

                    var result = await cmd.ExecuteScalarAsync();
                    bool success = result != null && Convert.ToBoolean(result);

                    if (!success)
                        return false;
                }

                // 2️⃣ Fetch employee and manager details for email notification
                string query = @"
                    SELECT 
                        u.email AS employee_email,
                        u.first_name || ' ' || u.last_name AS employee_name,
                        lt.name AS leave_type,
                        la.start_date,
                        la.end_date,
                        la.total_days,
                        m.email AS manager_email,
                        m.first_name || ' ' || m.last_name AS manager_name
                    FROM leave_applications la
                    JOIN users u ON la.user_id = u.id
                    JOIN leave_types lt ON la.leave_type_id = lt.id
                    JOIN users m ON u.manager_id = m.id
                    WHERE la.id = @appId;
                ";

                EmployeeLeaveEmailDTO emailData = null;
                string managerEmail = null;
                string managerName = null;

                await using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@appId", appId);

                    await using var reader = await cmd.ExecuteReaderAsync();
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

                        managerEmail = reader["manager_email"]?.ToString();
                        managerName = reader["manager_name"]?.ToString() ?? "Manager";
                    }
                }

                if (emailData == null)
                    return true; // no email data found, skip sending email

                string statusColor = dto.Action.ToLower() == "approved" ? "#4CAF50" : "#F44336";

                // 3️⃣ Email to Employee
                string employeeSubject = $"Your Leave Application {dto.Action.ToUpper()} by Manager";
                string employeeHtml = $@"
                    <div style='font-family:Segoe UI, sans-serif;'>
                        <h3 style='color:{statusColor};'>Your Leave has been {dto.Action.ToUpper()} by Manager!</h3>
                        <p>Dear {emailData.EmployeeName},</p>
                        <p>Your leave application has been <b>{dto.Action.ToUpper()}</b> by your manager.</p>
                        <table style='border-collapse: collapse; margin-top:10px;'>
                            <tr><td><b>Leave Type:</b></td><td>{emailData.LeaveType}</td></tr>
                            <tr><td><b>From:</b></td><td>{emailData.StartDate:dd MMM yyyy}</td></tr>
                            <tr><td><b>To:</b></td><td>{emailData.EndDate:dd MMM yyyy}</td></tr>
                            <tr><td><b>Total Days:</b></td><td>{emailData.TotalDays}</td></tr>
                            <tr><td><b>Manager Comments:</b></td><td>{dto.Comments}</td></tr>
                        </table>
                        <br/>
                        <p>Regards,<br/>HR Team</p>
                    </div>";

                _emailService.SendEmailFireAndForget(emailData.EmployeeEmail, employeeSubject, employeeHtml);

                // 4️⃣ Email to Manager (confirmation)
                if (!string.IsNullOrEmpty(managerEmail))
                {
                    string managerSubject = $"You have {dto.Action.ToUpper()} the Leave Application";
                    string managerHtml = $@"
                <div style='font-family:Segoe UI, sans-serif;'>
                    <h3 style='color:{statusColor};'>You have {dto.Action.ToUpper()} the leave!</h3>
                    <p>Dear {managerName},</p>
                    <p>You have <b>{dto.Action.ToUpper()}</b> the leave application of {emailData.EmployeeName}.</p>
                    <table style='border-collapse: collapse; margin-top:10px;'>
                        <tr><td><b>Leave Type:</b></td><td>{emailData.LeaveType}</td></tr>
                        <tr><td><b>From:</b></td><td>{emailData.StartDate:dd MMM yyyy}</td></tr>
                        <tr><td><b>To:</b></td><td>{emailData.EndDate:dd MMM yyyy}</td></tr>
                        <tr><td><b>Total Days:</b></td><td>{emailData.TotalDays}</td></tr>
                        <tr><td><b>Comments:</b></td><td>{dto.Comments}</td></tr>
                    </table>
                    <br/>
                    <p>Regards,<br/>HR System</p>
                </div>";

                    _emailService.SendEmailFireAndForget(managerEmail, managerSubject, managerHtml);
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error performing manager action on leave.", ex);
            }
        }
    }
}
