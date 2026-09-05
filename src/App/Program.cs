using App.Configurations;
using App.Extensions;
using App.Middlewares;
using Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// Install services from assemblies implementing IServiceInstaller
builder.Services
    .InstallServices(
        builder.Configuration,
        typeof(IServiceInstaller).Assembly);

// Configure Serilog for logging
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MigrateDatabase<ApplicationDbContext>();

app.UseHttpsRedirection();
app.UseCors("Frontend");

// Use Serilog request logging
app.UseSerilogRequestLogging();

// Use authorization
app.UseAuthorization();

// Register the global exception handling middleware in the request processing pipeline
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

// Map controllers to route endpoints
app.MapControllers();

app.UseRateLimiter();

app.Run();

// auto apply migrations to database function
