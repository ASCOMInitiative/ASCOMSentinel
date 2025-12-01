using ASCOM.Common;
using Microsoft.Extensions.DependencyInjection;
using ObsMan;
using Radzen;

Settings settings=new ("");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Add the API controllers
builder.Services.AddControllers();

// Add Radzen components
builder.Services.AddRadzenComponents();

// Add a StateService singleton to hold application state
builder.Services.AddSingleton<State>();

// Add a Logger singleton
builder.Services.AddSingleton<Logger>();

// Add a Settings singleton that  requires a logger instance as a parameter
builder.Services.AddSingleton<Settings>(provider =>
{
    return settings;
});

// Configure the application to listen on the configured port (32324 by default)
builder.WebHost.UseUrls($"http://localhost:{settings.ApplicationIpPort}");

// Add an Alpaca responder singleton
builder.Services.AddSingleton<AlpacaResponder>();

var app = builder.Build();

// Map the API controllers
app.MapControllers();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
   app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");



app.Run();
