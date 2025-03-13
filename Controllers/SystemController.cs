using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MiniCrm.Models;

namespace MiniCrm.Controllers;

[ApiController]
public class SystemController : ControllerBase
{

    private readonly IConfiguration _configuration;
    private readonly AppSettings _appSettings;

    public SystemController(IConfiguration configuration, IOptions<AppSettings> appSettings)
    {
        _configuration = configuration;
        _appSettings = appSettings.Value;
    }

    [HttpGet("test")]
    [ProducesResponseType(typeof(string), 200)]
    [ProducesResponseType(500)]
    [ProducesResponseType(400)]
    [Produces("text/plain")]
    public async Task<IActionResult> Test(string? text)
    {
        Console.WriteLine($"GET /test: {text}");
        await Task.Delay(_appSettings.Delay);
        if (Random.Shared.NextDouble() < _appSettings.ErrorFactor)
            return StatusCode(500);

        return Content(text ?? "", "text/plain");
    }

    [HttpGet("version")]
    [ProducesResponseType(typeof(string), 200)] // 💡 Sørg for at Swagger ved, at dette er en string
    [ProducesResponseType(500)]
    [Produces("text/plain")]
    public async Task<IActionResult> Version()
    {
        Console.WriteLine($"GET /version");
        await Task.Delay(_appSettings.Delay);
        if (Random.Shared.NextDouble() < _appSettings.ErrorFactor)
            return StatusCode(500);

        var version = Assembly.GetExecutingAssembly().GetName().Version;
        return Content(version!.ToString(), "text/plain");
    }

}
