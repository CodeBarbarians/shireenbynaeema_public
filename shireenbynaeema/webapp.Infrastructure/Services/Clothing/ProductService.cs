namespace Infrastructure
{
using Application;
using Domain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using SharedServices;

public class ProductService : Service<Product>, IProductService
{
    private readonly DatabaseContext db;
    private readonly IResponse resp;
    private readonly string uploadsRoot;

    public ProductService(IRepository<Product> repository, IResponse response, DatabaseContext db, IWebHostEnvironment env)
        : base(repository, response)
    {
        this.db = db;
        this.resp = response;
        this.uploadsRoot = Path.Combine(env.WebRootPath, "uploads");
    }

    private static Product_Listing ProjectToDto(Product p)
    {
        return new Product_Listing
        {
            Id = p.Id,
            Name = p.Name,
            Slug = p.Slug,
            Description = p.Description,
            SKU = p.SKU,
            BasePrice = p.BasePrice,
            SalePrice = p.SalePrice,
            CategoryId = p.CategoryId,
            CategoryName = p.Category != null ? p.Category.Name : "",
            Brand = p.Brand,
            IsFeatured = p.IsFeatured,
            IsActive = p.IsActive,
            Rating = p.Rating,
            ReviewCount = p.ReviewCount,
            CreatedOn = p.CreatedOn,
            Variants = p.Variants.Select(v => new VariantDto
            {
                Id = v.Id,
                Size = v.Size,
                Color = v.Color,
                ColorHex = v.ColorHex,
                Stock = v.Stock,
                Price = v.Price,
                SKU = v.SKU,
            }).ToList(),
            Images = p.Images.Select(i => new ProductImageDto
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl,
                AltText = i.AltText,
                SortOrder = i.SortOrder,
                IsPrimary = i.IsPrimary,
            }).ToList(),
            SizeChartJson = p.SizeChartJson,
        };
    }

    private IQueryable<Product> GetBaseQuery()
    {
        return db.Products.AsNoTracking()
            .Include(p => p.Variants)
            .Include(p => p.Images.OrderBy(i => i.SortOrder))
            .Include(p => p.Category);
    }

    public async Task<IResponse> ListProducts(ProductListRequest request)
    {
        var query = GetBaseQuery();

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(p => p.Name.Contains(request.Search) || p.SKU.Contains(request.Search));
        if (request.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);
        if (request.IsFeatured.HasValue)
            query = query.Where(p => p.IsFeatured == request.IsFeatured.Value);
        if (request.MinPrice.HasValue)
            query = query.Where(p => p.BasePrice >= request.MinPrice.Value);
        if (request.MaxPrice.HasValue)
            query = query.Where(p => p.BasePrice <= request.MaxPrice.Value);

        var totalCount = await query.CountAsync();
        var items = await query.OrderByDescending(p => p.CreatedOn).Skip(request.Skip).Take(request.Take).ToListAsync();
        var mapped = items.Select(ProjectToDto).ToList();

        resp.IsSuccess = true;
        resp.Data = mapped.ToListResponse(request, totalCount);
        return resp;
    }

    public override async Task<IResponse> GetByIdAsync(Guid id)
    {
        var product = await GetBaseQuery().FirstOrDefaultAsync(p => p.Id == id);
        if (product == null)
        {
            resp.IsSuccess = false;
            resp.Message = "Product not found";
            return resp;
        }
        resp.IsSuccess = true;
        resp.Data = ProjectToDto(product);
        return resp;
    }

    public async Task<IResponse> GetByCategory(string slug)
    {
        var cat = await db.Categories.FirstOrDefaultAsync(c => c.Slug == slug);
        if (cat == null) { resp.IsSuccess = false; resp.Message = "Category not found"; return resp; }

        var items = await GetBaseQuery().Where(p => p.CategoryId == cat.Id && p.IsActive).ToListAsync();
        resp.IsSuccess = true;
        resp.Data = new { entities = items.Select(ProjectToDto).ToList(), totalCount = items.Count };
        return resp;
    }

    public async Task<IResponse> GetFeatured()
    {
        var items = await GetBaseQuery().Where(p => p.IsFeatured && p.IsActive).OrderByDescending(p => p.CreatedOn).Take(8).ToListAsync();
        resp.IsSuccess = true;
        resp.Data = new { entities = items.Select(ProjectToDto).ToList(), totalCount = items.Count };
        return resp;
    }

    public async Task<IResponse> GetNewArrivals()
    {
        var items = await GetBaseQuery().Where(p => p.IsActive).OrderByDescending(p => p.CreatedOn).Take(8).ToListAsync();
        resp.IsSuccess = true;
        resp.Data = new { entities = items.Select(ProjectToDto).ToList(), totalCount = items.Count };
        return resp;
    }

    public async Task<IResponse> Search(string query)
    {
        var items = await GetBaseQuery()
            .Where(p => p.IsActive && (p.Name.Contains(query) || p.Brand.Contains(query) || p.Category!.Name.Contains(query)))
            .ToListAsync();
        resp.IsSuccess = true;
        resp.Data = new { entities = items.Select(ProjectToDto).ToList(), totalCount = items.Count };
        return resp;
    }

    public async Task<IResponse> AddProduct(Product_AddEdit request)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(), Name = request.Name, Slug = request.Slug,
            Description = request.Description, SKU = request.SKU,
            BasePrice = request.BasePrice, SalePrice = request.SalePrice,
            CategoryId = request.CategoryId, Brand = request.Brand,
            IsFeatured = request.IsFeatured, IsActive = request.IsActive,
            SizeChartJson = request.SizeChartJson,
            CreatedOn = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        if (request.Variants?.Any() == true)
        {
            foreach (var v in request.Variants)
            {
                product.Variants.Add(new ProductVariant
                {
                    Id = Guid.NewGuid(), ProductId = product.Id, Size = v.Size,
                    Color = v.Color, ColorHex = v.ColorHex, Stock = v.Stock,
                    Price = v.Price, SKU = v.SKU
                });
            }
        }

        db.Products.Add(product);
        await db.SaveChangesAsync();

        resp.IsSuccess = true;
        resp.Data = product.Id;
        return resp;
    }

    public async Task<IResponse> UpdateProduct(Product_AddEdit request)
    {
        var product = await db.Products.Include(p => p.Variants).FirstOrDefaultAsync(p => p.Id == request.Id);
        if (product == null) { resp.IsSuccess = false; resp.Message = "Product not found"; return resp; }

        product.Name = request.Name;
        product.Slug = request.Slug;
        product.Description = request.Description;
        product.SKU = request.SKU;
        product.BasePrice = request.BasePrice;
        product.SalePrice = request.SalePrice;
        product.CategoryId = request.CategoryId;
        product.Brand = request.Brand;
        product.IsFeatured = request.IsFeatured;
        product.IsActive = request.IsActive;
        product.SizeChartJson = request.SizeChartJson;
        product.UpdatedOn = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        if (request.Variants != null)
        {
            var existingVariants = product.Variants.ToDictionary(v => v.Id);
            db.ProductVariants.RemoveRange(product.Variants);
            foreach (var v in request.Variants)
            {
                var bookedStock = existingVariants.TryGetValue(v.Id ?? Guid.NewGuid(), out var existing) ? existing.BookedStock : 0;
                product.Variants.Add(new ProductVariant
                {
                    Id = v.Id ?? Guid.NewGuid(), ProductId = product.Id, Size = v.Size,
                    Color = v.Color, ColorHex = v.ColorHex, Stock = v.Stock, BookedStock = bookedStock,
                    Price = v.Price, SKU = v.SKU
                });
            }
        }

        await db.SaveChangesAsync();
        resp.IsSuccess = true;
        return resp;
    }

    public override async Task<IResponse> DeleteAsync(Guid id)
    {
        var product = await db.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
        if (product == null)
        {
            resp.IsSuccess = false;
            resp.Message = "Product not found";
            return resp;
        }

        foreach (var image in product.Images)
        {
            if (!string.IsNullOrEmpty(image.ImageUrl))
            {
                var relativePath = image.ImageUrl.Replace(image.ImageUrl.Split("/uploads/")[0], "").TrimStart('/');
                var fullPath = Path.Combine(uploadsRoot, relativePath);
                if (File.Exists(fullPath)) File.Delete(fullPath);
            }
        }

        product.SoftDelete();
        await db.SaveChangesAsync();

        resp.IsSuccess = true;
        resp.Message = "Product deleted successfully";
        return resp;
    }
}
}