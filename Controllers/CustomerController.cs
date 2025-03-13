using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MiniCrm.Models;

namespace MiniCrm.Controllers;

[ApiController]
[Route("customers")]
public class CustomerController : ControllerBase
{
    private readonly CustomerDbContext _db;
    private readonly IConfiguration _config;
    private readonly AppSettings _appSettings;

    public CustomerController(CustomerDbContext db, IConfiguration config, IOptions<AppSettings> appSettings)
    {
        _db = db;
        _config = config;
        _appSettings = appSettings.Value;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<Customer>), 200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetCustomers()
    {
        Console.WriteLine("GET /customers");
        await Task.Delay(_config.GetValue<int>("AppSettings:delay"));

        if (Random.Shared.NextDouble() < _config.GetValue<double>("AppSettings:errorFactor"))
            return StatusCode(500);

        return Ok(await _db.Customers.ToListAsync());
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Customer), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetCustomer(int id)
    {
        Console.WriteLine($"GET /customers/{id}");
        await Task.Delay(_config.GetValue<int>("AppSettings:delay"));

        if (Random.Shared.NextDouble() < _config.GetValue<double>("AppSettings:errorFactor"))
            return StatusCode(500);

        var customer = await _db.Customers.FindAsync(id);
        return customer != null ? Ok(customer) : NotFound();
    }

    [HttpGet("findbyname/{name}")]
    [ProducesResponseType(typeof(List<Customer>), 200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> FindCustomerByName(string name)
    {
        Console.WriteLine($"GET /customers/findbyname/{name}");
        await Task.Delay(_config.GetValue<int>("AppSettings:delay"));

        if (Random.Shared.NextDouble() < _config.GetValue<double>("AppSettings:errorFactor"))
            return StatusCode(500);

        var customers = await _db.Customers
            .Where(c => EF.Functions.Like(c.Name, $"%{name}%"))
            .ToListAsync();
        return Ok(customers);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Customer), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> CreateCustomer([FromBody] Customer customer)
    {
        Console.WriteLine("POST /customers");
        await Task.Delay(_config.GetValue<int>("AppSettings:delay"));

        if (Random.Shared.NextDouble() < _config.GetValue<double>("AppSettings:errorFactor"))
            return StatusCode(500);

        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();
        return Created($"/api/customers/{customer.Id}", customer);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> UpdateCustomer(int id, [FromBody] Customer updatedCustomer)
    {
        Console.WriteLine($"PUT /customers/{id}");
        await Task.Delay(_config.GetValue<int>("AppSettings:delay"));

        if (Random.Shared.NextDouble() < _config.GetValue<double>("AppSettings:errorFactor"))
            return StatusCode(500);

        var customer = await _db.Customers.FindAsync(id);
        if (customer == null)
        {
            return NotFound();
        }

        customer.Name = updatedCustomer.Name;
        customer.Age = updatedCustomer.Age;
        customer.Country = updatedCustomer.Country;
        customer.Revenue = updatedCustomer.Revenue;
        customer.CreatedDate = updatedCustomer.CreatedDate;
        customer.IsActive = updatedCustomer.IsActive;
        customer.Tags = updatedCustomer.Tags;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> PatchCustomer(int id, [FromBody] JsonElement updates)
    {
        Console.WriteLine($"PATCH /customers/{id}");
        await Task.Delay(_config.GetValue<int>("AppSettings:delay"));

        if (Random.Shared.NextDouble() < _config.GetValue<double>("AppSettings:errorFactor"))
            return StatusCode(500);

        var customer = await _db.Customers.FindAsync(id);
        if (customer == null)
        {
            return NotFound();
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

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        Console.WriteLine($"DELETE /customers/{id}");
        await Task.Delay(_config.GetValue<int>("AppSettings:delay"));

        if (Random.Shared.NextDouble() < _config.GetValue<double>("AppSettings:errorFactor"))
            return StatusCode(500);

        var customer = await _db.Customers.FindAsync(id);
        if (customer == null)
        {
            return NotFound();
        }

        _db.Customers.Remove(customer);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
