namespace Infrastructure
{
using Application;
using Domain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using SharedServices;

public class CategoryService : Service<Category>, ICategoryService
{
    private readonly DatabaseContext db;
    private readonly IResponse resp;
    private readonly string uploadsRoot;

    public CategoryService(IRepository<Category> repository, IResponse response, DatabaseContext db, IWebHostEnvironment env)
        : base(repository, response)
    {
        this.db = db;
        this.resp = response;
        this.uploadsRoot = Path.Combine(env.WebRootPath, "uploads");
    }

    public async Task<IResponse> ListCategories(CategoryListRequest request)
    {
        var query = db.Categories.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(c => c.Name.Contains(request.Search));

        var totalCount = await query.CountAsync();
        var items = await query.OrderBy(c => c.SortOrder)
            .Skip(request.Skip).Take(request.Take)
            .Select(c => new Category_Listing
            {
                Id = c.Id, Name = c.Name, Slug = c.Slug, Description = c.Description,
                ParentId = c.ParentId, ImageUrl = c.ImageUrl, SortOrder = c.SortOrder,
                IsActive = c.IsActive, ShowInNav = c.ShowInNav, ProductCount = c.Products.Count
            }).ToListAsync();

        resp.IsSuccess = true;
        resp.Data = items.ToListResponse(request, totalCount);
        return resp;
    }

    public override async Task<IResponse> GetByIdAsync(Guid id)
    {
        var cat = await db.Categories.AsNoTracking()
            .Include(c => c.Images.OrderBy(i => i.SortOrder))
            .FirstOrDefaultAsync(c => c.Id == id);
        resp.IsSuccess = cat != null;
        resp.Data = cat;
        return resp;
    }

    public async Task<IResponse> GetBySlug(string slug)
    {
        var cat = await db.Categories.AsNoTracking()
            .Include(c => c.Images.OrderBy(i => i.SortOrder))
            .FirstOrDefaultAsync(c => c.Slug == slug);
        resp.IsSuccess = cat != null;
        resp.Data = cat;
        return resp;
    }

    public async Task<IResponse> GetTree()
    {
        var cats = await db.Categories.AsNoTracking()
            .Where(c => c.ParentId == null)
            .Include(c => c.Children.Where(ch => ch.IsActive))
            .Include(c => c.Images.OrderBy(i => i.SortOrder))
            .OrderBy(c => c.SortOrder).ToListAsync();

        resp.IsSuccess = true;
        resp.Data = cats;
        return resp;
    }

    public override async Task<IResponse> DeleteAsync(Guid id)
    {
        var category = await db.Categories.FindAsync(id);
        if (category == null)
        {
            resp.IsSuccess = false;
            resp.Message = "Category not found";
            return resp;
        }

        if (!string.IsNullOrEmpty(category.ImageUrl))
        {
            var relativePath = category.ImageUrl.Replace(category.ImageUrl.Split("/uploads/")[0], "").TrimStart('/');
            var fullPath = Path.Combine(uploadsRoot, relativePath);
            if (File.Exists(fullPath)) File.Delete(fullPath);
        }

        category.SoftDelete();
        await db.SaveChangesAsync();

        resp.IsSuccess = true;
        resp.Message = "Category deleted successfully";
        return resp;
    }
}
}