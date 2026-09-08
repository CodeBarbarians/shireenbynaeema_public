namespace Infrastructure
{
using Application;
using Domain;
using Microsoft.EntityFrameworkCore;
using SharedServices;

public class CartService : ICartService
{
    private readonly DatabaseContext db;
    private readonly IResponse resp;

    public CartService(DatabaseContext db, IResponse response)
    {
        this.db = db;
        this.resp = response;
    }

    private async Task<Cart> GetOrCreateCart(Guid userId)
    {
        var cart = await db.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId);
        if (cart == null)
        {
            cart = new Cart { Id = Guid.NewGuid(), UserId = userId, CreatedOn = DateTimeOffset.UtcNow.ToUnixTimeSeconds() };
            db.Carts.Add(cart);
            await db.SaveChangesAsync();
        }
        return cart;
    }

    private async Task<object> MapCart(Cart cart)
    {
        var items = await db.CartItems.Where(ci => ci.CartId == cart.Id)
            .Include(ci => ci.Variant).ThenInclude(v => v!.Product).ThenInclude(p => p!.Images)
            .Select(ci => new
            {
                ci.Id,
                ci.ProductVariantId,
                ci.Quantity,
                ci.UnitPrice,
                variant = ci.Variant != null ? new
                {
                    ci.Variant.Id,
                    ci.Variant.ProductId,
                    ci.Variant.Size,
                    ci.Variant.Color,
                    ci.Variant.ColorHex,
                    ci.Variant.Stock,
                    ci.Variant.Price,
                    ci.Variant.SKU,
                }
                : null,
                product = ci.Variant != null && ci.Variant.Product != null ? new
                {
                    ci.Variant.Product.Id,
                    ci.Variant.Product.Name,
                    ci.Variant.Product.Slug,
                    images = ci.Variant.Product.Images.OrderBy(i => i.SortOrder).Select(i => new
                    {
                        i.Id,
                        i.ImageUrl,
                        i.AltText,
                        i.SortOrder,
                        i.IsPrimary,
                    }).ToList(),
                }
                : null,
            }).ToListAsync();

        return items;
    }

    public async Task<IResponse> GetCart(Guid userId)
    {
        var cart = await GetOrCreateCart(userId);
        resp.IsSuccess = true;
        resp.Data = await MapCart(cart);
        return resp;
    }

    public async Task<IResponse> AddItem(Guid userId, CartItem_AddEdit request)
    {
        var cart = await GetOrCreateCart(userId);
        var variant = await db.ProductVariants.FindAsync(request.ProductVariantId);
        if (variant == null) { resp.IsSuccess = false; resp.Message = "Variant not found"; return resp; }

        var existing = await db.CartItems.FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.ProductVariantId == request.ProductVariantId);
        if (existing != null)
        {
            existing.Quantity += request.Quantity;
        }
        else
        {
            db.CartItems.Add(new CartItem
            {
                Id = Guid.NewGuid(), CartId = cart.Id, ProductVariantId = request.ProductVariantId,
                Quantity = request.Quantity, UnitPrice = variant.Price
            });
        }

        await db.SaveChangesAsync();
        resp.IsSuccess = true;
        resp.Data = await MapCart(await GetOrCreateCart(userId));
        return resp;
    }

    public async Task<IResponse> UpdateQuantity(Guid userId, Guid cartItemId, CartItem_UpdateQuantity request)
    {
        var cart = await GetOrCreateCart(userId);
        var item = await db.CartItems.FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.CartId == cart.Id);
        if (item == null) { resp.IsSuccess = false; resp.Message = "Item not found"; return resp; }

        if (request.Quantity <= 0)
            db.CartItems.Remove(item);
        else
            item.Quantity = request.Quantity;

        await db.SaveChangesAsync();
        resp.IsSuccess = true;
        resp.Data = await MapCart(await GetOrCreateCart(userId));
        return resp;
    }

    public async Task<IResponse> RemoveItem(Guid userId, Guid cartItemId)
    {
        var cart = await GetOrCreateCart(userId);
        var item = await db.CartItems.FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.CartId == cart.Id);
        if (item != null)
        {
            db.CartItems.Remove(item);
            await db.SaveChangesAsync();
        }
        resp.IsSuccess = true;
        resp.Data = await MapCart(await GetOrCreateCart(userId));
        return resp;
    }

    public async Task<IResponse> ClearCart(Guid userId)
    {
        var cart = await GetOrCreateCart(userId);
        var items = await db.CartItems.Where(ci => ci.CartId == cart.Id).ToListAsync();
        db.CartItems.RemoveRange(items);
        await db.SaveChangesAsync();
        resp.IsSuccess = true;
        return resp;
    }
}
}