using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using MiniCrm.Models;

namespace MiniCrm.Controllers;

[ApiController]
public class SystemController : ControllerBase
{

    private readonly IConfiguration _configuration;
    private readonly AppSettings _appSettings;

    public SystemController(IConfiguration configuration, AppSettings appSettings)
    {
        _configuration = configuration;
        _appSettings = appSettings;
    }

    [HttpGet("test")]
    public async Task<IActionResult> Test(string? text)
    {
        Console.WriteLine($"GET /test: {text}");
        await Task.Delay(_appSettings.Delay);
        if (Random.Shared.NextDouble() < _appSettings.ErrorFactor)
            return StatusCode(500);

        return Ok(text);
    }

    [HttpGet("version")]
    public async Task<IActionResult> Version()
    {
        Console.WriteLine($"GET /version");
        await Task.Delay(_appSettings.Delay);
        if (Random.Shared.NextDouble() < _appSettings.ErrorFactor)
            return StatusCode(500);

        var version = Assembly.GetExecutingAssembly().GetName().Version;
        return Ok($"{version?.Major}.{version?.Minor}.{version?.Build}");
    }
}
