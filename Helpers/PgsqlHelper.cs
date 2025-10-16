using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace HR_Arvius.Helpers
{
    public static class PgsqlHelper
    {
        // Execute a scalar query (returns single value)
        public static async Task<T> ExecuteScalarAsync<T>(string connectionString, string query, Action<NpgsqlCommand>? paramSetter = null)
        {
            using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(query, conn);
            paramSetter?.Invoke(cmd);

            var result = await cmd.ExecuteScalarAsync();
            if (result == null || result == DBNull.Value) return default!;
            return (T)result;
        }

        // Execute a non-query command (INSERT, UPDATE, DELETE)
        public static async Task<int> ExecuteNonQueryAsync(string connectionString, string query, Action<NpgsqlCommand>? paramSetter = null)
        {
            using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(query, conn);
            paramSetter?.Invoke(cmd);

            return await cmd.ExecuteNonQueryAsync();
        }

        // Execute a reader and return a list of mapped objects
        public static async Task<List<T>> ExecuteReaderAsync<T>(string connectionString, string query, Func<IDataReader, T> map, Action<NpgsqlCommand>? paramSetter = null)
        {
            var list = new List<T>();
            using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(query, conn);
            paramSetter?.Invoke(cmd);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(map(reader));
            }
            return list;
        }

        // Fill DataSet (for legacy code that uses adapters)
        public static async Task<DataSet> FillDataSetAsync(string connectionString, string query, Action<NpgsqlCommand>? paramSetter = null)
        {
            var ds = new DataSet();
            using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(query, conn);
            paramSetter?.Invoke(cmd);

            using var adapter = new NpgsqlDataAdapter(cmd);
            adapter.Fill(ds);

            return ds;
        }

        // Optional: Transaction helper
        public static async Task ExecuteTransactionAsync(string connectionString, Func<NpgsqlConnection, NpgsqlTransaction, Task> action)
        {
            using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();

            using var tran = conn.BeginTransaction();
            try
            {
                await action(conn, tran);
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
