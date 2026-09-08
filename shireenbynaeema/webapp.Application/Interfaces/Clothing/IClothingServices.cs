namespace Application
{
using Domain;
using Microsoft.AspNetCore.Http;
using SharedServices;

public interface ICategoryService : IService<Category>
{
    Task<IResponse> ListCategories(CategoryListRequest request);
    Task<IResponse> GetBySlug(string slug);
    Task<IResponse> GetTree();
}

public interface IProductService : IService<Product>
{
    Task<IResponse> ListProducts(ProductListRequest request);
    Task<IResponse> GetByCategory(string slug);
    Task<IResponse> GetFeatured();
    Task<IResponse> GetNewArrivals();
    Task<IResponse> Search(string query);
    Task<IResponse> AddProduct(Product_AddEdit request);
    Task<IResponse> UpdateProduct(Product_AddEdit request);
}

public interface ICartService
{
    Task<IResponse> GetCart(Guid userId);
    Task<IResponse> AddItem(Guid userId, CartItem_AddEdit request);
    Task<IResponse> UpdateQuantity(Guid userId, Guid cartItemId, CartItem_UpdateQuantity request);
    Task<IResponse> RemoveItem(Guid userId, Guid cartItemId);
    Task<IResponse> ClearCart(Guid userId);
}

public interface IOrderService
{
    Task<IResponse> PlaceOrder(Guid? userId, OrderPlaceRequest request);
    Task<IResponse> GetOrderHistory(Guid userId);
    Task<IResponse> GetOrderDetail(Guid? userId, Guid orderId);
    Task<IResponse> ListOrders(OrderListRequest request);
    Task<IResponse> UpdateStatus(Guid orderId, OrderStatusUpdateRequest request);
    Task<IResponse> Delete(Guid orderId);
}

public interface IInvoiceService
{
    Task<IResponse> ListInvoices(ListRequest request);
    Task<IResponse> GetById(Guid invoiceId);
    Task<IResponse> GetByOrder(Guid orderId);
    Task<IResponse> Generate(Guid orderId);
    Task<IResponse> MarkPaid(Guid invoiceId, string paymentId);
    Task<IResponse> Delete(Guid invoiceId);
}

public interface IDeliveryService
{
    Task<IResponse> ListDeliveries(ListRequest request);
    Task<IResponse> GetById(Guid deliveryId);
    Task<IResponse> Create(DeliveryCreateRequest request);
    Task<IResponse> UpdateTracking(Guid deliveryId, DeliveryUpdateRequest request);
    Task<IResponse> Delete(Guid deliveryId);
}

public interface IReturnService
{
    Task<IResponse> ListReturns(ListRequest request);
    Task<IResponse> GetById(Guid returnId);
    Task<IResponse> GetByUser(Guid userId);
    Task<IResponse> RequestReturn(Guid userId, ReturnRequestDto request);
    Task<IResponse> Process(Guid returnId, ReturnProcessDto request);
    Task<IResponse> Delete(Guid returnId);
}

public interface ICustomerService
{
    Task<IResponse> GetProfile(Guid userId);
    Task<IResponse> UpdateProfile(Guid userId, CustomerDto request);
    Task<IResponse> AddAddress(Guid userId, AddressDto request);
    Task<IResponse> UpdateAddress(Guid userId, Guid addressId, AddressDto request);
    Task<IResponse> ListCustomers(ListRequest request);
    Task<IResponse> GetCustomerDetail(Guid customerId);
    Task<IResponse> Delete(Guid customerId);
    Task<IResponse> DeleteAddress(Guid userId, Guid addressId);
}

public interface IRatingService
{
    Task<IResponse> GetByProduct(Guid productId);
    Task<IResponse> Create(Guid userId, ReviewCreateRequest request);
}

public interface IDashboardService
{
    Task<IResponse> GetStats();
}

public interface ICmsPageService : IService<CmsPage>
{
    Task<IResponse> ListPages(ListRequest request);
    Task<IResponse> GetBySlug(string slug);
}

public interface ISettingService
{
    Task<IResponse> GetAll();
    Task<IResponse> GetByKey(string key);
    Task<IResponse> Update(SettingDto request);
    Task<IResponse> BulkUpdate(List<SettingDto> request);
}

public interface IImageService
{
    Task<IResponse> Upload(IFormFile file, Guid? productId);
    Task<IResponse> UploadCategoryImage(IFormFile file, Guid categoryId);
    Task<IResponse> UploadReturnAttachment(IFormFile file);
    Task<IResponse> DeleteProductImage(Guid imageId);
    Task<IResponse> DeleteCategoryImage(Guid categoryId);
    Task<IResponse> UploadCategoryImages(IFormFile file, Guid categoryId);
    Task<IResponse> DeleteCategoryImageById(Guid imageId);
}

public interface IPaymentService
{
    Task<IResponse> CreatePaymentIntent(decimal amount, string currency, string orderId);
    Task<IResponse> GetPaymentIntent(string paymentIntentId);
    Task<IResponse> HandleWebhook(string json, string stripeSignature);
    Task<IResponse> ProcessRefund(string paymentIntentId, long amountInCents);
}

public interface ICouponService : IService<Coupon>
{
    Task<IResponse> ListCoupons(ListRequest request);
    Task<IResponse> Validate(string code, decimal orderTotal);
    Task<IResponse> Apply(Guid orderId, string code, decimal discount);
}

public interface IBlogPostService : IService<BlogPost>
{
    Task<IResponse> ListPublished(int skip, int take);
    Task<IResponse> GetBySlug(string slug);
    Task<IResponse> ListAll(ListRequest request);
}

public interface IReportService
{
    Task<IResponse> GetSalesReport(ReportFilterRequest filter);
    Task<IResponse> GetCategoryReport(ReportFilterRequest filter);
    Task<IResponse> GetProductReport(ReportFilterRequest filter);
}
}