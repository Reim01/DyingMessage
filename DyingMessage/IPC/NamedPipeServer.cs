using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DyingMessage.IPC
{
    public class NamedPipeServer : INamedPipeServer, IDisposable
    {
        private const string PipeName = "HardwareMetricPipe";
        private NamedPipeServerStream? _pipeStream;
        private readonly ILogger<NamedPipeServer> _logger;

        public NamedPipeServer(ILogger<NamedPipeServer> logger)
        {
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _ = Task.Run(() =>
                ListenForClientsAsync(cancellationToken), cancellationToken);
            
            await Task.CompletedTask;
        }

        private async Task ListenForClientsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {

                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "파이프 서버 오류");
                }
                finally
                {
                    //Disconnect();
                }
            }
        }
    }
}
