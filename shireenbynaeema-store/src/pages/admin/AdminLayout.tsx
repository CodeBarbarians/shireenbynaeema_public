import { Link, useLocation } from "react-router-dom";
import { useState } from "react";
import { LayoutDashboard, Package, ShoppingCart, Users, FileText, Truck, RotateCcw, Settings, ChevronLeft, Menu, X, FileEdit, Tag, BookOpen, BarChart3, Mail, Images } from "lucide-react";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import ThemeToggle from "@/components/layout/ThemeToggle";
import logo from "@/assets/logo.svg";
import { Store } from "lucide-react"

const navItems = [
  { label: "Dashboard", to: "/admin", icon: LayoutDashboard },
  { label: "Products", to: "/admin/products", icon: Package },
  { label: "Categories", to: "/admin/categories", icon: Package },
  { label: "Orders", to: "/admin/orders", icon: ShoppingCart },
  { label: "Customers", to: "/admin/customers", icon: Users },
  { label: "Invoices", to: "/admin/invoices", icon: FileText },
  { label: "Deliveries", to: "/admin/deliveries", icon: Truck },
  { label: "Returns", to: "/admin/returns", icon: RotateCcw },
  { label: "CMS Pages", to: "/admin/pages", icon: FileEdit },
  { label: "Coupons", to: "/admin/coupons", icon: Tag },
  { label: "Blog", to: "/admin/blog", icon: BookOpen },
  { label: "Reports", to: "/admin/reports", icon: BarChart3 },
  { label: "Email", to: "/admin/email", icon: Mail },
  { label: "Hero Slides", to: "/admin/hero", icon: Images },
  { label: "Settings", to: "/admin/settings", icon: Settings },
];

export default function AdminLayout({ children }: { children: React.ReactNode }) {
  const [collapsed, setCollapsed] = useState(false);
  const [mobileOpen, setMobileOpen] = useState(false);
  const location = useLocation();

  const renderNav = (isMobile: boolean) => (
    <nav className="p-2 space-y-1">
      {navItems.map((item) => {
        const Icon = item.icon;
        const isActive = location.pathname === item.to || (item.to !== "/admin" && location.pathname.startsWith(item.to));
        const showLabel = isMobile || !collapsed;
        return (
          <Link
            key={item.to}
            to={item.to}
            onClick={() => isMobile && setMobileOpen(false)}
            className={cn(
              "flex items-center gap-3 rounded-md transition-colors",
              showLabel ? "px-3 py-2.5 text-sm" : "justify-center px-0 py-2.5",
              isActive ? "bg-primary text-primary-foreground font-medium" : "text-muted-foreground hover:bg-accent hover:text-accent-foreground"
            )}
            title={!showLabel ? item.label : undefined}
          >
            <Icon size={18} strokeWidth={1.5} className="flex-shrink-0" />
            {showLabel && <span className="truncate">{item.label}</span>}
          </Link>
        );
      })}
    </nav>
  );

  const sidebarContent = (isMobile: boolean) => (
    <div className={cn(
      "bg-background border-r border-border flex-shrink-0 h-full flex flex-col transition-all duration-300",
      isMobile ? "w-64" : collapsed ? "w-[4.5rem]" : "w-60"
    )}>
      <div className={cn("flex items-center border-b border-border h-[4.05rem]", isMobile ? "justify-between px-4" : collapsed ? "justify-center" : "justify-between px-4")}>
        {isMobile || !collapsed ? (
          <Link to="/admin" onClick={() => isMobile && setMobileOpen(false)}>
            <img src={logo} alt="SBNE Admin" className="h-8 w-auto object-contain dark:invert" />
          </Link>
        ) : (
          <span />
        )}
        {isMobile ? (
          <Button variant="ghost" size="icon" onClick={() => setMobileOpen(false)} className="h-8 w-8">
            <X size={18} />
          </Button>
        ) : (
          <Button variant="ghost" size="icon" onClick={() => setCollapsed(!collapsed)} className="h-8 w-8 hidden lg:flex">
            <ChevronLeft className={cn("h-4 w-4 transition-transform", collapsed && "rotate-180")} />
          </Button>
        )}
      </div>
      <div className="flex-1 overflow-y-auto">
        {renderNav(isMobile)}
      </div>
    </div>
  );

  return (
    <div className="flex min-h-screen bg-muted/30">
      <div className="hidden lg:flex">{sidebarContent(false)}</div>

      {mobileOpen && (
        <>
          <div className="fixed inset-0 bg-black/40 z-40 lg:hidden" onClick={() => setMobileOpen(false)} />
          <div className="fixed inset-y-0 left-0 z-50 lg:hidden">
            {sidebarContent(true)}
          </div>
        </>
      )}

      <div className="flex-1 flex flex-col min-w-0">
        <header className="h-[4.05rem] bg-background border-b border-border flex items-center px-4 sm:px-6 justify-between">
          <div className="flex items-center gap-3">
            <Button variant="ghost" size="icon" onClick={() => setMobileOpen(!mobileOpen)} className="h-8 w-8 lg:hidden">
              {mobileOpen ? <X size={18} /> : <Menu size={18} />}
            </Button>
            <Link to="/" className="text-muted-foreground hover:text-foreground">
              <Store className="h-4 w-4" />
            </Link>
          </div>
          <div className="flex items-center gap-2 sm:gap-3">
            <ThemeToggle />
            <span className="text-sm text-muted-foreground hidden sm:inline">Admin</span>
            <Avatar className="h-7 w-7 sm:h-8 sm:w-8"><AvatarFallback className="text-xs">A</AvatarFallback></Avatar>
          </div>
        </header>
        <main className="flex-1 p-3 sm:p-6 overflow-x-auto">{children}</main>
      </div>
    </div>
  );
}
