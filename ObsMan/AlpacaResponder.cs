using ASCOM.Alpaca.Discovery;
using ASCOM.Common;
using LogLevel = ASCOM.Common.Interfaces.LogLevel;
namespace ObsMan
{
    public class AlpacaResponder
    {
        public AlpacaResponder(Logger logger, Settings settings)
        {
            logger.LogMessage("AlpacaResponder", "Init");

            logger.LogDebug("StartResponder", "About to start responder...");
            Responder responder = new (settings.ApplicationIpPort);
            logger.LogMessage("StartResponder", $"Started Alpaca Responder, reporting port: {settings.ApplicationIpPort}");
        }
    }
}