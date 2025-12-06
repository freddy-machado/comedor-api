using Comedor.API.Extensions;
using Comedor.Infrastructure.Data;
using Comedor.Infrastructure.Hubs; // Added for DispatchHub
using Microsoft.AspNetCore.Builder;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Host.UseSerilog();

// Add services to the container using extension methods
builder.Services.ConfigureCors();
builder.Services.ConfigureDbContext(builder.Configuration);
builder.Services.ConfigureIdentity();
builder.Services.ConfigureJwt(builder.Configuration);
builder.Services.ConfigureSwagger(); // This now configures SwaggerGen
builder.Services.ConfigureDIServices();

// Add SignalR
builder.Services.AddSignalR(); // Added

// Add AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Configure the HTTP request pipeline.
// Decide whether to enable Swagger: only in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Comedor API v1");
        c.RoutePrefix = string.Empty; // sirve Swagger UI en la raíz (/) — útil en dev
    });
}

// Aplicar seed antes de arrancar la app
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DataSeeder.SeedAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error durante la siembra de datos.");
        throw;
    }
}

app.UseRouting();

app.UseHttpsRedirection();

app.UseSerilogRequestLogging();

app.UseCors("CorsPolicy");

app.UseAuthentication(); // IMPORTANT: Must be before UseAuthorization
app.UseAuthorization();

app.MapControllers();

// Map SignalR Hub
app.MapHub<DispatchHub>("/dispatchHub"); // Added

try
{
    Log.Information("Starting web host");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
