import { useEffect } from "react";
import { Link } from "react-router-dom";
import { useCartStore } from "@/store/cartStore";
import { Button } from "@/components/ui/button";
import { X, Minus, Plus, ShoppingBag } from "lucide-react";
import { resolveImageUrl } from "@/lib/imageUtils";

export default function CartDrawer() {
  const { items, guestItems, total, isOpen, isGuest, loading, fetchCart, updateQuantity, removeItem, updateGuestQuantity, removeGuestItem, closeCart } = useCartStore();
  useEffect(() => { if (isOpen) fetchCart(); }, [isOpen, fetchCart]);

  const displayItems = isGuest ? guestItems : items;

  if (!isOpen) return null;

  return (
    <>
      <div className="fixed inset-0 bg-black/40 z-[60]" onClick={closeCart} />
      <div className="fixed inset-y-0 right-0 w-full max-w-md bg-background z-[60] shadow-xl flex flex-col">
        <div className="flex items-center justify-between px-6 py-4 border-b border-border">
          <h2 className="text-sm font-medium tracking-wider uppercase">Shopping Bag ({displayItems.length})</h2>
          <Button variant="ghost" size="icon" onClick={closeCart}><X size={20} /></Button>
        </div>

        <div className="flex-1 overflow-y-auto px-6 py-4">
          {loading && displayItems.length === 0 ? (
            <div className="space-y-4">{Array.from({ length: 3 }).map((_, i) => <div key={i} className="h-24 bg-muted rounded animate-pulse" />)}</div>
          ) : displayItems.length === 0 ? (
            <div className="flex flex-col items-center justify-center h-full text-center">
              <ShoppingBag size={48} className="text-muted-foreground mb-4" />
              <p className="text-sm text-muted-foreground mb-4">Your bag is empty</p>
              <Button onClick={closeCart}><Link to="/collections/all">Continue Shopping</Link></Button>
            </div>
          ) : (
            <div className="space-y-4">
              {displayItems.map((item: any) => (
                <div key={item.id} className="flex gap-4 border-b border-border pb-4">
                  <div className="w-20 h-28 bg-muted flex-shrink-0 overflow-hidden rounded">
                    {item.product?.images?.[0] && <img src={resolveImageUrl(item.product.images[0].imageUrl)} alt={item.product.name} className="w-full h-full object-cover" />}
                  </div>
                  <div className="flex-1 min-w-0">
                    <div className="flex items-start justify-between gap-2">
                      <h3 className="text-sm font-medium truncate">{item.product?.name || "Product"}</h3>
                      <Button variant="ghost" size="icon" className="h-6 w-6 flex-shrink-0" onClick={() => isGuest ? removeGuestItem(item.productVariantId) : removeItem(item.id)}><X size={14} /></Button>
                    </div>
                    <p className="text-xs text-muted-foreground mt-0.5">
                      {item.variant?.size && `Size: ${item.variant.size}`}{item.variant?.color && ` | Color: ${item.variant.color}`}
                    </p>
                    <div className="flex items-center justify-between mt-3">
                      <div className="flex items-center border border-border">
                        <Button variant="ghost" size="icon" className="h-7 w-7" onClick={() => isGuest ? updateGuestQuantity(item.productVariantId, item.quantity - 1) : updateQuantity(item.id, item.quantity - 1)}><Minus size={12} /></Button>
                        <span className="px-2 py-0.5 text-xs border-x border-border">{item.quantity}</span>
                        <Button variant="ghost" size="icon" className="h-7 w-7" onClick={() => isGuest ? updateGuestQuantity(item.productVariantId, item.quantity + 1) : updateQuantity(item.id, item.quantity + 1)}><Plus size={12} /></Button>
                      </div>
                      <span className="text-sm font-medium">Rs. {(item.unitPrice * item.quantity).toLocaleString()}</span>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        {displayItems.length > 0 && (
          <div className="border-t border-border px-6 py-4 space-y-3">
            <div className="flex justify-between text-sm"><span className="text-muted-foreground">Subtotal</span><span className="font-medium">Rs. {total.toLocaleString()} PKR</span></div>
            <Link to="/cart" onClick={closeCart}><Button className="w-full" size="lg">View Bag & Checkout</Button></Link>
          </div>
        )}
      </div>
    </>
  );
}
