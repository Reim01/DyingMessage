using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.IPC.Services
{
    //DB데이터 정리 및 Analyzer로 넘겨주는용
    public class ReportBuilder
    {
        private readonly string _connectionString;

        public ReportBuilder()
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var dbPath = Path.Combine(basePath, "metrics.db");

            _connectionString = $"Data Source={dbPath};Mode=ReadWriteCreate;Cache=Shared;";

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, Timestamp, CpuLoad, MemoryLoad, GpuLoad 
                FROM Metrics 
                ORDER BY Id DESC 
                LIMIT 100;";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var item = new MetricItem
                {
                    Id = reader.GetInt64(0),
                    Timestamp = reader.GetString(1),
                    // REAL 컬럼은 NULL일 수 있으므로 DBNull 체크 처리
                    CpuLoad = reader.IsDBNull(2) ? null : reader.GetDouble(2),
                    MemoryLoad = reader.IsDBNull(3) ? null : reader.GetDouble(3),
                    GpuLoad = reader.IsDBNull(4) ? null : reader.GetDouble(4)
                };

                metricsList.Add(item);
            }
        }
    }
}
