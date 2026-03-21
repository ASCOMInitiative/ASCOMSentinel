using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.Hosting;

namespace Sentinel
{
    public class CircuitHandlerService : CircuitHandler
    {
        private readonly IHostApplicationLifetime lifetime; // This is required if the StopApplication method is used.
        private readonly SentinelLogger logger;
        private readonly object connectionsLockObject = new();

        private readonly List<string> connections;
        public CircuitHandlerService(IHostApplicationLifetime lifetime, SentinelLogger logger)
        {
            this.lifetime = lifetime; // This is required if the StopApplication method is used.
            this.logger = logger;

            // Create a new connections object if required
            if (connections is null)
            {
                lock (connectionsLockObject)
                {
                    connections = new();
                }
            }
        }

        public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            logger.LogDebug("CircuitHandler", $"OnConnectionUpAsync - Circuit {circuit.Id} is up. Connection count: {connections.Count}");
            return Task.CompletedTask;
        }

        public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            logger.LogDebug("CircuitHandler", $"OnConnectionDownAsync - Circuit {circuit.Id} is down. Connection count: {connections.Count}");
            return Task.CompletedTask;
        }

        public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            lock (connectionsLockObject)
            {
                try
                {
                    // Add the circuit to the list of circuits if not already in the list (it shouldn't be!)
                    if (!connections.Contains(circuit.Id))
                    {
                        connections.Add(circuit.Id);
                        logger.LogDebug("CircuitHandler", $"OnCircuitOpenedAsync - Added connection {circuit.Id}. Connection count: {connections.Count}");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError("CircuitHandler", $"OnCircuitOpenedAsync {circuit.Id} - {ex.Message}\r\n{ex}");
                }

                return Task.CompletedTask;
            }
        }

        public override async Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            // Include a short delay to allow any new circuits to establish themselves before checking whether the application should close down
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            lock (connectionsLockObject)
            {
                try
                {
                    // Remove the circuit from the circuit list if present (it should be!)
                    if (connections.Contains(circuit.Id))
                    {
                        bool success = connections.Remove(circuit.Id);
                        logger.LogDebug("CircuitHandler", $"OnCircuitClosedAsync - Removed connection {circuit.Id}. Connection count: {connections.Count}, Success: {success}");
                    }

                    // End the application if all circuits are closed
                    if (connections.Count == 0)
                    {
                        logger.LogDebug("CircuitHandler", $"OnCircuitClosedAsync - Calling StopApplication. Circuit: {circuit.Id}, Connection count: {connections.Count}");
                        lifetime.StopApplication();
                    }
                    else
                    {
                        logger.LogDebug("CircuitHandler", $"OnCircuitClosedAsync - Connection count > 0, not calling StopApplication(). Count: {connections.Count}");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError("CircuitHandler", $"OnCircuitClosedAsync {circuit.Id} - {ex.Message}\r\n{ex}");
                }
            }
        }
    }

}
