using Authentication.ServiceProvider;
using Core.Settings;
using Serilog;
using Serilog.Formatting.Json;

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;
var config = builder.Configuration;
var services = builder.Services;

BuildConfiguration();
ConfigureLogging();

void BuildConfiguration()
{
    builder.Configuration
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .AddJsonFile("twilio.json", optional: false, reloadOnChange: false)
        .AddEnvironmentVariables()
        .Build();
    ConfigurationHelper.AppSettings = new AppSettings();
    ConfigurationHelper.Twilio = new TwilioConfiguration();
    ConfigurationHelper.JwtSettings = new JwtSettings();

    config.GetSection("AppSettings").Bind(ConfigurationHelper.AppSettings);
    config.GetSection("TwilioConfiguration").Bind(ConfigurationHelper.Twilio);
    config.GetSection("JwtSettings").Bind(ConfigurationHelper.JwtSettings);

    services.AddOptions();
    services.Configure<AppSettings>(config.GetSection("AppSettings"));
    services.Configure<JwtSettings>(config.GetSection("JwtSettings"));
    services.Configure<TwilioConfiguration>(config.GetSection("TwilioConfiguration"));
}

void ConfigureLogging()
{
    //Temp:Registreing Serilog provider for dev
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(config) // This line causes the error
        .WriteTo.Console(new JsonFormatter(renderMessage: true))
        .CreateLogger();
    services.AddSingleton(Log.Logger);
    //wire up serilog
    builder.Host.UseSerilog();
}

// Add services to the container.
await ServicesProvider.RegisterServices(builder.Services, builder.Configuration).ConfigureAwait(false);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseDeveloperExceptionPage();

app.UseCors("CorsPolicy");
app.UseHealthChecks("/hearbeat");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
