using ASCOM.Alpaca;
using ASCOM.Common;
using ASCOM.Common.DeviceInterfaces;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Radzen;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Sentinel
{
    public class Program
    {
        internal const string DriverID = "ASCOMSentinel"; // Device name

        internal const int DefaultPort = 32324; // Default port

        internal const string Manufacturer = "Peter Simpson";

        internal const string ServerName = Globals.APPLICATION_NAME;
        internal const string ServerVersion = Globals.APPLICATION_VERSION;

        internal static State state = new();
        internal static Settings settings = new Settings(string.Empty);
        internal static SentinelLogger logger = new(state, settings);

        internal static IHostApplicationLifetime? applicationLifetime;
        internal static bool RestartRequested;

        public static async Task  Main(string[] args)
        {

            #region Startup and Logging

            logger.LogMessage("", $"{ServerName} version {ServerVersion}");
            logger.LogMessage("", $"Running on: {RuntimeInformation.OSDescription}.");

            //If already running start browser
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    //Already running, start the browser, detects based on port in use
                    var con1 = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections().Where(con => con.LocalEndPoint.Port == settings.ServerPort);
                    if (IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections().Any(con => con.LocalEndPoint.Port == settings.ServerPort && (con.State == TcpState.Listen || con.State == TcpState.Established)))
                    {
                        logger.LogMessage("", "Detected driver port already open, starting web browser on IP and Port. If this fails something else is using the port");
                        StartBrowser(settings.ServerPort);
                        return;
                    }
                }
                else
                {
                    Assembly? entryAssembly = Assembly.GetEntryAssembly();
                    if (entryAssembly != null)
                    {
                        if (Process.GetProcessesByName(System.AppContext.BaseDirectory).Length > 1)
                        {
                            logger.LogMessage("", "Detected driver already running, starting web browser on IP and Port");
                            StartBrowser(settings.ServerPort);
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return;
            }

            //Reset all stored settings if requested
            if (args?.Any(str => str.Contains("--reset")) ?? false)
            {
                logger.LogMessage("", "Resetting Settings");
                settings.ResetToDefaults();

                //If you have any device settings you should reset them as well or add a specific reset command.

                return;
            }

            //Turn off Authentication. Once off the user can change the password and re-enable authentication
            if (args?.Any(str => str.Contains("--reset-auth")) ?? false)
            {
                logger.LogMessage("", "Turning off Authentication to allow password reset.");
                settings.UseAuth = false;
                settings.Save();
                logger.LogMessage("", "Authentication off, you can change the password and then re-enable Authentication.");
            }

            if (args?.Any(str => str.Contains("--local-address")) ?? false)
            {
                Console.WriteLine($"http://localhost:{settings.ServerPort}");
            }

            var builder = WebApplication.CreateBuilder(args ?? []);

            // Configure Kestrel to listen on the saved server port unless the user explicitly provided --urls on the command line.
            if (!args?.Any(str => str.Contains("--urls")) ?? true)
            {
                string host = settings.BindToAllNetworkAddresses ? "*" : "localhost";
                string listenUrl = $"http://{host}:{settings.ServerPort}";
                logger.LogMessage("", $"No --urls arg detected, binding to: {listenUrl}");
                builder.WebHost.UseUrls(listenUrl);
            }

            // Remove the default ASP.NET console logger and replace with one customised to create output in the application's colour and format
            builder.Logging.ClearProviders(); // Remove default console logger
            builder.Logging.AddProvider(new ConsoleLoggerProvider(settings.LogLevel.ToMSLogLevel())); // Add the customised logger

            //Attach the main logger to the Alpaca.Razor components
            Logging.AttachLogger(logger);

            #endregion Startup and Logging

            #region Configuration and Device Loading

            //Load the configuration
            DeviceManager.LoadConfiguration(new AlpacaConfiguration());

            // Create the safety monitor, observing conditions and switch devices that will be exposed to clients, save them to state and load them for use by clients.

            ISafetyMonitorV3 safetyMonitor = new DeviceAccess.SafetyMonitor(settings, state, logger);
            DeviceManager.LoadSafetyMonitor(0, safetyMonitor, $"{Globals.SAFETY_MONITOR_NAME} ({settings.Location})", settings.GetDeviceUniqueId("SafetyMonitor", 0));
            state.SafetyMonitor = safetyMonitor;

            IObservingConditionsV2 observingConditions = new DeviceAccess.ObservingConditions(settings, state, logger);
            DeviceManager.LoadObservingConditions(0, observingConditions, $"{Globals.OBSERVING_CONDITIONS_NAME} ({settings.Location})", settings.GetDeviceUniqueId("ObservingConditions", 0));
            state.ObservingConditions = observingConditions;

            DeviceManager.LoadSwitch(0, new DeviceAccess.Switch(), $"{Globals.APPLICATION_SHORT_NAME} - Switch Device ({settings.Location})", settings.GetDeviceUniqueId("Switch", 0));

            // Connect to devices if required
            if (settings.AutoConnect && !state.Connected)
                await ConnectionManager.ConnectAsync(state,settings,logger);




            #endregion

            #region Finish Building

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor(options =>
                {
                    options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromSeconds(Globals.DISCONNECTED_CIRCUIT_RETENTION_PERIOD);
                });

            // Limit how long the host waits for graceful shutdown so the app exits promptly when stopped via the UI (default is 30 seconds).
            builder.Host.ConfigureHostOptions(options =>
                {
                    options.ShutdownTimeout = TimeSpan.FromSeconds(Globals.APPLICATION_SHUTDOWN_TIMEOUT);
                });

            //Set default behaviors for Alpaca APIs
            ASCOM.Alpaca.Razor.StartupHelpers.ConfigureAlpacaAPIBehavoir(builder.Services);

            //Use Authentication
            ASCOM.Alpaca.Razor.StartupHelpers.ConfigureAuthentication(builder.Services);

            //Add User Service
            builder.Services.AddScoped<IUserService, Data.UserService>();

            //Load any xml comments for this program, this helps with swagger
            var xmlPath = " Path.Combine(AppContext.BaseDirectory, xmlFile)";

            //Add Swagger for the APIs
            ASCOM.Alpaca.Razor.StartupHelpers.ConfigureSwagger(builder.Services, xmlPath);

            builder.Services.AddRadzenComponents();

            // Per-circuit state (window size, scroll position) — scoped so each browser tab is isolated
            builder.Services.AddScoped<PerBrowserState>();

            // Add a StateService singleton to hold application state
            builder.Services.AddSingleton<State>(provider =>
            {
                return state;
            });

            // Add a Logger singleton
            builder.Services.AddSingleton<SentinelLogger>(provider =>
            {
                return logger;
            });

            // Add a Settings singleton that  requires a logger instance as a parameter
            builder.Services.AddSingleton<Settings>(provider =>
            {
                return settings;
            });

            // Add the password manager singleton for administrator authentication
            builder.Services.AddSingleton<PasswordManager>();

            // Initialise state with any values from settings that are needed at startup
            state.EnableRemoteClients = settings.EnableRemoteClients;
            state.RequireAdministratorLoginAtStartup = settings.RequireAdministratorLogin;

            // Add event handler to detect when the browser closes
            builder.Services.AddSingleton<CircuitHandler, CircuitHandlerService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }

            //Start Swagger on the Swagger endpoints if enabled.
            ASCOM.Alpaca.Razor.StartupHelpers.ConfigureSwagger(app);

            //Configure Discovery
            ASCOM.Alpaca.Razor.StartupHelpers.ConfigureDiscovery(app);

            //Allow authentication, either Cookie or Basic HTTP Auth
            ASCOM.Alpaca.Razor.StartupHelpers.ConfigureAuthentication(app);

            app.UseStaticFiles();

            app.UseRouting();

            app.MapBlazorHub(options =>
            {
                // Default is 5 seconds — reduce so the app exits promptly on shutdown
                options.WebSockets.CloseTimeout = TimeSpan.FromSeconds(Globals.WEBSOCKET_CLOSE_TIMEOUT);
            });

            app.MapControllers();

            app.MapFallbackToPage("/_Host");

            // Start the browser.
            try
            {
                if (args?.Any(str => str.Contains("--nobrowser")) ?? false)
                { } // Don't start the browser if the user requested not to
                else
                    StartBrowser(settings.ServerPort);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex.Message);
            }


            // Register events to log shutdown progress, this helps with troubleshooting shutdown issues and ensures we log the shutdown even if the browser is closed before the server is stopped.
            applicationLifetime = app.Lifetime;
            applicationLifetime.ApplicationStopping.Register(() =>
            {
                logger.LogMessage(nameof(Main), "Application shutting down...");
            });

            applicationLifetime.ApplicationStopped.Register(() =>
            {
                logger.LogBlankLine();
                logger.LogMessage(nameof(Main), "Application shutdown complete.");
            });

            #endregion Finish Building

            #region Start the application and handle re-start requests

            //Start the Alpaca Server. Execution stays here until the server is stopped via the UI.
            app.Run();

            // The application is now stopped, 

            // Check whether a restart was requested and start a new instance if required.
            if (RestartRequested)
            {
                try
                {
                    string? processPath = Environment.ProcessPath;
                    if (!string.IsNullOrWhiteSpace(processPath))
                    {
                        // Start a new instance of the application with the --nobrowser argument to avoid opening another browser window on restart.
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = processPath,
                            Arguments = "--nobrowser",
                            UseShellExecute = true
                        });
                    }
                    else
                    {
                        logger.LogError(nameof(Main), "Unable to restart: could not determine the application executable path.");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(nameof(Main), $"Failed to start new application instance: {ex.Message}");
                }
            }

            #endregion

        }

        #region Support code

        /// <summary>
        /// Starts the system default handler (normally a browser) for local host and the current port.
        /// </summary>
        /// <param name="port"></param>
        internal static void StartBrowser(int port)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = string.Format("http://localhost:{0}", port),
                UseShellExecute = true
            };
            Process.Start(psi);
        }

        #endregion

    }
}
