import { Link, useNavigate } from "react-router-dom";
import { Search, ShoppingBag, User, Menu, X, Shield } from "lucide-react";
import { useState, useEffect, useRef } from "react";
import { useCartStore } from "@/store/cartStore";
import { useAuthStore } from "@/store/authStore";
import { categoryApi, productApi } from "@/services/api";
import type { Category, Product } from "@/types";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import ThemeToggle from "@/components/layout/ThemeToggle";
import { resolveImageUrl } from "@/lib/imageUtils";
import logo from "@/assets/logo.svg";

export default function Header() {
  const navigate = useNavigate();
  const [searchOpen, setSearchOpen] = useState(false);
  const [mobileOpen, setMobileOpen] = useState(false);
  const [categories, setCategories] = useState<Category[]>([]);
  const [searchQuery, setSearchQuery] = useState("");
  const [searchResults, setSearchResults] = useState<Product[]>([]);
  const [searchLoading, setSearchLoading] = useState(false);
  const searchRef = useRef<HTMLDivElement>(null);
  const { itemCount, toggleCart } = useCartStore();
  const { isAuthenticated, isAdmin } = useAuthStore();

  useEffect(() => {
    categoryApi.list().then((res) => setCategories(res.data.data?.entities || []));
  }, []);

  useEffect(() => {
    if (!searchQuery.trim()) { setSearchResults([]); return; }
    const timer = setTimeout(() => {
      setSearchLoading(true);
      productApi.search(searchQuery).then((res) => {
        setSearchResults(res.data.data?.entities || []);
      }).finally(() => setSearchLoading(false));
    }, 300);
    return () => clearTimeout(timer);
  }, [searchQuery]);

  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (searchRef.current && !searchRef.current.contains(e.target as Node)) {
        setSearchResults([]);
        setSearchOpen(false);
      }
    };
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  useEffect(() => {
    if (mobileOpen) document.body.style.overflow = "hidden";
    else document.body.style.overflow = "";
    return () => { document.body.style.overflow = ""; };
  }, [mobileOpen]);

  const navLinks = [
    { label: "NEW IN", to: "/collections/new-arrivals" },
    ...categories.filter((c) => c.showInNav).map((c) => ({ label: c.name, to: `/collections/${c.slug}` })),
    { label: "All Products", to: "/collections/all" },
  ];

  return (
    <header className="sticky top-0 z-50 bg-background border-b border-border" role="banner">
      <div className="max-w-[1400px] mx-auto px-3 sm:px-6 lg:px-8">
        <div className="relative flex items-center lg:justify-between h-14 lg:h-20">

          <button className="lg:hidden p-2 -ml-2 z-20 shrink-0" onClick={() => setMobileOpen(!mobileOpen)} aria-label={mobileOpen ? "Close menu" : "Open menu"} aria-expanded={mobileOpen}>
            {mobileOpen ? <X size={22} strokeWidth={1.5} /> : <Menu size={22} strokeWidth={1.5} />}
          </button>

          <Link to="/" className="absolute left-1/2 -translate-x-1/2 lg:static lg:translate-x-0 flex items-center justify-center gap-1.5 lg:justify-start lg:mr-8 min-w-0 z-10" aria-label="Shireen by Naeema - Home">
            <img src={logo} alt="Shireen by Naeema" className="h-7 sm:h-8 md:h-9 lg:h-12 w-auto object-contain dark:invert flex-shrink-0" />
            <span className="text-[11px] sm:text-xs lg:text-[10px] font-medium tracking-[2px] lg:tracking-[4px] uppercase text-muted-foreground/80 whitespace-nowrap hidden sm:block">Shireen by Naeema</span>
          </Link>

          <nav className="hidden lg:flex items-center flex-1 justify-center space-x-6 xl:space-x-8" aria-label="Main navigation">
            {navLinks.map((link) => (
              <Link key={link.to} to={link.to} className="text-xs font-medium tracking-[1.5px] uppercase text-muted-foreground hover:text-foreground transition-colors whitespace-nowrap">
                {link.label}
              </Link>
            ))}
          </nav>

          <div className="flex items-center space-x-0.5 sm:space-x-1 z-20 shrink-0 ml-auto">
            <div ref={searchRef} className="relative">
              <Button variant="ghost" size="icon" onClick={() => setSearchOpen(!searchOpen)} className="h-9 w-9 sm:h-8 sm:w-8">
                <Search size={18} strokeWidth={1.5} />
              </Button>
              {searchOpen && (
                <div className="absolute right-0 top-full mt-2 w-[calc(100vw-2rem)] sm:w-80 max-w-80 bg-background border border-border rounded-lg shadow-lg z-50 p-3">
                  <div className="relative">
                    <Search size={16} className="absolute left-2.5 top-1/2 -translate-y-1/2 text-muted-foreground" />
                    <Input
                      placeholder="Search products..."
                      value={searchQuery}
                      onChange={(e) => setSearchQuery(e.target.value)}
                      className="pl-9 h-9 text-sm"
                      autoFocus
                    />
                    {searchQuery && (
                      <button onClick={() => { setSearchQuery(""); setSearchResults([]); }} className="absolute right-2.5 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground">
                        <X size={14} />
                      </button>
                    )}
                  </div>
                  {searchResults.length > 0 && (
                    <div className="mt-2 max-h-64 overflow-y-auto space-y-1">
                      {searchResults.slice(0, 8).map((p) => (
                        <button
                          key={p.id}
                          onClick={() => { navigate(`/products/${p.id}`); setSearchQuery(""); setSearchResults([]); setSearchOpen(false); }}
                          className="flex items-center gap-3 w-full p-2 rounded-md hover:bg-accent transition-colors text-left"
                        >
                          {p.images?.[0] && <img src={resolveImageUrl(p.images[0].imageUrl)} alt={p.name} className="w-10 h-10 object-cover rounded" loading="lazy" />}
                          <div className="min-w-0">
                            <p className="text-sm font-medium truncate">{p.name}</p>
                            <p className="text-xs text-muted-foreground">Rs. {(p.salePrice && p.salePrice < p.basePrice ? p.salePrice : p.basePrice).toLocaleString()}</p>
                          </div>
                        </button>
                      ))}
                    </div>
                  )}
                  {searchQuery && !searchLoading && searchResults.length === 0 && (
                    <p className="text-xs text-muted-foreground text-center py-4">No products found</p>
                  )}
                </div>
              )}
            </div>

            <ThemeToggle />

            {isAdmin && (
              <Link to="/admin">
                <Button variant="ghost" size="icon" className="h-9 w-9 sm:h-8 sm:w-8" title="Admin Panel">
                  <Shield size={18} strokeWidth={1.5} />
                </Button>
              </Link>
            )}

            <Link to={isAuthenticated ? "/account" : "/login"}>
              <Button variant="ghost" size="icon" className="h-9 w-9 sm:h-8 sm:w-8">
                <User size={18} strokeWidth={1.5} />
              </Button>
            </Link>

            <Button variant="ghost" size="icon" onClick={toggleCart} className="relative h-9 w-9 sm:h-8 sm:w-8">
              <ShoppingBag size={18} strokeWidth={1.5} />
              {itemCount > 0 && (
                <span className="absolute -top-0.5 -right-0.5 bg-primary text-primary-foreground text-[9px] w-4 h-4 rounded-full flex items-center justify-center">
                  {itemCount}
                </span>
              )}
            </Button>
          </div>
        </div>
      </div>

      {mobileOpen && (
        <div className="lg:hidden fixed inset-0 top-14 z-40 bg-background/80 backdrop-blur-sm" onClick={() => setMobileOpen(false)}>
          <nav className="bg-background border-t border-border px-4 py-4 space-y-1 shadow-lg" onClick={(e) => e.stopPropagation()} aria-label="Mobile navigation">
            {navLinks.map((link) => (
              <Link key={link.to} to={link.to} onClick={() => setMobileOpen(false)} className="block text-sm font-medium tracking-[1px] uppercase text-muted-foreground hover:text-foreground py-3 px-3 rounded-md hover:bg-accent transition-colors">
                {link.label}
              </Link>
            ))}
            {isAdmin && (
              <Link to="/admin" onClick={() => setMobileOpen(false)} className="flex items-center gap-2 text-sm font-medium tracking-[1px] uppercase text-muted-foreground hover:text-foreground py-3 px-3 rounded-md hover:bg-accent transition-colors">
                <Shield size={16} strokeWidth={1.5} />
                Admin Panel
              </Link>
            )}
          </nav>
        </div>
      )}
    </header>
  );
}
