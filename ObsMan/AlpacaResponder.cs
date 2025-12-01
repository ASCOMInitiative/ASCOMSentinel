using ASCOM.Alpaca.Discovery;
using LogLevel = ASCOM.Common.Interfaces.LogLevel;
namespace ObsMan
{
    public class AlpacaResponder
    {
        public AlpacaResponder(Logger logger, Settings settings)
        {
            logger.LogMessage("AlpacaResponder", "Init");

            logger.LogMessage("StartResponder", LogLevel.Debug, "About to start responder...");
            Responder responder = new (settings.ApplicationIpPort);
            logger.LogMessage("StartResponder", LogLevel.Information, $"Started Alpaca Responder, reporting port: {settings.ApplicationIpPort}");
        }
    }
}