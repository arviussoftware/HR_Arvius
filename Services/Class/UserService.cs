using HR_Arvius.DTOs.Requests.User;
using HR_Arvius.Services.Interface;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Threading.Tasks;

namespace HR_Arvius.Services.Class
{
    public class UserService : IUserService
    {
        private readonly string _connectionString;
        
        public UserService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<UserDTO> GetUserByEmailAsync(string email)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var query = "SELECT id, first_name, last_name, email, employee_id, role, department " +
                                "FROM users WHERE is_active = true AND email = @Email";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var fullName = $"{reader.GetString(reader.GetOrdinal("first_name"))} {reader.GetString(reader.GetOrdinal("last_name"))}";

                                return new UserDTO
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                                    FullName = fullName,
                                    Email = reader.GetString(reader.GetOrdinal("email")),
                                    EmployeeId = reader.GetString(reader.GetOrdinal("employee_id")),
                                    Role = reader.GetString(reader.GetOrdinal("role")),
                                    Department = reader.GetString(reader.GetOrdinal("department"))
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while fetching user: {ex.Message}");
                throw new InvalidOperationException("Failed to retrieve user.");
            }

            return null; // If no user found
        }

        public async Task<List<UserDTO>> GetActiveUsersAsync()
        {
            var users = new List<UserDTO>();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var query = "SELECT id, first_name, last_name, email, employee_id, role, department " +
                                "FROM users WHERE is_active = true";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var fullName = $"{reader.GetString(reader.GetOrdinal("first_name"))} {reader.GetString(reader.GetOrdinal("last_name"))}";

                                var user = new UserDTO
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                                    FullName = fullName,
                                    Email = reader.GetString(reader.GetOrdinal("email")),
                                    EmployeeId = reader.GetString(reader.GetOrdinal("employee_id")),
                                    Role = reader.GetString(reader.GetOrdinal("role")),
                                    Department = reader.GetString(reader.GetOrdinal("department"))
                                };

                                users.Add(user);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while fetching users: {ex.Message}");
                throw new InvalidOperationException("Failed to retrieve users.");
            }

            return users;
        }
        public async Task<CreateUserDTO> AddUserAsync(CreateUserDTO newUser)
        {
            try
            {
                await using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var query = @"INSERT INTO users(
                                email, password, first_name, last_name, employee_id,
                                role, department, location, manager_id, hire_date, is_active
                            ) VALUES (@Email, @Password, @FirstName, @LastName, @EmployeeId,
                                      @Role, @Department, @Location, @ManagerId, @HireDate, true)
                            RETURNING id";

                    var existingUser = await GetUserByEmailAsync(newUser.Email);
                    if (existingUser != null)
                    {
                        throw new InvalidOperationException("User with this email already exists.");
                    }
                    else
                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Email", newUser.Email);
                            command.Parameters.AddWithValue("@Password", BCrypt.Net.BCrypt.HashPassword(newUser.Password));
                            command.Parameters.AddWithValue("@FirstName", newUser.FirstName);
                            command.Parameters.AddWithValue("@LastName", newUser.LastName);
                            command.Parameters.AddWithValue("@EmployeeId", newUser.EmployeeId);
                            command.Parameters.AddWithValue("@Role", newUser.Role);
                            command.Parameters.AddWithValue("@Department", newUser.Department);
                            command.Parameters.AddWithValue("@Location", newUser.Location);
                            command.Parameters.AddWithValue("@ManagerId", (object?)newUser.ManagerId ?? DBNull.Value);
                            command.Parameters.AddWithValue("@HireDate", newUser.HireDate);

                            var userId = await command.ExecuteScalarAsync();

                            if (userId != null)
                            {
                                //newUser.Id = Convert.ToInt32(userId);
                                return newUser;
                            }
                            else
                            {
                                return null;
                            }
                        }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[AddUserAsync ERROR] {e}");
                throw; 
            }
        }
        public async Task<UpdateUserDTO> UpdateUserAsync(UpdateUserDTO updateUser)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // SQL query to update user
                    var query = @"
                        UPDATE users
                        SET 
                            email = @Email,
                            first_name = @FirstName,
                            last_name = @LastName,
                            employee_id = @EmployeeId,
                            role = @Role,
                            department = @Department,
                            location = COALESCE(@Location, location),  
                            manager_id = COALESCE(@ManagerId, manager_id)
                        WHERE id = @Id
                        RETURNING id, first_name, last_name, email, employee_id, role, department, location, manager_id;";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", updateUser.Id);
                        command.Parameters.AddWithValue("@Email", updateUser.Email);
                        command.Parameters.AddWithValue("@FirstName", updateUser.FirstName);
                        command.Parameters.AddWithValue("@LastName", updateUser.LastName);
                        command.Parameters.AddWithValue("@EmployeeId", updateUser.EmployeeId);
                        command.Parameters.AddWithValue("@Role", updateUser.Role);
                        command.Parameters.AddWithValue("@Department", updateUser.Department);
                        command.Parameters.AddWithValue("@Location", (object?)updateUser.Location ?? DBNull.Value); // Handle null
                        command.Parameters.AddWithValue("@ManagerId", (object?)updateUser.ManagerId ?? DBNull.Value); // Handle null

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                // Return updated user data as UpdateUserDTO
                                return new UpdateUserDTO
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                                    FirstName = reader.GetString(reader.GetOrdinal("first_name")),
                                    LastName = reader.GetString(reader.GetOrdinal("last_name")),
                                    Email = reader.GetString(reader.GetOrdinal("email")),
                                    EmployeeId = reader.GetString(reader.GetOrdinal("employee_id")),
                                    Role = reader.GetString(reader.GetOrdinal("role")),
                                    Department = reader.GetString(reader.GetOrdinal("department")),
                                    Location = reader.IsDBNull(reader.GetOrdinal("location")) ? null : reader.GetString(reader.GetOrdinal("location")),
                                    ManagerId = reader.IsDBNull(reader.GetOrdinal("manager_id")) ? null : reader.GetInt32(reader.GetOrdinal("manager_id"))
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while updating user: {ex.Message}");
                throw new InvalidOperationException("Failed to update user.");
            }

            return null; // If update fails
        }
    }
}
