using Core.Interfaces;
using Core.Managers;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace Authentication.ServiceProvider
{
    public static class ServicesProvider
    {
        public static async Task RegisterServices(IServiceCollection services, ConfigurationManager config)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {
                    builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
                });
            });
            services.AddHealthChecks();
            services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            //services.AddApplicationInsightsTelemetry(new ApplicationInsightsServiceOptions
            //{
            //    EnableAdaptiveSampling = false,
            //    InstrumentationKey = ConfigurationHelper.AppSettings.AppInsights_InstrumentationKey
            //});
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Auth API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "This site uses Bearer token and you have to pass" +
                     "it as Bearer<<space>>Token",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                    new OpenApiSecurityScheme
                    {
                        Reference=new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id="Bearer"
                        },
                        Scheme="oauth2",
                        Name="Bearer",
                        In = ParameterLocation.Header
                    },
                    new List<string>()
                    }
                });
            });

            //configure dependencies
            services.AddScoped<ITwilioManager, TwilioManager>();
            services.AddTransient<IJWTManager, JWTManager>();

            //services.AddLogging(loggingBuilder =>
            //{
            //    //OPtional: Apply filters to configure loglevel trace or above is sent to application insights for all categories

            //    loggingBuilder.AddFilter<ApplicationInsightsLoggerProvider>("", LogLevel.Warning);
            //    loggingBuilder.AddApplicationInsights();
            //});

            //services.AddDbContext<ApplicationDbContext>(options =>
            //options.UseMySQL(config.GetConnectionString("MySqlConnection"), ServerVersion.AutoDetect(config.GetConnectionString("MySqlConnection"))));

        }
    }
}
