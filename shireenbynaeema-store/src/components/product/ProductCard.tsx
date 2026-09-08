import { Link } from "react-router-dom";
import type { Product } from "@/types";
import { Card, CardContent } from "@/components/ui/card";
import { resolveImageUrl } from "@/lib/imageUtils";

export default function ProductCard({ product }: { product: Product }) {
  const primaryImage = resolveImageUrl(product.images?.find((img) => img.isPrimary)?.imageUrl || product.images?.[0]?.imageUrl || "");
  const hoverImage = resolveImageUrl(product.images?.[1]?.imageUrl || product.images?.[0]?.imageUrl || "");
  const displayPrice = product.salePrice && product.salePrice < product.basePrice ? product.salePrice : product.basePrice;
  const hasDiscount = product.salePrice != null && product.salePrice < product.basePrice;

  return (
    <Link to={`/products/${product.id}`} className="group block" aria-label={`View ${product.name} - Rs. ${displayPrice.toLocaleString()}`}>
      <Card className="border-0 shadow-none overflow-hidden">
        <div className="relative aspect-[3/4] overflow-hidden bg-muted mb-3">
          {primaryImage && (
            <>
              <img src={primaryImage} alt={product.images?.[0]?.altText || product.name} className="absolute inset-0 w-full h-full object-cover transition-opacity duration-500 group-hover:opacity-0" loading="lazy" />
              {hoverImage !== primaryImage && (
                <img src={hoverImage} alt={product.images?.[1]?.altText || `${product.name} alternate view`} className="absolute inset-0 w-full h-full object-cover opacity-0 transition-opacity duration-500 group-hover:opacity-100" loading="lazy" />
              )}
            </>
          )}
          {hasDiscount && (
            <span className="absolute top-3 left-3 bg-white text-[10px] font-medium tracking-wider uppercase px-2 py-1">Sale</span>
          )}
          <div className="absolute bottom-0 left-0 right-0 p-3 bg-gradient-to-t from-black/20 to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-300">
            <span className="w-full bg-white/95 text-black text-xs font-medium tracking-wider uppercase py-2.5 inline-block text-center hover:bg-black hover:text-white transition-colors">
              Quick View
            </span>
          </div>
        </div>
        <CardContent className="p-5 space-y-1">
          <h3 className="text-sm font-medium text-foreground group-hover:text-muted-foreground transition-colors line-clamp-1">{product.name}</h3>
          <div className="flex items-center space-x-2">
            <span className="text-sm font-medium">Rs. {displayPrice.toLocaleString()} PKR</span>
            {hasDiscount && <span className="text-sm text-muted-foreground line-through">Rs. {product.basePrice.toLocaleString()} PKR</span>}
          </div>
          {product.variants && product.variants.length > 0 && (
            <div className="flex items-center space-x-1.5 pt-1">
              {[...new Set(product.variants.map((v) => v.size))].slice(0, 5).map((size) => (
                <span key={size} className="text-[10px] text-muted-foreground border border-border px-1.5 py-0.5">{size}</span>
              ))}
            </div>
          )}
        </CardContent>
      </Card>
    </Link>
  );
}
