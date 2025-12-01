using ASCOM.Alpaca.Discovery;
using Microsoft.AspNetCore.Mvc;
namespace ObsMan
{
    [ApiController]
    [Route("management")]
    public class ManagementApiRoot : ControllerBase
    {
        private State state;

        public ManagementApiRoot(State state)
        {
            this.state = state;
        }

        [FromQuery(Name = "ClientTransactionId")]
        public uint ClientTransactionId { get; set; } = 0;

        [HttpGet("apiversions")]
        public string ApiVersions()
        {
            return $"{{\"Value\": [1], \"ClientTransactionID\": {ClientTransactionId}, \"ServerTransactionID\": {state.GetServerTransactionId()}}}";
        }
    }

    [ApiController]
    [Route("management/v1")]
    public class ManagementV1ApiRoot : ControllerBase
    {
        private readonly Settings settings;
        private State state;

        [FromQuery(Name = "ClientTransactionId")]
        public uint ClientTransactionId { get; set; } = 0;

        public ManagementV1ApiRoot(Settings settings, State state)
        {
            this.settings = settings;
            this.state = state;
        }

        [HttpGet("description")]
        public AlpacaDescriptionResponse Description()
        {
            return new AlpacaDescriptionResponse(ClientTransactionId, state.GetServerTransactionId(), new AlpacaDeviceDescription(settings.ServerName, Globals.MANUFACTURER_NAME, Globals.MANUFACTURER_VERSION, settings.Location));
        }

        [HttpGet("configureddevices")]
        public AlpacaConfiguredDevicesResponse ConfiguredDevices()
        {
            return new AlpacaConfiguredDevicesResponse(ClientTransactionId, state.GetServerTransactionId(), new List<AlpacaConfiguredDevice>()
                { 
                    new AlpacaConfiguredDevice("ObsMan Composite Observing Conditions", "ObservingConditions", 0, "123456"),
                    new AlpacaConfiguredDevice("ObsMan Composite Safety Monitor", "SafetyMonitor", 0, "7890"),
                    new AlpacaConfiguredDevice("ObsMan Composite Switch", "Switch", 0, "567879")
                });
        }
    }
}
