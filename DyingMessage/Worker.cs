using DyingMessage.Core;
using DyingMessage.Storage;

namespace DyingMessage
{
    public class Worker : BackgroundService
    {
        private readonly IHardwareCollector _collector;
        private readonly ILocalRepository _repository;
        private readonly ILogger<Worker> _logger;

        public Worker(
            IHardwareCollector collector,
            ILocalRepository repository,
            ILogger<Worker> logger)
        {
            _collector = collector;
            _repository = repository;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));

            _logger.LogInformation("모니터링 시작 (CPU, RAM, GPU 사용량)");

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    var metric = _collector.Collect();
                    await _repository.SaveAsync(metric, stoppingToken);

                    _logger.LogInformation("[{Time}] CPU: {Cpu:F1}%, RAM: {Mem:F1}%, GPU: {Gpu:F1}% 저장 완료",
                        metric.Timestamp.ToLocalTime().ToString("HH:mm:ss"),
                        metric.CpuLoad,
                        metric.MemoryLoad,
                        metric.GpuLoad);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "데이터 수집 또는 저장 중 오류 발생");
                }
            }
        }
    }
}
