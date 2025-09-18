using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MiniCrm.Models;
using MiniCrm.Services;
using System.Globalization;
using System.Text;
using System.Text.Json;

internal class Program
{
    private static void Main(string[] args)
    {
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");

        var builder = WebApplication.CreateBuilder(args);

        // Konfigurer Kestrel til at bruge port 5000
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenLocalhost(5000);
        });

        // Læs appsettings
        var appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>()!;
        var jwtSettings = builder.Configuration.GetSection("JwtSettings");

        // Register AppSettings
        builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

        // Konfigurer JWT authentication
        var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(secretKey)
                };

                // Add custom token validation to check if token is revoked
                // This ensures that revoked tokens are immediately invalidated for all protected endpoints
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var jwtService = context.HttpContext.RequestServices.GetRequiredService<JwtService>();
                        
                        // Extract the raw JWT token from the Authorization header
                        var authHeader = context.Request.Headers.Authorization.ToString();
                        if (authHeader.StartsWith("Bearer "))
                        {
                            var token = authHeader.Substring("Bearer ".Length).Trim();
                            
                            if (jwtService.IsTokenRevoked(token))
                            {
                                context.Fail("Token has been revoked");
                            }
                        }
                        
                        return Task.CompletedTask;
                    }
                };
            });

        builder.Services.AddAuthorization();

        // Konfigurer database
        builder.Services.AddDbContext<CustomerDbContext>(options =>
            options.UseSqlite("Data Source=customers.db"));

        // Tilføj controllers
        builder.Services.AddControllers();

        // Add CORS policy to allow all origins
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        // Register JwtService
        builder.Services.AddScoped<JwtService>();

        // Tilføj Swagger
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.EnableAnnotations();
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "MiniCRM API",
                Version = "v1",
                Description = "A simple example ASP.NET Core Web API for a mini CRM application",
                Contact = new OpenApiContact
                {
                    Name = "Repository",
                    Url = new Uri("https://github.com/devcronberg/minicrm")
                }
            });
        });

        var app = builder.Build();

        if (args.Length == 0)
        {
            HandleDatabaseInitialization(app);
        }
        else if (args.Length > 0 && args[0] == "/nodbinit")
        {
            System.Console.WriteLine("Database initialization skipped");
        }

        if (args.Length > 0 && args[0] == "init")


            Console.WriteLine("Starting the application - listening on http://localhost:5000 - Press Ctrl+C to exit.");

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/error");
        }

        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseRouting();

        // Use CORS middleware - must be called before UseAuthentication and UseAuthorization
        app.UseCors();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();

        void HandleDatabaseInitialization(WebApplication app)
        {
            string dbPath = "customers.db";
            if (File.Exists(dbPath))
            {
                Console.WriteLine();
                Console.WriteLine("An existing customers database was found.");
                Console.Write("Do you want to delete it and start fresh? (y/n): ");
                var response = Console.ReadKey().KeyChar;

                if (response == 'y' || response == 'Y')
                {
                    File.Delete(dbPath);
                    Console.WriteLine("\nDatabase deleted. Starting fresh...");
                }
                else
                {
                    Console.WriteLine("\nKeeping existing database...");
                }
            }

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();
                db.Database.EnsureCreated();

                if (!db.Customers.Any())
                {
                    var jsonData = File.ReadAllText("customers.json");
                    var customers = JsonSerializer.Deserialize<List<Customer>>(jsonData);
                    if (customers != null)
                    {
                        db.Customers.AddRange(customers);
                        db.SaveChanges();
                    }
                }
            }
        }
    }
}