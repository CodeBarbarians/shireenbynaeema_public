namespace Infrastructure
{
using Application;
using Domain;
using Microsoft.EntityFrameworkCore;
using SharedServices;

public class CustomerService : ICustomerService
{
    private readonly DatabaseContext db;
    private readonly IResponse resp;

    public CustomerService(DatabaseContext db, IResponse response) { this.db = db; this.resp = response; }

    public async Task<IResponse> GetProfile(Guid userId)
    {
        var customer = await db.Customers.AsNoTracking().Include(c => c.User).Include(c => c.Addresses)
            .FirstOrDefaultAsync(c => c.UserId == userId);
        if (customer == null) { resp.IsSuccess = false; resp.Message = "Customer not found"; return resp; }

        resp.IsSuccess = true;
        resp.Data = MapCustomerDto(customer);
        return resp;
    }

    public async Task<IResponse> UpdateProfile(Guid userId, CustomerDto request)
    {
        var customer = await db.Customers.Include(c => c.User).FirstOrDefaultAsync(c => c.UserId == userId);
        if (customer == null) { resp.IsSuccess = false; resp.Message = "Customer not found"; return resp; }

        customer.Phone = request.Phone;
        customer.FirstName = request.FirstName;
        customer.LastName = request.LastName;
        if (customer.User != null)
        {
            customer.User.FirstName = request.FirstName;
            customer.User.LastName = request.LastName;
            customer.User.DisplayName = $"{request.FirstName} {request.LastName}";
        }

        await db.SaveChangesAsync();
        resp.IsSuccess = true;
        return resp;
    }

    public async Task<IResponse> AddAddress(Guid userId, AddressDto request)
    {
        var customer = await db.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
        if (customer == null) { resp.IsSuccess = false; resp.Message = "Customer not found"; return resp; }

        if (request.IsDefault)
        {
            var existing = await db.Addresses.Where(a => a.CustomerId == customer.Id && a.IsDefault).ToListAsync();
            foreach (var e in existing) e.IsDefault = false;
        }

        var address = new Address
        {
            Id = Guid.NewGuid(), CustomerId = customer.Id, Label = request.Label,
            Street = request.Street, City = request.City, State = request.State,
            ZipCode = request.ZipCode, Country = request.Country, IsDefault = request.IsDefault
        };

        db.Addresses.Add(address);
        await db.SaveChangesAsync();

        resp.IsSuccess = true;
        resp.Data = address.Id;
        return resp;
    }

    public async Task<IResponse> UpdateAddress(Guid userId, Guid addressId, AddressDto request)
    {
        var address = await db.Addresses.FindAsync(addressId);
        if (address == null) { resp.IsSuccess = false; resp.Message = "Address not found"; return resp; }

        address.Label = request.Label;
        address.Street = request.Street;
        address.City = request.City;
        address.State = request.State;
        address.ZipCode = request.ZipCode;
        address.Country = request.Country;
        address.IsDefault = request.IsDefault;

        await db.SaveChangesAsync();
        resp.IsSuccess = true;
        return resp;
    }

    public async Task<IResponse> ListCustomers(ListRequest request)
    {
        var query = db.Customers.AsNoTracking().Include(c => c.User).AsQueryable();
        var totalCount = await query.CountAsync();
        var items = await query.OrderByDescending(c => c.CreatedOn).Skip(request.Skip).Take(request.Take)
            .Select(c => new CustomerDto
            {
                Id = c.Id, UserId = c.UserId,
                FirstName = c.User != null ? c.User.FirstName : c.FirstName,
                LastName = c.User != null ? c.User.LastName : c.LastName,
                Email = c.User != null ? c.User.Email : c.Email,
                Phone = c.Phone, OrderCount = c.OrderCount, TotalSpent = c.TotalSpent, CreatedOn = c.CreatedOn
            }).ToListAsync();

        resp.IsSuccess = true;
        resp.Data = items.ToListResponse(request, totalCount);
        return resp;
    }

    public async Task<IResponse> GetCustomerDetail(Guid customerId)
    {
        var customer = await db.Customers.AsNoTracking()
            .Include(c => c.User)
            .Include(c => c.Addresses)
            .Include(c => c.Orders).ThenInclude(o => o.Items).ThenInclude(i => i.Variant).ThenInclude(v => v!.Product).ThenInclude(p => p!.Images)
            .FirstOrDefaultAsync(c => c.Id == customerId);
        if (customer == null) { resp.IsSuccess = false; resp.Message = "Customer not found"; return resp; }

        var orderIds = customer.Orders.Select(o => o.Id).ToList();
        var invoices = await db.Invoices.AsNoTracking()
            .Where(i => orderIds.Contains(i.OrderId))
            .Include(i => i.Order)
            .ToListAsync();

        var dto = MapCustomerDto(customer);
        dto.Orders = customer.Orders.OrderByDescending(o => o.CreatedOn).Select(o => new OrderDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            Status = o.Status,
            Subtotal = o.Subtotal,
            Tax = o.Tax,
            Shipping = o.Shipping,
            Discount = o.Discount,
            CouponCode = o.CouponCode ?? "",
            Total = o.Total,
            Notes = o.Notes,
            CreatedOn = o.CreatedOn,
            Items = o.Items.Select(i => new OrderItemDto
            {
                Id = i.Id,
                ProductName = i.Variant != null && i.Variant.Product != null ? i.Variant.Product.Name : "",
                Size = i.Variant != null ? i.Variant.Size : "",
                Color = i.Variant != null ? i.Variant.Color : "",
                ImageUrl = i.Variant != null && i.Variant.Product != null
                    ? i.Variant.Product.Images.OrderBy(img => img.SortOrder).Select(img => img.ImageUrl).FirstOrDefault() ?? "" : "",
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Total = i.Total
            }).ToList()
        }).ToList();
        dto.Invoices = invoices.Select(i => new InvoiceDto
        {
            Id = i.Id,
            InvoiceNumber = i.InvoiceNumber,
            OrderId = i.OrderId,
            OrderNumber = i.Order?.OrderNumber ?? "",
            Amount = i.Amount,
            Tax = i.Tax,
            Status = i.Status,
            DueDate = i.DueDate,
            PaidDate = i.PaidDate,
            CreatedOn = i.CreatedOn
        }).ToList();

        resp.IsSuccess = true;
        resp.Data = dto;
        return resp;
    }

    private static CustomerDto MapCustomerDto(Customer customer)
    {
        return new CustomerDto
        {
            Id = customer.Id,
            UserId = customer.UserId,
            FirstName = customer.User?.FirstName ?? customer.FirstName,
            LastName = customer.User?.LastName ?? customer.LastName,
            Email = customer.User?.Email ?? customer.Email,
            Phone = customer.Phone,
            Addresses = customer.Addresses.Select(a => new AddressDto
            {
                Id = a.Id, Label = a.Label, Street = a.Street, City = a.City,
                State = a.State, ZipCode = a.ZipCode, Country = a.Country, IsDefault = a.IsDefault
            }).ToList(),
            OrderCount = customer.OrderCount,
            TotalSpent = customer.TotalSpent,
            CreatedOn = customer.CreatedOn
        };
    }

    public async Task<IResponse> Delete(Guid customerId)
    {
        var customer = await db.Customers.FindAsync(customerId);
        if (customer == null) { resp.IsSuccess = false; resp.Message = "Customer not found"; return resp; }

        customer.SoftDelete();
        await db.SaveChangesAsync();

        resp.IsSuccess = true;
        resp.Message = "Customer deleted successfully";
        return resp;
    }

    public async Task<IResponse> DeleteAddress(Guid userId, Guid addressId)
    {
        var customer = await db.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
        if (customer == null) { resp.IsSuccess = false; resp.Message = "Customer not found"; return resp; }

        var address = await db.Addresses.FirstOrDefaultAsync(a => a.Id == addressId && a.CustomerId == customer.Id);
        if (address == null) { resp.IsSuccess = false; resp.Message = "Address not found"; return resp; }

        db.Addresses.Remove(address);
        await db.SaveChangesAsync();

        resp.IsSuccess = true;
        resp.Message = "Address deleted successfully";
        return resp;
    }
}
}
