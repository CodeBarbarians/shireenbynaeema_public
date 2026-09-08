import { BrowserRouter, Routes, Route } from "react-router-dom";
import { useEffect } from "react";
import { HelmetProvider } from "react-helmet-async";
import { Toaster } from "react-hot-toast";
import { initTheme } from "@/store/themeStore";
import Header from "@/components/layout/Header";
import Footer from "@/components/layout/Footer";
import CartDrawer from "@/components/cart/CartDrawer";
import ProtectedRoute from "@/components/ProtectedRoute";
import { useAuthStore } from "@/store/authStore";
import Home from "@/pages/store/Home";
import ProductList from "@/pages/store/ProductList";
import ProductDetail from "@/pages/store/ProductDetail";
import Cart from "@/pages/store/Cart";
import Checkout from "@/pages/store/Checkout";
import Login from "@/pages/store/Login";
import Register from "@/pages/store/Register";
import Account from "@/pages/store/Account";
import OrderHistory from "@/pages/store/OrderHistory";
import RequestReturn from "@/pages/store/RequestReturn";
import MyReturns from "@/pages/store/MyReturns";
import AdminLayout from "@/pages/admin/AdminLayout";
import Dashboard from "@/pages/admin/Dashboard";
import AdminProducts from "@/pages/admin/Products";
import ProductForm from "@/pages/admin/ProductForm";
import AdminCategories from "@/pages/admin/Categories";
import AdminOrders from "@/pages/admin/Orders";
import AdminOrderDetail from "@/pages/admin/OrderDetail";
import AdminCustomers from "@/pages/admin/Customers";
import AdminCustomerDetail from "@/pages/admin/CustomerDetail";
import AdminInvoices from "@/pages/admin/Invoices";
import AdminInvoiceDetail from "@/pages/admin/InvoiceDetail";
import AdminDeliveries from "@/pages/admin/Deliveries";
import AdminDeliveryDetail from "@/pages/admin/DeliveryDetail";
import AdminReturns from "@/pages/admin/Returns";
import AdminReturnDetail from "@/pages/admin/ReturnDetail";
import AdminCmsPages from "@/pages/admin/CmsPages";
import AdminCoupons from "@/pages/admin/Coupons";
import AdminBlogPosts from "@/pages/admin/BlogPosts";
import AdminReports from "@/pages/admin/Reports";
import AdminSmtpSettings from "@/pages/admin/SmtpSettings";
import AdminHeroSlides from "@/pages/admin/HeroSlides";
import AdminSettings from "@/pages/admin/Settings";
import CmsPage from "@/pages/store/CmsPage";
import Blog from "@/pages/store/Blog";
import BlogPost from "@/pages/store/BlogPost";

function StoreLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="min-h-screen flex flex-col">
      <Header />
      <main className="flex-1">{children}</main>
      <Footer />
    </div>
  );
}

export default function App() {
  const loadUser = useAuthStore((s) => s.loadUser);
  useEffect(() => { loadUser(); initTheme(); }, [loadUser]);

  return (
    <HelmetProvider>
    <BrowserRouter>
      <Toaster position="top-center" toastOptions={{ duration: 3000, style: { fontSize: "14px" } }} />
      <CartDrawer />
      <Routes>
        <Route path="/" element={<StoreLayout><Home /></StoreLayout>} />
        <Route path="/collections/:slug" element={<StoreLayout><ProductList /></StoreLayout>} />
        <Route path="/products/:id" element={<StoreLayout><ProductDetail /></StoreLayout>} />
        <Route path="/cart" element={<StoreLayout><Cart /></StoreLayout>} />
        <Route path="/checkout" element={<StoreLayout><Checkout /></StoreLayout>} />
        <Route path="/login" element={<StoreLayout><Login /></StoreLayout>} />
        <Route path="/register" element={<StoreLayout><Register /></StoreLayout>} />
        <Route path="/account" element={<ProtectedRoute><StoreLayout><Account /></StoreLayout></ProtectedRoute>} />
        <Route path="/account/orders" element={<ProtectedRoute><StoreLayout><OrderHistory /></StoreLayout></ProtectedRoute>} />
        <Route path="/account/returns" element={<ProtectedRoute><StoreLayout><MyReturns /></StoreLayout></ProtectedRoute>} />
        <Route path="/account/returns/:orderId" element={<ProtectedRoute><StoreLayout><RequestReturn /></StoreLayout></ProtectedRoute>} />

        <Route path="/contact" element={<StoreLayout><CmsPage /></StoreLayout>} />
        <Route path="/shipping" element={<StoreLayout><CmsPage /></StoreLayout>} />
        <Route path="/size-guide" element={<StoreLayout><CmsPage /></StoreLayout>} />
        <Route path="/faq" element={<StoreLayout><CmsPage /></StoreLayout>} />
        <Route path="/about" element={<StoreLayout><CmsPage /></StoreLayout>} />
        <Route path="/privacy" element={<StoreLayout><CmsPage /></StoreLayout>} />
        <Route path="/terms" element={<StoreLayout><CmsPage /></StoreLayout>} />
        <Route path="/blog" element={<StoreLayout><Blog /></StoreLayout>} />
        <Route path="/blog/:slug" element={<StoreLayout><BlogPost /></StoreLayout>} />

        <Route path="/admin" element={<ProtectedRoute adminOnly><AdminLayout><Dashboard /></AdminLayout></ProtectedRoute>} />
        <Route path="/admin/products" element={<ProtectedRoute adminOnly><AdminLayout><AdminProducts /></AdminLayout></ProtectedRoute>} />
        <Route path="/admin/products/new" element={<ProtectedRoute adminOnly><AdminLayout><ProductForm /></AdminLayout></ProtectedRoute>} />
        <Route path="/admin/products/:id/edit" element={<ProtectedRoute adminOnly><AdminLayout><ProductForm /></AdminLayout></ProtectedRoute>} />
        <Route path="/admin/categories" element={<ProtectedRoute adminOnly><AdminLayout><AdminCategories /></AdminLayout></ProtectedRoute>} />
        <Route path="/admin/orders" element={<ProtectedRoute adminOnly><AdminLayout><AdminOrders /></AdminLayout></ProtectedRoute>} />
        <Route path="/admin/orders/:id" element={<ProtectedRoute adminOnly><AdminLayout><AdminOrderDetail /></AdminLayout></ProtectedRoute>} />
        <Route path="/admin/customers" element={<ProtectedRoute adminOnly><AdminLayout><AdminCustomers /></AdminLayout></ProtectedRoute>} />
        <Route path="/admin/customers/:id" element={<ProtectedRoute adminOnly><AdminLayout><AdminCustomerDetail /></AdminLayout></ProtectedRoute>} />
        <Route path="/admin/invoices" element={<ProtectedRoute adminOnly><AdminLayout><AdminInvoices /></AdminLayout></ProtectedRoute>} />
        <Route path="/admin/invoices/:id" element={<ProtectedRoute adminOnly><AdminLayout><AdminInvoiceDetail /></AdminLayout></ProtectedRoute>} />
        <Route path="/admin/deliveries" element={<ProtectedRoute adminOnly><AdminLayout><AdminDeliveries /></AdminLayout></ProtectedRoute>} />
        <Route path="/admin/deliveries/:id" element={<ProtectedRoute adminOnly><AdminLayout><AdminDeliveryDetail /></AdminLayout></ProtectedRoute>} />
        <Route path="/admin/returns" element={<ProtectedRoute adminOnly><AdminLayout><AdminReturns /></AdminLayout></ProtectedRoute>} />
        <Route path="/admin/returns/:id" element={<ProtectedRoute adminOnly><AdminLayout><AdminReturnDetail /></AdminLayout></ProtectedRoute>} />
        <Route path="/admin/pages" element={<ProtectedRoute adminOnly><AdminLayout><AdminCmsPages /></AdminLayout></ProtectedRoute>} />
        <Route path="/admin/coupons" element={<ProtectedRoute adminOnly><AdminLayout><AdminCoupons /></AdminLayout></ProtectedRoute>} />
        <Route path="/admin/blog" element={<ProtectedRoute adminOnly><AdminLayout><AdminBlogPosts /></AdminLayout></ProtectedRoute>} />
        <Route path="/admin/reports" element={<ProtectedRoute adminOnly><AdminLayout><AdminReports /></AdminLayout></ProtectedRoute>} />
        <Route path="/admin/email" element={<ProtectedRoute adminOnly><AdminLayout><AdminSmtpSettings /></AdminLayout></ProtectedRoute>} />
        <Route path="/admin/hero" element={<ProtectedRoute adminOnly><AdminLayout><AdminHeroSlides /></AdminLayout></ProtectedRoute>} />
        <Route path="/admin/settings" element={<ProtectedRoute adminOnly><AdminLayout><AdminSettings /></AdminLayout></ProtectedRoute>} />
      </Routes>
    </BrowserRouter>
    </HelmetProvider>
  );
}
