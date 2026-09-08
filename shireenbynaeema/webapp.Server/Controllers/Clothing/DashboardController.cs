namespace Server
{
using Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService dashboardService;

    public DashboardController(IDashboardService dashboardService) { this.dashboardService = dashboardService; }

    [HttpGet("Stats")]
    public async Task<ActionResult> GetStats()
    {
        var response = await dashboardService.GetStats();
        return Ok(response);
    }
}
}