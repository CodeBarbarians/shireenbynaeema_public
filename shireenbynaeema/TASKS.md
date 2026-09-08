# Shireen by Naeema — Clothing Website Project Plan

## Project Overview

Full-stack clothing e-commerce website with admin panel, built on:
- **Backend**: .NET 10 (existing Clean Architecture boilerplate)
- **Frontend**: React + TypeScript + Vite (located at `D:\ClothingWebsite\shireenbynaeema-admin`)
- **Payment**: Stripe
- **Design Reference**: https://taara.store/

---

## Architecture

```
D:\ClothingWebsite\
├── shireenbynaeema/              ← .NET Backend (existing)
│   ├── webapp.SharedServices/    ← Constants, Enums, Extensions, DTOs
│   ├── webapp.Domain/            ← Entities, DTOs
│   ├── webapp.Application/       ← Interfaces (services, repositories)
│   ├── webapp.Infrastructure/    ← EF Core, Services, Repositories
│   ├── webapp.Server/            ← API Controllers, Startup, Program.cs
│   └── webapp.Tests/             ← Unit tests
│
└── shireenbynaeema-admin/        ← React Frontend (new)
    └── src/
        ├── components/           ← Reusable UI components
        ├── pages/store/          ← Public storefront pages
        ├── pages/admin/          ← Admin dashboard pages
        ├── services/             ← API client services
        ├── hooks/                ← Custom React hooks
        ├── store/                ← Zustand state management
        ├── types/                ← TypeScript type definitions
        └── utils/                ← Helper utilities
```

---

## Phase 1: Backend Cleanup (In Progress)

### 1.1 Remove FacilityNetwork References
| Task | Status |
|------|--------|
| Delete Sync entities (`Domain/Entities/Logging/Sync/`) | Done |
| Delete Sync DTOs (`Domain/Dtos/Logging/Sync/`) | Done |
| Delete Sync interfaces (`Application/Interfaces/Logging/Sync/`) | Done |
| Delete WorkOrder email templates (21 files) | Done |
| Rewrite `Constants.cs` — remove WorkOrder/Vendor/Dispatcher/Client/Property | Pending |
| Rewrite `Permissions.cs` — clothing website permissions | Pending |
| Update `appsettings.json` — CORS, URLs, branding | Pending |
| Update `appsettings.Development.json` — dev URLs | Pending |
| Update `Middleware.cs` — Swagger title | Pending |
| Update `webapp.Server.csproj` — remove deleted template references | Pending |
| Update `README.md` — new project description | Pending |
| Clean `Enums/` — remove FacilityNetwork-specific enums | Pending |

### 1.2 New Permission Structure
```
Store
├── Products.View
├── Cart.Manage
├── Orders.Place
└── Account.Manage

Admin
├── Dashboard.View
├── Products (View / Add / Update / Delete)
├── Categories (View / Add / Update / Delete)
├── Orders (View / Update / Cancel)
├── Customers (View / Update)
├── Invoices (View / Generate)
├── Deliveries (View / Update)
├── Returns (View / Process)
├── Ratings (View / Delete)
└── Settings (View / Update)

Auth
├── Users (View / Add / Update / Delete / Invite / ManageRoles / ManagePermissions)
├── Roles (View / Add / Update / Delete)
└── AuditLogs (View / Delete)
```

---

## Phase 2: Domain Entities (T2)

### Product Management
| Entity | Description |
|--------|-------------|
| `Product` | Main product (Name, Description, SKU, BasePrice, SalePrice, CategoryId, Brand, IsFeatured, IsActive, Rating, ReviewCount) |
| `ProductVariant` | Size/Color variants (ProductId, Size, Color, Stock, Price, SKU) |
| `ProductImage` | Product images uploaded to server (ProductId, ImageUrl, AltText, SortOrder, IsPrimary) |
| `Category` | Product categories (Name, Slug, Description, ParentId, ImageUrl, SortOrder, IsActive) |
| `SizeChart` | Size charts per category (CategoryId, SizeName, Measurements JSON) |
| `Rating` | Product ratings/reviews (ProductId, UserId, Score, Comment, CreatedOn) |

### Customer & Orders
| Entity | Description |
|--------|-------------|
| `Customer` | Customer profile (UserId, Phone, Addresses[]) |
| `Address` | Shipping/billing addresses (CustomerId, Street, City, State, Zip, Country, IsDefault) |
| `Cart` | Shopping cart (UserId, Items[]) |
| `CartItem` | Cart items (CartId, ProductVariantId, Quantity) |
| `Order` | Customer orders (CustomerId, OrderNumber, Status, Total, ShippingAddress, BillingAddress, Notes) |
| `OrderItem` | Order line items (OrderId, ProductVariantId, Quantity, UnitPrice, Total) |

### Fulfillment
| Entity | Description |
|--------|-------------|
| `Invoice` | Order invoices (OrderId, InvoiceNumber, Amount, Tax, Status, DueDate, PaidDate) |
| `Delivery` | Shipping/delivery tracking (OrderId, Carrier, TrackingNumber, Status, ShippedOn, DeliveredOn) |
| `Return` | Return requests (OrderId, Reason, Status, RefundAmount, RequestedOn, ProcessedOn) |

---

## Phase 3: Backend DTOs, Interfaces, Services (T3)

### For Each Entity:
- **Request DTOs**: `{Entity}_AddEdit`, `{Entity}ListRequest` (with filters, pagination, sorting)
- **Response DTOs**: `{Entity}_Listing`, `{Entity}_Lookup`, `{Entity}_Detail`
- **Interface**: `I{Entity}Service` extending `IService<T>`
- **Service**: `{Entity}Service` extending `Service<T>`
- **Repository**: Uses generic `Repository<T>` (already implemented)

### Custom Service Methods:
```
IProductService
├── GetByCategory(slug)
├── GetFeatured()
├── GetNewArrivals()
├── Search(query)
├── GetWithVariants(id)
└── UpdateStock(variantId, quantity)

IOrderService
├── PlaceOrder(cartId, shippingAddress, paymentIntentId)
├── UpdateStatus(orderId, status)
├── GetOrderHistory(customerId)
└── GetOrderDetail(orderId)

ICartService
├── GetCart(userId)
├── AddItem(userId, variantId, quantity)
├── UpdateQuantity(cartItemId, quantity)
├── RemoveItem(cartItemId)
└── ClearCart(userId)

IInvoiceService
├── Generate(orderId)
├── MarkPaid(invoiceId, paymentId)
└── GetByOrder(orderId)

IDeliveryService
├── Create(orderId, carrier, tracking)
├── UpdateTracking(deliveryId, status)
└── Track(trackingNumber)

IReturnService
├── Request(orderId, reason)
├── Process(returnId, status, refundAmount)
└── GetByOrder(orderId)
```

---

## Phase 4: Backend Controllers (T4)

### Store APIs (Public / Auth Required)
| Controller | Endpoints |
|------------|-----------|
| `ProductController` | `GET /api/Product/List`, `GET /api/Product/{id}`, `GET /api/Product/ByCategory/{slug}`, `GET /api/Product/Featured`, `GET /api/Product/Search` |
| `CartController` | `GET /api/Cart`, `POST /api/Cart/Add`, `PUT /api/Cart/Update`, `DELETE /api/Cart/Remove/{id}`, `DELETE /api/Cart/Clear` |
| `OrderController` | `POST /api/Order/Place`, `GET /api/Order/History`, `GET /api/Order/{id}` |
| `PaymentController` | `POST /api/Payment/CreateIntent`, `POST /api/Payment/Confirm`, `GET /api/Payment/Status/{id}` |
| `CustomerController` | `GET /api/Customer/Profile`, `PUT /api/Customer/Profile`, `POST /api/Customer/Address`, `PUT /api/Customer/Address/{id}` |
| `ReviewController` | `POST /api/Review`, `GET /api/Review/Product/{id}` |

### Admin APIs (Permission Required)
| Controller | Endpoints |
|------------|-----------|
| `AdminController/Product` | Full CRUD + image upload + variants management |
| `AdminController/Category` | Full CRUD + reorder |
| `AdminController/Order` | List, Update status, Cancel |
| `AdminController/Customer` | List, View detail |
| `AdminController/Invoice` | List, Generate, Mark paid |
| `AdminController/Delivery` | List, Create, Update tracking |
| `AdminController/Return` | List, Process (approve/reject/refund) |
| `AdminController/Dashboard` | Stats, recent orders, top products |
| `ImageController` | `POST /api/Image/Upload` (direct server upload to wwwroot/uploads/) |

### Image Upload (Direct to Server)
- Images saved to `wwwroot/uploads/products/{productId}/`
- Endpoint: `POST /api/Image/Upload` with `IFormFile`
- Returns: `{ url: "/uploads/products/{id}/filename.jpg" }`
- No cloud storage dependency

---

## Phase 5: React Storefront (T5)

### Pages
| Page | Route | Description |
|------|-------|-------------|
| Home | `/` | Hero banner, new arrivals, featured, categories, testimonials |
| Products | `/collections/:slug` | Product grid with filters (size, color, price, category) |
| Product Detail | `/products/:id` | Images, variants, size chart, reviews, add to cart |
| Cart | `/cart` | Cart items, quantity update, remove, subtotal |
| Checkout | `/checkout` | Shipping info, payment (Stripe), order summary |
| Account | `/account` | Profile, order history, addresses |
| Login | `/login` | Email/password + SSO (Google) |
| Register | `/register` | Registration form |
| Order Confirmation | `/order/:id` | Thank you page with order details |

### Reusable Components
| Component | Description |
|-----------|-------------|
| `Header` | Nav bar with logo, categories, search, cart icon, account |
| `Footer` | Links, social media, newsletter, payment icons |
| `ProductCard` | Image, name, price, hover effects, quick add |
| `ProductGrid` | Responsive grid of ProductCards |
| `SizeChart` | Modal with size chart table |
| `CartDrawer` | Slide-out cart summary |
| `SearchBar` | Live search with suggestions |
| `HeroSection` | Full-width banner with CTA |
| `CategoryCard` | Category image + name |
| `RatingStars` | Star rating display/input |
| `Breadcrumb` | Navigation breadcrumb |
| `QuantitySelector` | +/- quantity control |
| `PriceDisplay` | Original/sale price with discount badge |

### Design System (Matching taara.store)
- **Color Palette**: Clean whites, soft blacks, accent gold/blush
- **Typography**: Elegant serif headings, clean sans-serif body
- **Layout**: Full-width hero, centered content max-width 1200px
- **Product Cards**: Hover image swap, quick size selection
- **Animations**: Smooth transitions, fade-ins on scroll

---

## Phase 6: React Admin Panel (T6)

### Pages
| Page | Route | Description |
|------|-------|-------------|
| Dashboard | `/admin` | Revenue, orders, customers, top products stats |
| Products | `/admin/products` | Product list with search/filter |
| Product Form | `/admin/products/new`, `/admin/products/:id/edit` | Add/edit product with variants, images |
| Categories | `/admin/categories` | Category tree management |
| Orders | `/admin/orders` | Order list with status filters |
| Order Detail | `/admin/orders/:id` | Order detail, status update, timeline |
| Customers | `/admin/customers` | Customer list with search |
| Customer Detail | `/admin/customers/:id` | Customer profile, order history |
| Invoices | `/admin/invoices` | Invoice list, generate, mark paid |
| Deliveries | `/admin/deliveries` | Delivery tracking, update status |
| Returns | `/admin/returns` | Return requests, process approve/reject |
| Settings | `/admin/settings` | Store settings, payment config |
| Users | `/admin/users` | Admin user management |
| Roles | `/admin/roles` | Role and permission management |

### Admin Components
| Component | Description |
|-----------|-------------|
| `AdminLayout` | Sidebar + top bar + content area |
| `Sidebar` | Collapsible nav with icons |
| `DataTable` | Sortable, filterable, paginated table |
| `StatCard` | KPI card with icon and trend |
| `StatusBadge` | Colored status indicators |
| `ImageUploader` | Drag-drop image upload with preview |
| `RichTextEditor` | Product description editor |
| `Modal` | Reusable modal dialog |
| `Toast` | Success/error notifications |

---

## Phase 7: Stripe Integration (T7)

### Backend
- NuGet: `Stripe.net`
- Config: Stripe secret key in appsettings.json
- Endpoints:
  - `POST /api/Payment/CreateIntent` — creates PaymentIntent, returns clientSecret
  - `POST /api/Payment/Confirm` — confirms payment, creates order
  - Webhook endpoint for payment events

### Frontend
- Package: `@stripe/stripe-js`, `@stripe/react-stripe-js`
- `PaymentForm` component with Stripe Elements
- Checkout flow: Cart → Shipping → Payment → Confirmation

---

## Phase 8: Authentication (T8)

### Backend (Existing + Extended)
- JWT Bearer auth (already implemented)
- `POST /api/Auth/Register` — new customer registration
- `POST /api/Auth/Login` — email/password login
- `POST /api/Auth/LoginWithGoogle` — Google SSO
- Role-based: `Customer`, `Admin`, `SuperAdmin`

### Frontend
- `AuthContext` with Zustand
- Login/Register pages
- Google SSO button
- Protected routes (store vs admin)
- Token storage in localStorage

---

## Phase 9: Email Templates (T9)

### Existing (Kept)
| Template | Purpose |
|----------|---------|
| `SendForgetPassword.html` | Password reset link |
| `SendOtp.html` | OTP verification code |
| `InvitationEmailTemplate.html` | User invitation |
| `SendEmailToSystemAdmin.html` | Admin notification |

### New (Added)
| Template | Purpose | Placeholders |
|----------|---------|--------------|
| `OrderConfirmationEmailTemplate.html` | Order placed confirmation | `{0}` Invoice#, `{1}` CustomerName, `{2}` OrderURL, `{3}` Total, `{4}` Date, `{5}` OrderNumber, `{6}` OrderDate, `{7}` ItemCount, `{8}` ShippingAddress |
| `InvoiceEmailTemplate.html` | Invoice notification | `{0}` Invoice#, `{1}` CustomerName, `{2}` OrderURL, `{3}` Total, `{4}` Date, `{5}` OrderNumber, `{6}` Subtotal, `{7}` Tax, `{8}` Shipping, `{9}` StatusColor, `{10}` StatusText |
| `DeliveryEmailTemplate.html` | Shipping notification | `{0}` unused, `{1}` CustomerName, `{2}` unused, `{3}` unused, `{4}` unused, `{5}` OrderNumber, `{6}` Carrier, `{7}` TrackingNumber, `{8}` ShippedDate, `{9}` EstimatedDelivery, `{10}` Address, `{11}` TrackingURL |

### SendGrid From Keys
| Key | Email | Purpose |
|-----|-------|---------|
| `PwdReset` | security@shireenbynaeema.com | Password reset |
| `Invoices` | invoices@shireenbynaeema.com | Invoice emails |
| `Delivery` | shipping@shireenbynaeema.com | Delivery notifications |
| `Notifications` | notifications@shireenbynaeema.com | General notifications |

---

## File Cleanup Checklist

### Files to Delete
- [x] `webapp.Domain/Entities/Logging/Sync/` (3 files)
- [x] `webapp.Domain/Dtos/Logging/Sync/` (6 files)
- [x] `webapp.Application/Interfaces/Logging/Sync/` (2 files)
- [x] 21 FacilityNetwork email templates from `webapp.Server/Content/`
- [ ] `webapp.SharedServices/Enums/ClientAttachmentType.cs`
- [ ] `webapp.SharedServices/Enums/CoverLetterTemplate.cs`
- [ ] `webapp.SharedServices/Enums/DefaultBillingGroupType.cs`
- [ ] `webapp.SharedServices/Enums/InvoiceTemplate.cs`
- [ ] `webapp.SharedServices/Enums/JobOutcome.cs`
- [ ] `webapp.SharedServices/Enums/NTETaxType.cs`
- [ ] `webapp.SharedServices/Enums/PaymentTerms.cs`
- [ ] `webapp.SharedServices/Enums/RuleType.cs`

### Files to Rewrite
- [ ] `webapp.SharedServices/Constants/Constants.cs`
- [ ] `webapp.SharedServices/Constants/Permissions.cs`
- [x] `webapp.Server/appsettings.json` — SendGrid from emails updated
- [x] `webapp.Server/appsettings.Development.json` — SendGrid from emails updated
- [ ] `webapp.Server/Startup/Middleware.cs`
- [x] `webapp.Server/webapp.Server.csproj` — cleaned old templates, added new ones
- [ ] `README.md`

### Files to Add (Backend)
- [ ] Domain entities (14 entities)
- [ ] Domain DTOs (request/response for each entity)
- [ ] Application interfaces (service interfaces)
- [ ] Infrastructure services (service implementations)
- [ ] Server controllers (12+ controllers)
- [ ] EF Core configurations
- [ ] New migration

### Files to Add (Frontend)
- [ ] Project scaffolding (Vite + React + TS)
- [ ] 20+ reusable components
- [ ] 15+ store pages
- [ ] 12+ admin pages
- [ ] API service layer
- [ ] State management (Zustand)
- [ ] Type definitions
- [ ] Routing setup
- [ ] Authentication flow

---

## Task Tracker

| ID | Task | Status | Priority |
|----|------|--------|----------|
| T1 | Create React frontend project | In Progress | High |
| T2 | Add clothing domain entities | Open | High |
| T3 | Add backend DTOs, interfaces, services | Open | High |
| T4 | Add backend controllers | Open | High |
| T5 | Build React store layout | Open | High |
| T6 | Build React admin panel | Open | Medium |
| T7 | Add Stripe payment integration | Open | High |
| T8 | Add user auth (login, register, SSO) | Open | High |
| T9 | Backend cleanup (FacilityNetwork removal) | In Progress | High |
| T10 | EF Core migration | Open | Medium |
| T11 | API testing & integration | Open | Medium |
| T12 | Responsive design & polish | Open | Low |
| T13 | Email templates (Invoice, Delivery, Order Confirmation) | Done | High |
