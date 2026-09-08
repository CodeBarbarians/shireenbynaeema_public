namespace Infrastructure
{
using Application;
using Domain;
using Microsoft.EntityFrameworkCore;
using SharedServices;

public class SettingService : ISettingService
{
    private readonly DatabaseContext db;
    private readonly IResponse resp;

    public SettingService(DatabaseContext db, IResponse response) { this.db = db; this.resp = response; }

    public async Task<IResponse> GetAll()
    {
        var settings = await db.Set<Setting>().AsNoTracking().ToListAsync();
        resp.IsSuccess = true;
        resp.Data = settings.Select(s => new SettingDto
        {
            Id = s.Id, Key = s.Key, Value = s.Value, Description = s.Description
        }).ToList();
        return resp;
    }

    public async Task<IResponse> GetByKey(string key)
    {
        var setting = await db.Set<Setting>().AsNoTracking().FirstOrDefaultAsync(s => s.Key == key);
        resp.IsSuccess = setting != null;
        if (setting != null)
        {
            resp.Data = new SettingDto
            {
                Id = setting.Id,
                Key = setting.Key,
                Value = setting.Value,
                Description = setting.Description
            };
        }

        return resp;
    }

    public async Task<IResponse> Update(SettingDto request)
    {
        var setting = await db.Set<Setting>().FirstOrDefaultAsync(s => s.Key == request.Key);
        if (setting == null)
        {
            setting = new Setting
            {
                Id = Guid.NewGuid(),
                Key = request.Key,
                Value = request.Value,
                Description = request.Description,
                CreatedOn = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            db.Set<Setting>().Add(setting);
        }
        else
        {
            setting.Value = request.Value;
            setting.UpdatedOn = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        await db.SaveChangesAsync();
        resp.IsSuccess = true;
        resp.Data = new SettingDto { Id = setting.Id, Key = setting.Key, Value = setting.Value, Description = setting.Description };
        return resp;
    }

    public async Task<IResponse> BulkUpdate(List<SettingDto> request)
    {
        foreach (var item in request)
        {
            var setting = await db.Set<Setting>().FirstOrDefaultAsync(s => s.Key == item.Key);
            if (setting == null)
            {
                setting = new Setting
                {
                    Id = Guid.NewGuid(),
                    Key = item.Key,
                    Value = item.Value,
                    Description = item.Description,
                    CreatedOn = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                };
                db.Set<Setting>().Add(setting);
            }
            else
            {
                setting.Value = item.Value;
                setting.UpdatedOn = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }
        }

        await db.SaveChangesAsync();
        resp.IsSuccess = true;
        return resp;
    }
}
}
