using ObsMan;
using Radzen;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Add Radzen components
builder.Services.AddRadzenComponents();

builder.Services.AddSingleton<Logger>();

// Add a Settings singleton that  requires a logger instance as a parameter
builder.Services.AddSingleton<Settings>(provider =>
{
    Logger  localSetup = provider.GetRequiredService<Logger>();
    return new Settings(localSetup);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
