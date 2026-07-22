using LibreHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DyingMessage.Core
{
    public record HardwareMetric
    {
        public DateTime Timestamp { get; init; }
        public float CpuLoad { get; init; }
        public float MemoryLoad { get; init; }
        public float GpuLoad { get; init; }
    }

    public interface IHardwareCollector : IDisposable
    {
        HardwareMetric Collect();
    }

    public class HardwareCollector : IHardwareCollector, IVisitor
    {
        private readonly Computer _computer;
        private float? _cpuLoad;
        private float? _memoryLoad;
        private float? _gpuLoad;
        private readonly ILogger<Worker> _logger;
        public HardwareCollector(ILogger<Worker> logger)
        {
            _logger = logger;

            _computer = new Computer
            {
                IsCpuEnabled = true,
                IsMemoryEnabled = true,
                IsGpuEnabled = true,
                IsMotherboardEnabled = false,
                IsControllerEnabled = false,
                IsNetworkEnabled = false,
                IsStorageEnabled = false
            };

            try
            {
                _computer.Open();
            }catch(Exception ex)
            {
                Debug.WriteLine($"HardwareCollector Open 실패 : {ex.Message}");
            }
        }

        public HardwareMetric Collect()
        {
            _cpuLoad = null;
            _memoryLoad = null;
            _gpuLoad = null;

            try
            {
                _computer.Accept(this);
            }catch(Exception ex)
            {
                Debug.WriteLine($"센서 갱신 실패 : {ex.Message}");
            }

            return new HardwareMetric
            {
                Timestamp = DateTime.UtcNow,
                CpuLoad = _cpuLoad ?? 0,
                MemoryLoad = _memoryLoad ?? 0,
                GpuLoad = _gpuLoad ?? 0
            };
        }

        public void VisitComputer(IComputer computer) => computer.Traverse(this);
        public void VisitHardware(IHardware hardware)
        {
            hardware.Update();
            foreach (IHardware sub in hardware.SubHardware) sub.Accept(this);

            foreach (ISensor sensor in hardware.Sensors)
            {
                if (!sensor.Value.HasValue) continue;

                if (hardware.HardwareType == HardwareType.Cpu &&
                    sensor.SensorType == SensorType.Load &&
                    (sensor.Name.Contains("Total") || sensor.Name.Contains("Average")))
                {
                    _cpuLoad = sensor.Value;
                }

                if(hardware.HardwareType == HardwareType.Memory &&
                    sensor.SensorType == SensorType.Load &&
                    sensor.Name.Contains("Memory"))
                {
                    _memoryLoad = sensor.Value;
                }

                if ((hardware.HardwareType == HardwareType.GpuNvidia ||
                    hardware.HardwareType == HardwareType.GpuAmd ||
                    hardware.HardwareType == HardwareType.GpuIntel) &&
                    sensor.SensorType == SensorType.Load &&
                    (sensor.Name.Contains("D3D 3D"))) // 3d엔진 사용률만 가져옴, Performance Counter를 따로 구현해야할 필요
                {
                    if (_gpuLoad == null || sensor.Value > _gpuLoad)
                    {
                        _gpuLoad = sensor.Value;
                    }
                }
            }
        }

        public void VisitSensor(ISensor sensor) { }
        public void VisitParameter(IParameter parameter) { }

        public void Dispose()
        {
            try { _computer.Close(); } catch { }
        }
    }
}
