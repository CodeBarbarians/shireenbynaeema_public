import { useEffect, useState, useCallback, useMemo } from "react";
import { useParams } from "react-router-dom";
import { productApi, categoryApi } from "@/services/api";
import type { Product, Category } from "@/types";
import ProductCard from "@/components/product/ProductCard";
import { Skeleton } from "@/components/ui/skeleton";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { SlidersHorizontal, X } from "lucide-react";
import SEO from "@/components/seo/SEO";

type SortOption = "default" | "price-asc" | "price-desc" | "newest" | "name-asc" | "name-desc";

export default function ProductList() {
  const { slug } = useParams();
  const [allProducts, setAllProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [minPrice, setMinPrice] = useState("");
  const [maxPrice, setMaxPrice] = useState("");
  const [selectedCategoryId, setSelectedCategoryId] = useState<string>("all");
  const [selectedSize, setSelectedSize] = useState<string>("all");
  const [sortBy, setSortBy] = useState<SortOption>("default");
  const [filtersOpen, setFiltersOpen] = useState(false);
  const [categories, setCategories] = useState<Category[]>([]);

  const allSizes = ["XS", "S", "M", "L", "XL", "XXL"];

  useEffect(() => { categoryApi.list().then((res) => setCategories(res.data.data?.entities || [])); }, []);

  const fetchProducts = useCallback(async () => {
    setLoading(true);
    try {
      let res;
      if (slug === "all") {
        const params: Record<string, unknown> = {};
        if (selectedCategoryId && selectedCategoryId !== "all") params.categoryId = selectedCategoryId;
        if (minPrice) params.minPrice = parseFloat(minPrice);
        if (maxPrice) params.maxPrice = parseFloat(maxPrice);
        res = await productApi.list(params);
      } else if (slug === "new-arrivals") {
        res = await productApi.newArrivals();
      } else {
        res = await productApi.getByCategory(slug || "");
      }
      setAllProducts(res.data.data?.entities || []);
    } finally {
      setLoading(false);
    }
  }, [slug, selectedCategoryId, minPrice, maxPrice]);

  useEffect(() => { fetchProducts(); }, [fetchProducts]);

  const products = useMemo(() => {
    let filtered = [...allProducts];

    if (selectedSize !== "all") {
      filtered = filtered.filter((p) => p.variants?.some((v) => v.size === selectedSize));
    }

    if (minPrice) {
      filtered = filtered.filter((p) => (p.salePrice || p.basePrice) >= parseFloat(minPrice));
    }
    if (maxPrice) {
      filtered = filtered.filter((p) => (p.salePrice || p.basePrice) <= parseFloat(maxPrice));
    }

    switch (sortBy) {
      case "price-asc":
        filtered.sort((a, b) => (a.salePrice || a.basePrice) - (b.salePrice || b.basePrice));
        break;
      case "price-desc":
        filtered.sort((a, b) => (b.salePrice || b.basePrice) - (a.salePrice || a.basePrice));
        break;
      case "newest":
        filtered.sort((a, b) => (b.createdOn || 0) - (a.createdOn || 0));
        break;
      case "name-asc":
        filtered.sort((a, b) => a.name.localeCompare(b.name));
        break;
      case "name-desc":
        filtered.sort((a, b) => b.name.localeCompare(a.name));
        break;
    }

    return filtered;
  }, [allProducts, selectedSize, minPrice, maxPrice, sortBy]);

  const clearFilters = () => {
    setMinPrice("");
    setMaxPrice("");
    setSelectedCategoryId("all");
    setSelectedSize("all");
    setSortBy("default");
  };

  const hasActiveFilters = selectedSize !== "all" || minPrice || maxPrice || sortBy !== "default" || (slug === "all" && selectedCategoryId !== "all");

  const title = slug === "all" ? "All Products" : slug === "new-arrivals" ? "New Arrivals" : slug?.replace(/-/g, " ") || "Products";

  return (
    <div className="max-w-[1400px] mx-auto px-4 sm:px-6 lg:px-8 py-8 lg:py-12">
      <SEO
        title={title}
        description={`Shop ${title} at Shireen by Naeema. ${products.length} products available with free nationwide delivery across Pakistan.`}
        url={`https://shireenbynaeema.com/collections/${slug}`}
        breadcrumbs={[
          { name: "Home", url: "/" },
          { name: title, url: `/collections/${slug}` },
        ]}
      />
      {/* Header */}
      <div className="text-center mb-8">
        <h1 className="text-2xl sm:text-3xl font-light tracking-wide capitalize" style={{ fontFamily: "Georgia, serif" }}>
          {title}
        </h1>
        <p className="text-sm text-muted-foreground mt-2">{products.length} product{products.length !== 1 ? "s" : ""}</p>
      </div>

      {/* Toolbar: Sort + Filter toggle */}
      <div className="flex items-center justify-between mb-6">
        <div className="flex items-center gap-3">
          <Select value={sortBy} onValueChange={(v) => setSortBy(v as SortOption)}>
            <SelectTrigger className="w-[180px] h-9 text-sm">
              <SelectValue placeholder="Sort by" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="default">Featured</SelectItem>
              <SelectItem value="price-asc">Price: Low to High</SelectItem>
              <SelectItem value="price-desc">Price: High to Low</SelectItem>
              <SelectItem value="newest">Newest</SelectItem>
              <SelectItem value="name-asc">Name: A to Z</SelectItem>
              <SelectItem value="name-desc">Name: Z to A</SelectItem>
            </SelectContent>
          </Select>
        </div>

        <Button variant="outline" size="sm" className="h-9 gap-2" onClick={() => setFiltersOpen(!filtersOpen)}>
          <SlidersHorizontal size={14} />
          Filters
          {hasActiveFilters && (
            <span className="w-5 h-5 bg-foreground text-background text-[10px] rounded-full flex items-center justify-center font-medium">
              {[selectedSize !== "all", !!minPrice, !!maxPrice, slug === "all" && selectedCategoryId !== "all"].filter(Boolean).length}
            </span>
          )}
        </Button>
      </div>

      {/* Filter Panel */}
      {filtersOpen && (
        <div className="mb-6 bg-muted/50 rounded-lg p-4 border border-border">
          <div className="flex flex-wrap items-end gap-3">
            {/* Category filter - only on "All Products" */}
            {slug === "all" && (
              <div className="w-full sm:w-auto sm:flex-1 sm:min-w-[160px]">
                <label className="text-[10px] font-medium tracking-wider uppercase block mb-1">Category</label>
                <Select value={selectedCategoryId} onValueChange={setSelectedCategoryId}>
                  <SelectTrigger className="w-full"><SelectValue placeholder="All" /></SelectTrigger>
                  <SelectContent>
                    <SelectItem value="all">All Categories</SelectItem>
                    {categories.map((c) => <SelectItem key={c.id} value={c.id}>{c.name}</SelectItem>)}
                  </SelectContent>
                </Select>
              </div>
            )}

            {/* Size */}
            <div className="w-full sm:w-auto sm:min-w-[100px]">
              <label className="text-[10px] font-medium tracking-wider uppercase block mb-1">Size</label>
              <Select value={selectedSize} onValueChange={setSelectedSize}>
                <SelectTrigger className="w-full"><SelectValue placeholder="All" /></SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Sizes</SelectItem>
                  {allSizes.map((s) => <SelectItem key={s} value={s}>{s}</SelectItem>)}
                </SelectContent>
              </Select>
            </div>

            {/* Price Range */}
            <div className="w-1/2 sm:w-auto sm:min-w-[110px]">
              <label className="text-[10px] font-medium tracking-wider uppercase block mb-1">Min Price</label>
              <Input type="number" placeholder="0" value={minPrice} onChange={(e) => setMinPrice(e.target.value)} className="h-9 text-sm" />
            </div>
            <div className="w-1/2 sm:w-auto sm:min-w-[110px]">
              <label className="text-[10px] font-medium tracking-wider uppercase block mb-1">Max Price</label>
              <Input type="number" placeholder="Any" value={maxPrice} onChange={(e) => setMaxPrice(e.target.value)} className="h-9 text-sm" />
            </div>

            <Button variant="ghost" size="sm" onClick={clearFilters} className="h-9 text-sm gap-1">
              <X size={12} /> Clear
            </Button>
          </div>
        </div>
      )}

      {/* Active filter pills */}
      {hasActiveFilters && !filtersOpen && (
        <div className="flex flex-wrap gap-2 mb-6">
          {selectedSize !== "all" && (
            <span className="inline-flex items-center gap-1 px-3 py-1 bg-muted rounded-full text-xs">
              Size: {selectedSize}
              <button onClick={() => setSelectedSize("all")} className="ml-0.5 hover:text-foreground"><X size={12} /></button>
            </span>
          )}
          {minPrice && (
            <span className="inline-flex items-center gap-1 px-3 py-1 bg-muted rounded-full text-xs">
              Min: Rs. {minPrice}
              <button onClick={() => setMinPrice("")} className="ml-0.5 hover:text-foreground"><X size={12} /></button>
            </span>
          )}
          {maxPrice && (
            <span className="inline-flex items-center gap-1 px-3 py-1 bg-muted rounded-full text-xs">
              Max: Rs. {maxPrice}
              <button onClick={() => setMaxPrice("")} className="ml-0.5 hover:text-foreground"><X size={12} /></button>
            </span>
          )}
          {slug === "all" && selectedCategoryId !== "all" && (
            <span className="inline-flex items-center gap-1 px-3 py-1 bg-muted rounded-full text-xs">
              Category: {categories.find((c) => c.id === selectedCategoryId)?.name}
              <button onClick={() => setSelectedCategoryId("all")} className="ml-0.5 hover:text-foreground"><X size={12} /></button>
            </span>
          )}
          <button onClick={clearFilters} className="text-xs underline text-muted-foreground hover:text-foreground">Clear all</button>
        </div>
      )}

      {/* Product Grid */}
      {loading ? (
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-3 lg:gap-6">
          {Array.from({ length: 8 }).map((_, i) => (
            <div key={i} className="space-y-3">
              <Skeleton className="aspect-[3/4] w-full" />
              <Skeleton className="h-4 w-3/4" />
              <Skeleton className="h-3 w-1/2" />
            </div>
          ))}
        </div>
      ) : products.length === 0 ? (
        <div className="text-center py-16">
          <p className="text-muted-foreground mb-4">No products found.</p>
          {hasActiveFilters && (
            <Button variant="outline" size="sm" onClick={clearFilters}>Clear filters</Button>
          )}
        </div>
      ) : (
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-3 lg:gap-6">
          {products.map((product) => (
            <ProductCard key={product.id} product={product} />
          ))}
        </div>
      )}
    </div>
  );
}
