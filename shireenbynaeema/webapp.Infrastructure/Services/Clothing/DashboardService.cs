namespace Infrastructure
{
using Application;
using Domain;
using Microsoft.EntityFrameworkCore;
using SharedServices;

public class DashboardService : IDashboardService
{
    private readonly DatabaseContext db;
    private readonly IResponse resp;

    public DashboardService(DatabaseContext db, IResponse response) { this.db = db; this.resp = response; }

    public async Task<IResponse> GetStats()
    {
        resp.IsSuccess = true;
        resp.Data = new DashboardStatsDto
        {
            TotalRevenue = await db.Orders.SumAsync(o => o.Total),
            TotalOrders = await db.Orders.CountAsync(),
            TotalCustomers = await db.Customers.CountAsync(),
            TotalProducts = await db.Products.CountAsync()
        };
        return resp;
    }
}
}