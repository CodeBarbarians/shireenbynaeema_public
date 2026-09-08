# Shireen by Naeema - Clothing E-Commerce Platform

Full-stack clothing e-commerce website with admin panel.

## Tech Stack

**Backend:** .NET 10, Entity Framework Core, SQL Server, SendGrid, Stripe
**Frontend:** React 19, TypeScript, Vite, Zustand, React Router, Tailwind-style CSS, Lucide Icons

## Project Structure

```
shireenbynaeema/          ← .NET Backend API
shireenbynaeema-admin/    ← React Frontend (Vite)
```

## Features

### Store
- Product browsing with category filters
- Product detail with size chart, variants, reviews
- Shopping cart with quantity management
- Checkout with Stripe payment
- User registration (email + Google SSO)
- Order history and account management

### Admin Panel
- Dashboard with revenue/order/customer stats
- Product management (CRUD + variants + image upload)
- Category management
- Order management with status updates
- Customer management
- Invoice generation and payment tracking
- Delivery tracking with carrier and tracking numbers
- Return request processing

### Backend
- Clean Architecture (Domain → Application → Infrastructure → Server)
- JWT authentication with role-based authorization
- Permission-based access control
- Image upload directly to server (no cloud dependency)
- Email notifications (order confirmation, invoice, delivery)

## Getting Started

### Backend
```bash
cd shireenbynaeema
dotnet restore
dotnet build
dotnet run --project webapp.Server
```

### Frontend
```bash
cd shireenbynaeema-admin
npm install
npm run dev
```

The frontend runs on `http://localhost:5173` and proxies API calls to `https://localhost:7211`.
