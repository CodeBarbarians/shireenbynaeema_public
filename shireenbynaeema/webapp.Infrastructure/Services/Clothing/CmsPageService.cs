namespace Infrastructure
{
using Application;
using Domain;
using Microsoft.EntityFrameworkCore;
using SharedServices;

public class CmsPageService : Service<CmsPage>, ICmsPageService
{
    private readonly DatabaseContext db;
    private readonly IResponse resp;

    public CmsPageService(IRepository<CmsPage> repository, IResponse response, DatabaseContext db)
        : base(repository, response)
    {
        this.db = db;
        this.resp = response;
    }

    public async Task<IResponse> ListPages(ListRequest request)
    {
        var query = db.CmsPages.AsNoTracking().AsQueryable();
        var totalCount = await query.CountAsync();
        var items = await query.OrderByDescending(p => p.CreatedOn)
            .Skip(request.Skip).Take(request.Take)
            .Select(p => new CmsPage_Listing
            {
                Id = p.Id, Slug = p.Slug, Title = p.Title, Content = p.Content,
                IsActive = p.IsActive, CreatedOn = p.CreatedOn
            }).ToListAsync();

        resp.IsSuccess = true;
        resp.Data = items.ToListResponse(request, totalCount);
        return resp;
    }

    public async Task<IResponse> GetBySlug(string slug)
    {
        var page = await db.CmsPages.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive);

        if (page == null)
        {
            resp.IsSuccess = false;
            resp.Message = "Page not found";
            return resp;
        }

        resp.IsSuccess = true;
        resp.Data = new { page.Slug, page.Title, page.Content };
        return resp;
    }
}
}
