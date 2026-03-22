using Microsoft.AspNetCore.Components.Server.Circuits;

namespace Sentinel
{
    /// <summary>
    /// Tracks active Blazor circuits for diagnostic logging.
    /// The application continues running regardless of whether any browsers are connected.
    /// </summary>
    public class CircuitHandlerService : CircuitHandler
    {
        private readonly SentinelLogger logger;
        private readonly Lock _lock = new();
        private readonly List<string> _connections = [];

        public CircuitHandlerService(SentinelLogger logger)
        {
            this.logger = logger;
        }

        public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            logger.LogDebug("CircuitHandler", $"OnConnectionUpAsync - Circuit {circuit.Id} is up. Connection count: {_connections.Count}");
            return Task.CompletedTask;
        }

        public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            logger.LogDebug("CircuitHandler", $"OnConnectionDownAsync - Circuit {circuit.Id} is down. Connection count: {_connections.Count}");
            return Task.CompletedTask;
        }

        public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            lock (_lock)
            {
                if (!_connections.Contains(circuit.Id))
                {
                    _connections.Add(circuit.Id);
                    logger.LogDebug("CircuitHandler", $"OnCircuitOpenedAsync - Circuit {circuit.Id} opened. Connection count: {_connections.Count}");
                }
            }
            return Task.CompletedTask;
        }

        public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            lock (_lock)
            {
                _connections.Remove(circuit.Id);
                logger.LogDebug("CircuitHandler", $"OnCircuitClosedAsync - Circuit {circuit.Id} closed. Connection count: {_connections.Count}");
            }
            return Task.CompletedTask;
        }
    }
}
