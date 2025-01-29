using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.IO;
using System.Reflection;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to use port 5000
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5000);
});

// Add services to the container
builder.Services.AddDbContext<CustomerDbContext>(options =>
    options.UseSqlite("Data Source=customers.db"));

builder.Services.AddControllers();

// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
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

// Inform the user about the readme.md file
Console.WriteLine("Welcome to this MiniCRM. For more information, please refer to the readme.md file.");

// Handle database initialization
HandleDatabaseInitialization(app);

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
}

// Enable middleware to serve generated Swagger as a JSON endpoint
app.UseSwagger();

// Enable middleware to serve Swagger UI at the root URL
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MiniCRM API v1");
    c.RoutePrefix = string.Empty; // Serve the Swagger UI at the app's root
});

app.UseRouting();

// Configure endpoints
ConfigureEndpoints(app);

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

void ConfigureEndpoints(WebApplication app)
{
    app.MapGet("/customers", async (CustomerDbContext db) =>
    {
        Console.WriteLine("GET /customers");
        return Results.Ok(await db.Customers.ToListAsync());
    });

    app.MapGet("/customers/{id}", async (CustomerDbContext db, int id) =>
    {
        Console.WriteLine($"GET /customers/{id}");
        var customer = await db.Customers.FindAsync(id);
        return customer != null ? Results.Ok(customer) : Results.NotFound();
    });

    app.MapGet("/customers/findbyname/{name}", async (CustomerDbContext db, string name) =>
    {
        Console.WriteLine($"GET /customers/findbyname/{name}");
        var customers = await db.Customers
            .Where(c => EF.Functions.Like(c.Name, $"%{name}%"))
            .ToListAsync();
        return Results.Ok(customers);
    });

    app.MapPost("/customers", async (CustomerDbContext db, Customer customer) =>
    {
        Console.WriteLine("POST /customers");
        db.Customers.Add(customer);
        await db.SaveChangesAsync();
        return Results.Created($"/customers/{customer.Id}", customer);
    });

    app.MapPost("/api/customers", async (CustomerDbContext db, Customer customer) =>
    {
        Console.WriteLine("POST /api/customers");
        db.Customers.Add(customer);
        await db.SaveChangesAsync();
        return Results.Created($"/api/customers/{customer.Id}", customer);
    });

    app.MapPut("/api/customers/{id}", async (CustomerDbContext db, int id, Customer updatedCustomer) =>
    {
        Console.WriteLine($"PUT /api/customers/{id}");
        var customer = await db.Customers.FindAsync(id);
        if (customer == null)
        {
            return Results.NotFound();
        }

        customer.Name = updatedCustomer.Name;
        customer.Age = updatedCustomer.Age;
        customer.Country = updatedCustomer.Country;
        customer.Revenue = updatedCustomer.Revenue;
        customer.CreatedDate = updatedCustomer.CreatedDate;
        customer.IsActive = updatedCustomer.IsActive;
        customer.Tags = updatedCustomer.Tags;

        await db.SaveChangesAsync();
        return Results.NoContent();
    });

    app.MapPatch("/api/customers/{id}", async (CustomerDbContext db, int id, JsonElement updates) =>
    {
        Console.WriteLine($"PATCH /api/customers/{id}");
        var customer = await db.Customers.FindAsync(id);
        if (customer == null)
        {
            return Results.NotFound();
        }

        foreach (var property in updates.EnumerateObject())
        {
            switch (property.Name)
            {
                case "name":
                    customer.Name = property.Value.GetString() ?? customer.Name;
                    break;
                case "age":
                    customer.Age = property.Value.GetInt32();
                    break;
                case "country":
                    customer.Country = property.Value.GetString() ?? customer.Country;
                    break;
                case "revenue":
                    customer.Revenue = property.Value.GetDouble();
                    break;
                case "createdDate":
                    customer.CreatedDate = property.Value.GetDateTime();
                    break;
                case "isActive":
                    customer.IsActive = property.Value.GetBoolean();
                    break;
                case "tags":
                    customer.Tags = property.Value.Deserialize<List<string>>() ?? customer.Tags;
                    break;
            }
        }

        await db.SaveChangesAsync();
        return Results.NoContent();
    });

    app.MapDelete("/api/customers/{id}", async (CustomerDbContext db, int id) =>
    {
        Console.WriteLine($"DELETE /api/customers/{id}");
        var customer = await db.Customers.FindAsync(id);
        if (customer == null)
        {
            return Results.NotFound();
        }

        db.Customers.Remove(customer);
        await db.SaveChangesAsync();
        return Results.NoContent();
    });

    // Add a /test endpoint that returns a text (mirror)
    app.MapGet("/test", (string text) =>
    {
        Console.WriteLine($"GET /test: {text}");
        return Results.Ok(text);
    });

    // Add a /version endpoint that returns the version
    app.MapGet("/version", () =>
    {
        Console.WriteLine("GET /version");
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        return Results.Ok($"{version?.Major}.{version?.Minor}.{version?.Build}");
    });
}