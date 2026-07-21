using DyingMessage.Core;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DyingMessage.Storage
{
    public interface ILocalRepository
    {
        Task SaveAsync(HardwareMetric metric, CancellationToken cancellationToken);
    }

    public class SQLiteRepository : ILocalRepository
    {
        private readonly string _connectionString;

        public SQLiteRepository()
        {
            SQLitePCL.Batteries.Init();

            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var dbPath = Path.Combine(basePath, "metrics.db");

            _connectionString = $"Data Source={dbPath};Mode=ReadWriteCreate;Cache=Shared;";

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            // AUTOINCREMENT PK + WAL 모드로 강종 안전성 확보
            command.CommandText = @"
            PRAGMA journal_mode = WAL;
            PRAGMA synchronous = NORMAL;
            CREATE TABLE IF NOT EXISTS Metrics (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Timestamp TEXT NOT NULL,
                CpuLoad REAL,
                MemoryLoad REAL,
                GpuLoad REAL
            );";
            command.ExecuteNonQuery();
        }

        public async Task SaveAsync(HardwareMetric metric, CancellationToken cancellationToken)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = @"
            INSERT INTO Metrics (Timestamp, CpuLoad, MemoryLoad, GpuLoad)
            VALUES ($ts, $cpu, $mem, $gpu);";

            command.Parameters.AddWithValue("$ts", metric.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            command.Parameters.AddWithValue("$cpu", metric.CpuLoad);
            command.Parameters.AddWithValue("$mem", metric.MemoryLoad);
            command.Parameters.AddWithValue("$gpu", metric.GpuLoad);

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
