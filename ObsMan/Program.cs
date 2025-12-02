using ASCOM.Alpaca;
using ASCOM.Common;
using ASCOM.Tools;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.Logging;
using Radzen;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ObsMan
{
    public class Program
    {
        //ToDo
        //Fill this with your driver name
        internal const string DriverID = "ObsMan.Alpaca";

        //Change this to a unique value
        //You should offer a way for the end user to customize this via the command line so it can be changed in the case of a collision.
        //This supports --urls=http://*:port by default.
        internal const int DefaultPort = 32324;

        //Fill these out
        internal const string Manufacturer = "Peter Simpson";

        internal const string ServerName = "Peter'sAlpaca Server";
        internal const string ServerVersion = "1.0";

        internal static State state = new();
        internal static Settings settings = new Settings("");
        internal static ObsManLogger logger = new(state, settings);

        internal static IHostApplicationLifetime? Lifetime;

        public static void Main(string[] args)
        {
            //First fill in information for your driver in the Alpaca Configuration Class. Some of these you may want to store in a user changeable settings file.
            //Then fill in the ToDos in this file. Each is marked with a //ToDo
            //You shouldn't need to do anything in the Startup and Logging or Finish Building and Start Server regions


            //This region contains startup and logging features, most of the time you shouldn't need to customize this
            //You can add custom Command Line arguments here
            #region Startup and Logging

            logger.LogInformation($"{ServerName} version {ServerVersion}");
            logger.LogInformation($"Running on: {RuntimeInformation.OSDescription}.");

            //If already running start browser
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    //Already running, start the browser, detects based on port in use
                    var con1 = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections().Where(con => con.LocalEndPoint.Port == ServerSettings.ServerPort);
                    if (IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections().Any(con => con.LocalEndPoint.Port == ServerSettings.ServerPort && (con.State == TcpState.Listen || con.State == TcpState.Established)))
                    {
                        logger.LogInformation("Detected driver port already open, starting web browser on IP and Port. If this fails something else is using the port");
                        StartBrowser(ServerSettings.ServerPort);
                        return;
                    }
                }
                else
                {
                    Assembly? entryAssembly = Assembly.GetEntryAssembly();
                    if (entryAssembly != null)
                    {
                        if (Process.GetProcessesByName(entryAssembly.Location).Length > 1)
                        {
                            logger.LogInformation("Detected driver already running, starting web browser on IP and Port");
                            StartBrowser(ServerSettings.ServerPort);
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
                logger.LogInformation("Reseting Settings");
                ServerSettings.Reset();

                //If you have any device settings you should reset them as well or add a specific reset command.

                return;
            }

            //Turn off Authentication. Once off the user can change the password and re-enable authentication
            if (args?.Any(str => str.Contains("--reset-auth")) ?? false)
            {
                logger.LogInformation("Turning off Authentication to allow password reset.");
                ServerSettings.UseAuth = false;
                logger.LogInformation("Authentication off, you can change the password and then re-enable Authentication.");
            }

            if (args?.Any(str => str.Contains("--local-address")) ?? false)
            {
                Console.WriteLine($"http://localhost:{ServerSettings.ServerPort}");
            }

            if (!args?.Any(str => str.Contains("--urls")) ?? true)
            {
                args ??= [];

                logger.LogInformation("No startup url args detected, binding to saved server settings.");

                var temparray = new string[args.Length + 1];

                args.CopyTo(temparray, 0);

                string startupURLArg = "--urls=http://";

                //If set to allow remote access bind to all local ips, otherwise bind only to localhost
                if (ServerSettings.AllowRemoteAccess)
                {
                    startupURLArg += "*";
                }
                else
                {
                    startupURLArg += "localhost";
                }

                startupURLArg += ":" + ServerSettings.ServerPort;

                logger.LogInformation("Startup URL args: " + startupURLArg);

                temparray[args.Length] = startupURLArg;

                args = temparray;
            }

            var builder = WebApplication.CreateBuilder(args ?? []);

            #endregion Startup and Logging

            //ToDo you can add devices here

            //Attach the logger
            Logging.AttachLogger(logger);

            //Load the configuration
            DeviceManager.LoadConfiguration(new AlpacaConfiguration());

            //Add a safety monitor with device id 0. You can load any number of the same device with different ids or load other devices with Load* functions.
            //You may want to inject settings and logging here to the Driver Instance.
            //For each device you add you should add or edit an existing settings page in the settings folder and an entry in the Shared NavMenu.
            //There are pages already included for the first device of each device type.
            DeviceManager.LoadSafetyMonitor(0, new DeviceAccess.BasicMonitor(), "Really Basic Safety Monitor", ServerSettings.GetDeviceUniqueId("SafetyMonitor", 0));
            DeviceManager.LoadObservingConditions(0, new DeviceAccess.ObservingConditions(), "Observing Conditions Device", ServerSettings.GetDeviceUniqueId("ObservingConditions", 0));
            DeviceManager.LoadSwitch(0, new DeviceAccess.Switch(), "Switch Device", ServerSettings.GetDeviceUniqueId("Switch", 0));

            #region Finish Building and Start server

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();

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

            // Add a StateService singleton to hold application state
            builder.Services.AddSingleton<State>(provider =>
            {
                return state;
            });

            // Add a Logger singleton
            builder.Services.AddSingleton<ObsManLogger>(provider =>
            {
                return logger;
            });

            // Add a Settings singleton that  requires a logger instance as a parameter
            builder.Services.AddSingleton<Settings>(provider =>
            {
                return settings;
            });

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

            app.MapBlazorHub();

            app.MapControllers();

            app.MapFallbackToPage("/_Host");

            if (ServerSettings.AutoStartBrowser)
            {
                try
                {
                    StartBrowser(ServerSettings.ServerPort);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex.Message);
                }
            }

            #endregion Finish Building and Start server

            Lifetime = app.Lifetime;

            //ToDo Put code here that should run at shutdown
            Lifetime.ApplicationStopping.Register(() =>
            {
                logger.LogInformation($"{ServerName} Stopping");
            });

            //Start the Alpaca Server
            app.Run();
        }

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
    }
}