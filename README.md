# Shireen by Naeema

Full-stack e-commerce platform for a women's western clothing brand — storefront,
checkout, order management and an admin back office.

**Stack** — ASP.NET Core Web API - React + TypeScript + Vite - PostgreSQL +
EF Core - Stripe - Azure Blob Storage

## What it does

**Storefront**
- Product catalogue with categories and size/colour variants, stock tracked per variant
- Live search, SEO metadata, structured data and sitemap generation
- Guest and authenticated carts
- Customer accounts with order history
- CMS-driven pages (about, FAQ, shipping, size guide) and a blog
- Product reviews

**Checkout and fulfilment**
- Stripe, cash on delivery and bank transfer
- Coupon and discount handling
- Invoice generation (QuestPDF)
- Delivery tracking
- Returns and refund processing

**Admin**
- Dashboard with sales reporting
- CRUD across products, categories, coupons and CMS content
- Image upload with WebP compression (SixLabors.ImageSharp)
- Soft delete across master data, so nothing is destroyed by an admin mis-click
- Role-based access, activity log, exception log

## Layout

```
shireenbynaeema/          ASP.NET Core, clean-architecture projects
  webapp.Server/                controllers, DI, configuration
  webapp.Application/           use cases
  webapp.Domain/Clothing/       catalogue, orders, cart entities
  webapp.Infrastructure/        EF Core (Npgsql), Stripe, blob storage
  webapp.Tests/                 MSTest
shireenbynaeema-store/    React storefront (Zustand, Stripe.js, shadcn)
PROPOSAL.md               scope, current feature status and roadmap
```

## Running it

```bash
# API
cd shireenbynaeema
dotnet ef database update --project webapp.Infrastructure --startup-project webapp.Server
dotnet run --project webapp.Server

# storefront
cd shireenbynaeema-store
npm install && npm run dev
```

## Configuration

No real config is committed. Start from the examples:

```bash
cp shireenbynaeema/webapp.Server/appsettings.Example.json \
   shireenbynaeema/webapp.Server/appsettings.json
cp shireenbynaeema-store/.env.example shireenbynaeema-store/.env
```

The API needs a PostgreSQL connection string, a JWT signing secret, an
encryption key, Stripe secret and webhook secrets, a SendGrid API key,
reCAPTCHA keys and an Azure Blob connection string. Use user secrets locally:

```bash
cd shireenbynaeema/webapp.Server
dotnet user-secrets set "Stripe:SecretKey" "<your test key>"
```

The storefront needs only the Stripe **publishable** key and the API base URL.
Use Stripe test keys for development; `stripe listen --forward-to` is the
easiest way to exercise webhooks locally.

## Notes

Background work runs on Hangfire with the PostgreSQL storage provider, so order
emails and invoice generation don't block the checkout response — a customer
should never wait on SendGrid to see a confirmation page.

Product images are converted to WebP on upload rather than at request time.
Most of the catalogue is photography and the brand's audience is largely on
mobile data, so the conversion cost is paid once at upload instead of on every
page view.

Master data is soft-deleted throughout. Orders reference products, and a hard
delete would leave historical invoices pointing at nothing.
