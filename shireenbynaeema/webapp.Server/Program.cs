using Infrastructure;

using NLog;
using NLog.Web;

using Server;

var builder = WebApplication.CreateBuilder(args);

// Load configuration
builder.Configuration
       .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
       .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
       .AddEnvironmentVariables();

// ✅ NOW initialize NLog
var logger = LogManager.Setup()
    .LoadConfigurationFromAppSettings()
    .GetCurrentClassLogger();

builder.Logging.ClearProviders();
builder.Host.UseNLog();

// Register services
builder.Services.AddAppConfigurations(builder.Configuration);
builder.Services.AddApplicationDependencies();
builder.Services.AddApplicationServices();

if (string.IsNullOrEmpty(builder.Environment.WebRootPath))
    builder.Environment.WebRootPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
Directory.CreateDirectory(builder.Environment.WebRootPath);

// Build app
var app = builder.Build();

// Middleware pipeline
app.ConfigureRequestPipeline();

// Current user
using (var scope = app.Services.CreateScope())
{
    var httpContextAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
    var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

    CurrentUser.Configure(httpContextAccessor, dbContext);

    // Seed CMS pages
    if (!dbContext.CmsPages.Any())
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        dbContext.CmsPages.AddRange(
            new Domain.CmsPage { Id = Guid.NewGuid(), Slug = "contact", Title = "Contact Us", Content = "We'd love to hear from you! Whether you have a question about orders, sizing, or anything else, our team is ready to help.\n\nEmail: support@shireenbynaeema.com\nPhone: +92 300 1234567\nWhatsApp: +92 300 1234567\n\nBusiness Hours:\nMonday - Saturday: 10:00 AM - 7:00 PM\nSunday: Closed\n\nSocial Media:\nInstagram: @shireenbynaeema\nFacebook: Shireen by Naeema", IsActive = true, CreatedOn = now },
            new Domain.CmsPage { Id = Guid.NewGuid(), Slug = "shipping", Title = "Shipping & Returns", Content = "Shipping Policy:\n- Free nationwide delivery on all orders\n- Karachi delivery: 2-3 working days\n- Other cities: 3-4 working days\n- Cash on Delivery available across Pakistan\n\nExchange Policy:\n- 14-day exchange policy on all orders\n- Item must be unworn, unwashed, and in original packaging\n- Exchange is available for the same article in a different size\n- To initiate an exchange, contact us via WhatsApp or email\n\nReturn Policy:\n- We do not offer refunds, only exchanges within 14 days\n- Sale items are final sale and cannot be exchanged", IsActive = true, CreatedOn = now },
            new Domain.CmsPage { Id = Guid.NewGuid(), Slug = "size-guide", Title = "Size Guide", Content = "Our sizes are designed to fit the following body measurements:\n\nXS (Extra Small): Bust 30-31\", Waist 23-24\", Hips 33-34\"\nS (Small): Bust 32-33\", Waist 25-26\", Hips 35-36\"\nM (Medium): Bust 34-35\", Waist 27-28\", Hips 37-38\"\nL (Large): Bust 36-37\", Waist 29-30\", Hips 39-40\"\nXL (Extra Large): Bust 38-39\", Waist 31-32\", Hips 41-42\"\nXXL: Bust 40-41\", Waist 33-34\", Hips 43-44\"\n\nHow to Measure:\n- Bust: Measure around the fullest part of your chest\n- Waist: Measure around your natural waistline\n- Hips: Measure around the fullest part of your hips\n\nIf you are between sizes, we recommend sizing up for a more comfortable fit.", IsActive = true, CreatedOn = now },
            new Domain.CmsPage { Id = Guid.NewGuid(), Slug = "faq", Title = "Frequently Asked Questions", Content = "Q: How long does delivery take?\nA: Karachi orders are delivered in 2-3 working days. Orders for other cities take 3-4 working days.\n\nQ: Do you offer Cash on Delivery?\nA: Yes, COD is available across Pakistan.\n\nQ: Can I exchange my order?\nA: Yes, we offer a 14-day exchange policy. Items must be unworn and in original packaging.\n\nQ: How do I track my order?\nA: Once your order is dispatched, you will receive a tracking number via SMS and WhatsApp.\n\nQ: What payment methods do you accept?\nA: We accept Cash on Delivery, bank transfers, and credit/debit cards via Stripe.\n\nQ: Do you ship internationally?\nA: Currently, we only ship within Pakistan.\n\nQ: How do I find my size?\nA: Please refer to our Size Guide for detailed measurements. You can also contact us on WhatsApp for personalized sizing advice.", IsActive = true, CreatedOn = now },
            new Domain.CmsPage { Id = Guid.NewGuid(), Slug = "about", Title = "About Us", Content = "Shireen by Naeema is a premium women's clothing brand that celebrates elegance, femininity, and timeless style. We design pieces that make every entrance unforgettable.\n\nOur Story:\nFounded with a passion for creating beautiful, high-quality clothing, Shireen by Naeema has grown into a beloved brand trusted by women across Pakistan. Every piece in our collection is thoughtfully designed with attention to detail, quality fabrics, and flattering silhouettes.\n\nOur Values:\n- Quality: We use premium fabrics and meticulous craftsmanship\n- Elegance: Our designs blend timeless beauty with modern trends\n- Inclusivity: We offer a wide range of sizes to celebrate every body\n- Sustainability: We are committed to responsible fashion practices", IsActive = true, CreatedOn = now },
            new Domain.CmsPage { Id = Guid.NewGuid(), Slug = "privacy", Title = "Privacy Policy", Content = "Last Updated: January 2026\n\nInformation We Collect:\n- Name, email address, phone number\n- Shipping and billing addresses\n- Payment information (processed securely via Stripe)\n- Browsing and purchase history\n\nHow We Use Your Information:\n- To process and fulfill your orders\n- To communicate about your orders and promotions\n- To improve our products and services\n- To provide customer support\n\nData Security:\nWe implement industry-standard security measures to protect your personal information. Payment data is encrypted and processed through Stripe's secure servers.\n\nThird-Party Services:\nWe use Stripe for payment processing, and analytics tools to improve our website. These services may collect information as governed by their own privacy policies.\n\nYour Rights:\nYou may request access to, correction, or deletion of your personal data at any time by contacting us at support@shireenbynaeema.com.", IsActive = true, CreatedOn = now },
            new Domain.CmsPage { Id = Guid.NewGuid(), Slug = "terms", Title = "Terms of Service", Content = "Last Updated: January 2026\n\nAcceptance of Terms:\nBy accessing and using the Shireen by Naeema website, you agree to be bound by these Terms of Service.\n\nProducts and Pricing:\n- All prices are in PKR and include applicable taxes\n- We reserve the right to change prices without prior notice\n- Product colors may vary slightly from what is shown on screen\n\nOrders:\n- An order is confirmed only after successful payment or COD verification\n- We reserve the right to cancel orders due to stock unavailability\n\nIntellectual Property:\nAll content on this website, including images, text, and designs, is the property of Shireen by Naeema and may not be reproduced without permission.\n\nLimitation of Liability:\nShireen by Naeema shall not be liable for any indirect, incidental, or consequential damages arising from the use of our products or services.\n\nGoverning Law:\nThese terms are governed by the laws of Pakistan.", IsActive = true, CreatedOn = now });
        dbContext.SaveChanges();
    }
}

app.Run();