using ASCOM.Alpaca;

namespace ObsMan
{
    internal class AlpacaConfiguration : IAlpacaConfiguration
    {
        public bool RunInStrictAlpacaMode => Program.settings.RunInStrictAlpacaMode;

        public bool PreventRemoteDisconnects => Program.settings.PreventRemoteDisconnects;

        public string ServerName => Program.ServerName;

        public string Manufacturer => Program.Manufacturer;

        public string ServerVersion => Program.ServerVersion;

        public string Location => Program.settings.Location;

        public bool AllowImageBytesDownload => Program.settings.AllowImageBytesDownload;

        public bool AllowDiscovery => Program.settings.AllowDiscovery;

        public int ServerPort => Program.settings.ServerPort;

        public bool AllowRemoteAccess => Program.settings.AllowRemoteAccess;

        public bool LocalRespondOnlyToLocalHost => Program.settings.LocalRespondOnlyToLocalHost;

        public bool RunSwagger => Program.settings.RunSwagger;
    }
}