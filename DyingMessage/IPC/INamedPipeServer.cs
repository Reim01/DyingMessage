using DyingMessage.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DyingMessage.IPC
{
    public interface INamedPipeServer
    {
        Task StartAsync(CancellationToken cancellationToken);
        Task BroadcastMetricAsync(HardwareMetric metric, CancellationToken cancellationToken);
    }
}
