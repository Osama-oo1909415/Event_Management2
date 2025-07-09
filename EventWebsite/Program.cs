var builder = WebApplication.CreateBuilder(args);

// This explicitly tells the application to scan for controller classes like EventsController.
builder.Services.AddControllers();

// This part creates the Umbraco application builder
builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddDeliveryApi()
    .AddComposers()
    .Build();

var app = builder.Build();

await app.BootUmbracoAsync();

// This is the standard Umbraco middleware and endpoint configuration
app.UseUmbraco()
    .WithMiddleware(u =>
    {
        u.UseBackOffice();
        u.UseWebsite();
    })
    .WithEndpoints(u =>
    {
        u.UseInstallerEndpoints();
        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints();
    });

// --- THIS IS THE FIX ---
// MapControllers() must be called on the main 'app' object, not inside the Umbraco endpoints.
app.MapControllers();

await app.RunAsync();
