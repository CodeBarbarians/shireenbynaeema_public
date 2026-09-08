import { useEffect, useState, useRef } from "react";
import { useParams } from "react-router-dom";
import { productApi, reviewApi } from "@/services/api";
import { useCartStore } from "@/store/cartStore";
import type { Product, ProductVariant, Review as ReviewType, SizeChartEntry } from "@/types";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { Star, Truck, RefreshCw, Shield } from "lucide-react";
import toast from "react-hot-toast";
import SEO from "@/components/seo/SEO";
import { resolveImageUrl } from "@/lib/imageUtils";

export default function ProductDetail() {
  const { id } = useParams();
  const [product, setProduct] = useState<Product | null>(null);
  const [reviews, setReviews] = useState<ReviewType[]>([]);
  const [selectedVariant, setSelectedVariant] = useState<ProductVariant | null>(null);
  const [selectedImage, setSelectedImage] = useState(0);
  const [selectedSize, setSelectedSize] = useState("");
  const [selectedColor, setSelectedColor] = useState("");
  const [quantity, setQuantity] = useState(1);
  const [sizeChartOpen, setSizeChartOpen] = useState(false);
  const [sizeChart, setSizeChart] = useState<SizeChartEntry[]>([]);
  const sizeChartRef = useRef<HTMLDivElement>(null);
  const addItem = useCartStore((s) => s.addItem);

  useEffect(() => {
    if (!id) return;
    productApi.getById(id).then((res) => {
      const p = res.data.data || res.data;
      setProduct(p);
      try { setSizeChart(JSON.parse(p.sizeChartJson || "[]")); } catch { setSizeChart([]); }
      if (p.variants?.length > 0) {
        setSelectedSize(p.variants[0].size);
        setSelectedColor(p.variants[0].color);
      }
    });
    reviewApi.getByProduct(id).then((res) => setReviews(res.data.data || []));
  }, [id]);

  useEffect(() => {
    if (product?.variants) {
      const v = product.variants.find((v) => v.size === selectedSize && v.color === selectedColor);
      setSelectedVariant(v || null);
    }
  }, [selectedSize, selectedColor, product]);

  const handleAddToCart = async () => {
    if (!selectedVariant) { toast.error("Please select a size and color"); return; }
    if (selectedVariant.stock <= 0) { toast.error("This item is out of stock"); return; }
    await addItem(selectedVariant, product!, quantity);
    toast.success("Added to cart");
  };

  if (!product) {
    return (
      <div className="max-w-[1400px] mx-auto px-4 py-16">
        <div className="animate-pulse grid grid-cols-1 lg:grid-cols-2 gap-8">
          <Skeleton className="aspect-square" />
          <div className="space-y-4"><Skeleton className="h-8 w-3/4" /><Skeleton className="h-4 w-1/2" /><Skeleton className="h-20" /></div>
        </div>
      </div>
    );
  }

  const sizes = [...new Set(product.variants?.map((v) => v.size) || [])];
  const colors = [...new Set(product.variants?.filter((v) => v.size === selectedSize).map((v) => v.color) || [])];
  const displayPrice = product.salePrice && product.salePrice < product.basePrice ? product.salePrice : product.basePrice;
  const hasDiscount = product.salePrice != null && product.salePrice < product.basePrice;

  return (
    <div className="max-w-[1400px] mx-auto px-4 sm:px-6 lg:px-8 py-8 lg:py-12">
      <SEO
        title={product.name}
        description={product.description || `${product.name} - Shop at Shireen by Naeema. ${product.categoryName ? `Category: ${product.categoryName}. ` : ""}Free nationwide delivery.`}
        image={resolveImageUrl(product.images?.[0]?.imageUrl)}
        url={`https://shireenbynaeema.com/products/${product.id}`}
        type="product"
        product={{
          name: product.name,
          description: product.description || product.name,
          image: resolveImageUrl(product.images?.[0]?.imageUrl) || "",
          price: String(displayPrice),
          currency: "PKR",
          availability: selectedVariant && selectedVariant.stock > 0 ? "InStock" : "OutOfStock",
          brand: product.brand || "Shireen by Naeema",
          rating: product.rating || undefined,
          reviewCount: product.reviewCount || undefined,
        }}
        breadcrumbs={[
          { name: "Home", url: "/" },
          { name: "Products", url: "/collections/all" },
          ...(product.categoryName ? [{ name: product.categoryName, url: `/collections/all` }] : []),
          { name: product.name, url: `/products/${product.id}` },
        ]}
      />
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8 lg:gap-12">
        <div>
          <div className="aspect-[3/4] bg-muted mb-3 overflow-hidden">
            {product.images?.[selectedImage] && (
              <img src={resolveImageUrl(product.images[selectedImage].imageUrl)} alt={product.name} className="w-full h-full object-cover" />
            )}
          </div>
          {product.images && product.images.length > 1 && (
            <div className="grid grid-cols-4 gap-2">
              {product.images.map((img, i) => (
                <button key={img.id} onClick={() => setSelectedImage(i)} className={`aspect-square bg-muted overflow-hidden border-2 ${selectedImage === i ? "border-foreground" : "border-transparent"}`}>
                  <img src={resolveImageUrl(img.imageUrl)} alt={img.altText} className="w-full h-full object-cover" />
                </button>
              ))}
            </div>
          )}
        </div>

        <div className="lg:sticky lg:top-24 lg:self-start">
          <h1 className="text-2xl lg:text-3xl font-light mb-2" style={{ fontFamily: "Georgia, serif" }}>{product.name}</h1>
          <div className="flex items-center space-x-3 mb-4">
            <span className="text-lg font-medium">Rs. {displayPrice.toLocaleString()} PKR</span>
            {hasDiscount && <span className="text-lg text-muted-foreground line-through">Rs. {product.basePrice.toLocaleString()} PKR</span>}
            {hasDiscount && <span className="text-xs font-medium bg-destructive/10 text-destructive px-2 py-0.5 rounded">SALE</span>}
          </div>
          <div className="flex items-center space-x-1 mb-6">
            {Array.from({ length: 5 }).map((_, i) => <Star key={i} size={14} className={i < Math.floor(product.rating) ? "fill-amber-400 text-amber-400" : "text-gray-300"} />)}
            <span className="text-sm text-muted-foreground ml-2">({product.reviewCount} reviews)</span>
          </div>
          <p className="text-sm text-muted-foreground leading-relaxed mb-6">{product.description}</p>

          {sizes.length > 0 && (
            <div className="mb-4">
              <div className="flex items-center justify-between mb-2">
                <span className="text-xs font-medium tracking-wider uppercase">Size: <span className="text-foreground">{selectedSize}</span></span>
                <button onClick={() => { setSizeChartOpen(true); setTimeout(() => sizeChartRef.current?.scrollIntoView({ behavior: "smooth", block: "start" }), 100); }} className="text-xs underline text-muted-foreground hover:text-foreground transition-colors">Size Chart</button>
              </div>
              <div className="flex flex-wrap gap-2">
                {sizes.map((size) => {
                  const sizeVariants = product.variants?.filter((v) => v.size === size) || [];
                  const totalStock = sizeVariants.reduce((sum, v) => sum + v.stock, 0);
                  return (
                    <Button key={size} variant={selectedSize === size ? "default" : "outline"} size="sm" onClick={() => setSelectedSize(size)} className="min-w-[44px]" disabled={totalStock <= 0}>
                      {size}
                    </Button>
                  );
                })}
              </div>
            </div>
          )}

          {colors.length > 0 && (
            <div className="mb-4">
              <span className="text-xs font-medium tracking-wider uppercase block mb-2">Color: <span className="text-foreground">{selectedColor}</span></span>
              <div className="flex gap-2">
                {colors.map((color) => (
                  <button key={color} onClick={() => setSelectedColor(color)} className={`w-8 h-8 rounded-full border-2 ${selectedColor === color ? "border-foreground" : "border-border"}`} style={{ backgroundColor: product.variants?.find((v) => v.color === color)?.colorHex || color }} />
                ))}
              </div>
            </div>
          )}

          {selectedVariant && (
            <p className={`text-xs mb-4 ${selectedVariant.stock > 0 ? "text-green-600" : "text-destructive"}`}>
              {selectedVariant.stock > 0 ? `${selectedVariant.stock} in stock` : "Out of stock"}
            </p>
          )}

          <div className="flex items-center space-x-3 mb-6">
            <div className="flex border border-border">
              <Button variant="ghost" size="icon" className="h-10 w-10" onClick={() => setQuantity(Math.max(1, quantity - 1))}>-</Button>
              <span className="px-4 py-2 text-sm border-x border-border">{quantity}</span>
              <Button variant="ghost" size="icon" className="h-10 w-10" onClick={() => setQuantity(quantity + 1)}>+</Button>
            </div>
          </div>

          <Button onClick={handleAddToCart} disabled={!selectedVariant || selectedVariant.stock <= 0} className="w-full" size="lg">
            {!selectedVariant ? "Select Size & Color" : selectedVariant.stock <= 0 ? "Out of Stock" : "Add to Cart"}
          </Button>

          <div className="grid grid-cols-3 gap-4 mt-6 py-4 border-t border-border">
            <div className="flex flex-col items-center text-center gap-1.5">
              <Truck size={18} className="text-muted-foreground" />
              <span className="text-[10px] text-muted-foreground leading-tight">Free Delivery</span>
            </div>
            <div className="flex flex-col items-center text-center gap-1.5">
              <RefreshCw size={18} className="text-muted-foreground" />
              <span className="text-[10px] text-muted-foreground leading-tight">14-Day Exchange</span>
            </div>
            <div className="flex flex-col items-center text-center gap-1.5">
              <Shield size={18} className="text-muted-foreground" />
              <span className="text-[10px] text-muted-foreground leading-tight">Cash on Delivery</span>
            </div>
          </div>
        </div>
      </div>

      {sizeChartOpen && sizeChart.length > 0 && (
        <div ref={sizeChartRef} className="mt-8 border border-border rounded-lg p-6 scroll-mt-24">
          <div className="flex items-center justify-between mb-4">
            <h3 className="text-lg font-medium" style={{ fontFamily: "Georgia, serif" }}>Size Chart</h3>
            <button onClick={() => setSizeChartOpen(false)} className="text-sm text-muted-foreground hover:text-foreground">Close</button>
          </div>
          <p className="text-xs text-muted-foreground mb-4">All measurements are in inches. Please measure yourself and compare with the chart below to find your perfect fit.</p>
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-border">
                  <th className="text-left py-2 pr-4 font-medium text-xs tracking-wider uppercase">Size</th>
                  <th className="text-left py-2 px-4 font-medium text-xs tracking-wider uppercase">Bust</th>
                  <th className="text-left py-2 px-4 font-medium text-xs tracking-wider uppercase">Waist</th>
                  <th className="text-left py-2 px-4 font-medium text-xs tracking-wider uppercase">Hips</th>
                  <th className="text-left py-2 px-4 font-medium text-xs tracking-wider uppercase">Length</th>
                </tr>
              </thead>
              <tbody>
                {sizeChart.map((row) => (
                  <tr key={row.size} className={`border-b border-border ${selectedSize === row.size ? "bg-muted" : ""}`}>
                    <td className="py-2.5 pr-4 font-medium">{row.size}</td>
                    <td className="py-2.5 px-4 text-muted-foreground">{row.bust || "—"}</td>
                    <td className="py-2.5 px-4 text-muted-foreground">{row.waist || "—"}</td>
                    <td className="py-2.5 px-4 text-muted-foreground">{row.hips || "—"}</td>
                    <td className="py-2.5 px-4 text-muted-foreground">{row.length || "—"}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <p className="text-xs text-muted-foreground mt-4 italic">Tip: If you are between sizes, we recommend sizing up for a more comfortable fit.</p>
        </div>
      )}

      <div className="mt-12 border-t border-border pt-10">
        <h3 className="text-xl font-light text-center mb-8" style={{ fontFamily: "Georgia, serif" }}>Product Details</h3>
        <div className="max-w-2xl mx-auto space-y-4">
          <div className="grid grid-cols-2 gap-4 text-sm">
            <div><span className="text-muted-foreground">Category:</span> <span className="font-medium">{product.categoryName || "—"}</span></div>
            <div><span className="text-muted-foreground">Brand:</span> <span className="font-medium">{product.brand || "—"}</span></div>
            <div><span className="text-muted-foreground">SKU:</span> <span className="font-medium">{product.sku}</span></div>
            <div><span className="text-muted-foreground">Available Sizes:</span> <span className="font-medium">{sizes.join(", ")}</span></div>
          </div>
          {product.description && (
            <div className="pt-4 border-t border-border">
              <p className="text-sm text-muted-foreground leading-relaxed whitespace-pre-line">{product.description}</p>
            </div>
          )}
        </div>
      </div>

      <div className="mt-12 border-t border-border pt-10">
        <h3 className="text-xl font-light text-center mb-8" style={{ fontFamily: "Georgia, serif" }}>Customer Reviews</h3>
        {reviews.length > 0 ? (
          <div className="space-y-6 max-w-2xl mx-auto">
            {reviews.map((review) => (
              <div key={review.id} className="border-b border-border pb-6">
                <div className="flex items-center justify-between mb-2">
                  <span className="text-sm font-medium">{review.userName}</span>
                  <div className="flex items-center space-x-0.5">
                    {Array.from({ length: 5 }).map((_, i) => <Star key={i} size={12} className={i < review.score ? "fill-amber-400 text-amber-400" : "text-gray-300"} />)}
                  </div>
                </div>
                <p className="text-sm text-muted-foreground">{review.comment}</p>
              </div>
            ))}
          </div>
        ) : (
          <p className="text-center text-muted-foreground text-sm">No reviews yet.</p>
        )}
      </div>

      <div className="mt-16 border-t border-border pt-12">
        <h3 className="text-xl font-light text-center mb-8" style={{ fontFamily: "Georgia, serif" }}>You May Also Like</h3>
        <div className="text-center text-muted-foreground text-sm">Coming soon</div>
      </div>
    </div>
  );
}
