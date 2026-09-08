import { useEffect } from "react";
import { Link } from "react-router-dom";
import { useCartStore } from "@/store/cartStore";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { ShoppingBag, X, Minus, Plus } from "lucide-react";
import SEO from "@/components/seo/SEO";
import { resolveImageUrl } from "@/lib/imageUtils";

export default function Cart() {
  const { items, guestItems, total, isGuest, fetchCart, updateQuantity, removeItem, updateGuestQuantity, removeGuestItem, loading } = useCartStore();
  useEffect(() => { fetchCart(); }, [fetchCart]);

  const displayItems = isGuest ? guestItems : items;

  if (loading && displayItems.length === 0) {
    return (
      <div className="max-w-[1400px] mx-auto px-4 py-16">
        <div className="space-y-4">{Array.from({ length: 3 }).map((_, i) => <Skeleton key={i} className="h-24 w-full" />)}</div>
      </div>
    );
  }

  if (displayItems.length === 0) {
    return (
      <div className="max-w-[1400px] mx-auto px-4 py-16 text-center">
        <SEO title="Shopping Cart" description="Your shopping cart is empty. Browse our collections and find your perfect outfit." url="https://shireenbynaeema.com/cart" />
        <ShoppingBag size={48} className="mx-auto text-muted-foreground mb-4" />
        <h1 className="text-2xl font-light mb-2" style={{ fontFamily: "Georgia, serif" }}>Your cart is empty</h1>
        <p className="text-sm text-muted-foreground mb-6">Looks like you haven&apos;t added anything yet.</p>
        <Link to="/collections/all"><Button>Continue Shopping</Button></Link>
      </div>
    );
  }

  return (
    <div className="max-w-[1400px] mx-auto px-4 sm:px-6 lg:px-8 py-8 lg:py-12">
      <SEO title="Shopping Cart" description="Review your shopping cart. Free nationwide delivery on all orders." url="https://shireenbynaeema.com/cart" />
      <h1 className="text-2xl sm:text-3xl font-light mb-8" style={{ fontFamily: "Georgia, serif" }}>Shopping Cart</h1>
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        <div className="lg:col-span-2 space-y-4">
          {displayItems.map((item: any) => (
            <Card key={item.id} className="border-0 shadow-sm">
              <CardContent className="p-4 flex gap-4">
                <div className="w-24 h-32 bg-muted flex-shrink-0 overflow-hidden rounded">
                  {item.product?.images?.[0] && <img src={resolveImageUrl(item.product.images[0].imageUrl)} alt={item.product.name} className="w-full h-full object-cover" />}
                </div>
                <div className="flex-1">
                  <div className="flex items-start justify-between">
                    <div>
                      <h3 className="text-sm font-medium">{item.product?.name || "Product"}</h3>
                      <p className="text-xs text-muted-foreground mt-0.5">
                        {item.variant?.size && `Size: ${item.variant.size}`}{item.variant?.color && ` | Color: ${item.variant.color}`}
                      </p>
                    </div>
                    <Button variant="ghost" size="icon" className="h-8 w-8" onClick={() => isGuest ? removeGuestItem(item.productVariantId) : removeItem(item.id)}><X size={16} /></Button>
                  </div>
                  <div className="flex items-center justify-between mt-4">
                    <div className="flex items-center border border-border">
                      <Button variant="ghost" size="icon" className="h-8 w-8" onClick={() => isGuest ? updateGuestQuantity(item.productVariantId, item.quantity - 1) : updateQuantity(item.id, item.quantity - 1)}><Minus size={14} /></Button>
                      <span className="px-3 py-1 text-sm border-x border-border">{item.quantity}</span>
                      <Button variant="ghost" size="icon" className="h-8 w-8" onClick={() => isGuest ? updateGuestQuantity(item.productVariantId, item.quantity + 1) : updateQuantity(item.id, item.quantity + 1)}><Plus size={14} /></Button>
                    </div>
                    <span className="text-sm font-medium">Rs. {(item.unitPrice * item.quantity).toLocaleString()} PKR</span>
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
        <div className="lg:col-span-1">
          <Card className="sticky top-24">
            <CardContent className="p-6 space-y-4">
              <h2 className="text-sm font-medium tracking-wider uppercase">Order Summary</h2>
              <div className="flex justify-between text-sm"><span className="text-muted-foreground">Subtotal</span><span>Rs. {total.toLocaleString()} PKR</span></div>
              <div className="flex justify-between text-sm"><span className="text-muted-foreground">Shipping</span><span className="text-green-600">Free</span></div>
              <div className="border-t border-border pt-2"><div className="flex justify-between font-medium"><span>Total</span><span>Rs. {total.toLocaleString()} PKR</span></div></div>
              <Link to="/checkout"><Button className="w-full" size="lg">Checkout</Button></Link>
              <Link to="/collections/all" className="block text-center text-xs text-muted-foreground underline hover:text-foreground">Continue Shopping</Link>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
