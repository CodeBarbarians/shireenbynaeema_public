import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { Elements } from "@stripe/react-stripe-js";
import { loadStripe } from "@stripe/stripe-js";
import { useCartStore } from "@/store/cartStore";
import { orderApi, paymentApi, couponApi } from "@/services/api";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import StripePaymentForm from "@/components/payment/StripePaymentForm";
import toast from "react-hot-toast";
import SEO from "@/components/seo/SEO";
import { resolveImageUrl } from "@/lib/imageUtils";

const stripePromise = loadStripe(import.meta.env.VITE_STRIPE_PUBLISHABLE_KEY || "");

export default function Checkout() {
  const { items, guestItems, total, isGuest, clearGuestCart } = useCartStore();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [paymentMethod, setPaymentMethod] = useState("cod");
  const [clientSecret, setClientSecret] = useState("");
  const [orderId, setOrderId] = useState("");
  const [step, setStep] = useState<"form" | "payment">("form");
  const [form, setForm] = useState({
    firstName: "", lastName: "", street: "", city: "", zipCode: "", country: "Pakistan",
    email: "", phone: "",
  });
  const [couponCode, setCouponCode] = useState("");
  const [couponDiscount, setCouponDiscount] = useState(0);
  const [couponApplied, setCouponApplied] = useState(false);
  const [couponError, setCouponError] = useState("");

  const displayItems = isGuest ? guestItems : items;
  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => setForm({ ...form, [e.target.name]: e.target.value });

  const handleApplyCoupon = async () => {
    if (!couponCode.trim()) return;
    setCouponError("");
    try {
      const res = await couponApi.validate(couponCode, total);
      if (res.data.isSuccess) {
        setCouponDiscount(res.data.data.discount);
        setCouponApplied(true);
        toast.success(`Coupon applied! Rs. ${res.data.data.discount.toLocaleString()} off`);
      } else {
        setCouponError(res.data.message || "Invalid coupon");
        setCouponDiscount(0);
        setCouponApplied(false);
      }
    } catch { setCouponError("Invalid coupon"); }
  };

  const finalTotal = Math.max(0, total - couponDiscount);

  const placeOrder = async () => {
    const orderItems = isGuest ? guestItems.map((item) => ({
      productVariantId: item.productVariantId, quantity: item.quantity, unitPrice: item.unitPrice,
    })) : undefined;
    const res = await orderApi.place({
      paymentIntentId: "", firstName: form.firstName, lastName: form.lastName,
      email: form.email, phone: form.phone, street: form.street, city: form.city,
      state: "", zipCode: form.zipCode, country: form.country, items: orderItems,
      couponCode: couponApplied ? couponCode : undefined,
    });
    if (!res.data.isSuccess) throw new Error(res.data.message || "Failed to place order");
    return res.data.data as string;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.firstName || !form.lastName || !form.email || !form.phone || !form.street || !form.city || !form.zipCode) {
      toast.error("Please fill in all fields");
      return;
    }
    setLoading(true);
    try {
      if (paymentMethod === "stripe") {
        const id = await placeOrder();
        setOrderId(id);
        const intentRes = await paymentApi.createIntent(id);
        const secret = intentRes.data.data.clientSecret;
        setClientSecret(secret);
        setStep("payment");
      } else {
        await placeOrder();
        if (isGuest) clearGuestCart();
        toast.success("Order placed successfully!");
        navigate("/");
      }
    } catch (err: any) {
      toast.error(err.message || "Something went wrong");
    } finally {
      setLoading(false);
    }
  };

  const handlePaymentSuccess = () => {
    if (isGuest) clearGuestCart();
    navigate("/");
  };

  return (
    <div className="min-h-screen bg-[#f7f7f7]">
      <SEO
        title="Checkout"
        description="Complete your order at Shireen by Naeema. Secure checkout with multiple payment options."
        url="https://shireenbynaeema.com/checkout"
      />
      <div className="max-w-[1280px] mx-auto">
        <div className="grid grid-cols-1 lg:grid-cols-[1fr_400px] min-h-[calc(100vh-57px)]">
          <div className="bg-[#f7f7f7] px-6 lg:px-12 py-8 lg:py-12">
            {step === "form" ? (
              <form onSubmit={handleSubmit} className="max-w-[560px] mx-auto space-y-8">
                <div>
                  <h2 className="text-[17px] font-semibold text-[#1a1a1a] mb-3">Contact</h2>
                  <Input
                    name="email" type="email" value={form.email} onChange={handleChange}
                    placeholder="Email or mobile phone number" required
                    className="h-[48px] rounded-lg border-[#c8c8c8] bg-white text-sm placeholder:text-[#767676] focus:border-[#1a1a1a] focus:ring-1 focus:ring-[#1a1a1a]"
                  />
                  <label className="flex items-center gap-2.5 mt-3 text-sm text-[#1a1a1a] cursor-pointer">
                    <input type="checkbox" defaultChecked className="w-4 h-4 rounded border-[#c8c8c8] accent-[#1a1a1a]" />
                    Email me with news and offers
                  </label>
                </div>

                <div>
                  <h2 className="text-[17px] font-semibold text-[#1a1a1a] mb-3">Delivery</h2>
                  <div className="relative mb-4">
                    <select className="w-full h-[48px] rounded-lg border border-[#c8c8c8] bg-white px-4 text-sm text-[#1a1a1a] appearance-none cursor-pointer focus:border-[#1a1a1a] focus:ring-1 focus:ring-[#1a1a1a]">
                      <option>Pakistan</option>
                    </select>
                    <div className="absolute right-4 top-1/2 -translate-y-1/2 pointer-events-none">
                      <svg width="12" height="12" viewBox="0 0 12 12" fill="none"><path d="M2 4.5L6 8.5L10 4.5" stroke="#1a1a1a" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round"/></svg>
                    </div>
                  </div>
                  <div className="grid grid-cols-2 gap-4 mb-4">
                    <Input name="firstName" value={form.firstName} onChange={handleChange} placeholder="First name" required
                      className="h-[48px] rounded-lg border-[#c8c8c8] bg-white text-sm placeholder:text-[#767676] focus:border-[#1a1a1a] focus:ring-1 focus:ring-[#1a1a1a]" />
                    <Input name="lastName" value={form.lastName} onChange={handleChange} placeholder="Last name" required
                      className="h-[48px] rounded-lg border-[#c8c8c8] bg-white text-sm placeholder:text-[#767676] focus:border-[#1a1a1a] focus:ring-1 focus:ring-[#1a1a1a]" />
                  </div>
                  <Input name="street" value={form.street} onChange={handleChange} placeholder="Address" required
                    className="h-[48px] rounded-lg border-[#c8c8c8] bg-white text-sm placeholder:text-[#767676] focus:border-[#1a1a1a] focus:ring-1 focus:ring-[#1a1a1a] mb-4" />
                  <div className="grid grid-cols-2 gap-4 mb-4">
                    <Input name="city" value={form.city} onChange={handleChange} placeholder="City" required
                      className="h-[48px] rounded-lg border-[#c8c8c8] bg-white text-sm placeholder:text-[#767676] focus:border-[#1a1a1a] focus:ring-1 focus:ring-[#1a1a1a]" />
                    <Input name="zipCode" value={form.zipCode} onChange={handleChange} placeholder="Postal code" required
                      className="h-[48px] rounded-lg border-[#c8c8c8] bg-white text-sm placeholder:text-[#767676] focus:border-[#1a1a1a] focus:ring-1 focus:ring-[#1a1a1a]" />
                  </div>
                  <div className="relative">
                    <Input name="phone" type="tel" value={form.phone} onChange={handleChange} placeholder="Phone" required
                      className="h-[48px] rounded-lg border-[#c8c8c8] bg-white text-sm placeholder:text-[#767676] focus:border-[#1a1a1a] focus:ring-1 focus:ring-[#1a1a1a] pr-10" />
                    <button type="button" className="absolute right-3 top-1/2 -translate-y-1/2 w-5 h-5 rounded-full border border-[#c8c8c8] text-[#767676] text-xs flex items-center justify-center hover:border-[#1a1a1a] hover:text-[#1a1a1a]">?</button>
                  </div>
                  <label className="flex items-center gap-2.5 mt-4 text-sm text-[#1a1a1a] cursor-pointer">
                    <input type="checkbox" className="w-4 h-4 rounded border-[#c8c8c8] accent-[#1a1a1a]" />
                    Save this information for next time
                  </label>
                </div>

                <div>
                  <h2 className="text-[17px] font-semibold text-[#1a1a1a] mb-3">Shipping method</h2>
                  <div className="rounded-lg border border-[#c8c8c8] bg-white px-4 py-3 flex items-center justify-between">
                    <div className="flex items-center gap-3">
                      <input type="radio" name="shipping" defaultChecked className="w-4 h-4 accent-[#1a1a1a]" />
                      <span className="text-sm text-[#1a1a1a]">Standard Shipping</span>
                    </div>
                    <span className="text-sm text-[#1a1a1a]">FREE</span>
                  </div>
                </div>

                <div>
                  <h2 className="text-[17px] font-semibold text-[#1a1a1a] mb-1">Payment</h2>
                  <p className="text-sm text-[#767676] mb-3">All transactions are secure and encrypted.</p>
                  <div className="space-y-0">
                    <div className={`rounded-t-lg border border-b-0 ${paymentMethod === "stripe" ? "border-[#1a1a1a]" : "border-[#c8c8c8]"}`}>
                      <label className="flex items-center gap-3 px-4 py-3 cursor-pointer">
                        <input type="radio" name="payment" value="stripe" checked={paymentMethod === "stripe"} onChange={() => setPaymentMethod("stripe")} className="w-4 h-4 accent-[#1a1a1a]" />
                        <span className="text-sm text-[#1a1a1a] flex-1">Credit / Debit Card (Stripe)</span>
                        <div className="flex gap-1">
                          <div className="w-[38px] h-[24px] bg-[#1a1f71] rounded flex items-center justify-center text-white text-[8px] font-bold">VISA</div>
                          <div className="w-[38px] h-[24px] bg-[#eb001b] rounded flex items-center justify-center relative overflow-hidden">
                            <div className="absolute left-2 w-4 h-4 bg-[#eb001b] rounded-full opacity-80" />
                            <div className="absolute right-2 w-4 h-4 bg-[#f79e1b] rounded-full opacity-80" />
                          </div>
                          <div className="w-[38px] h-[24px] bg-[#6772e5] rounded flex items-center justify-center text-white text-[7px] font-bold">Stripe</div>
                        </div>
                      </label>
                    </div>
                    <div className={`border border-b-0 ${paymentMethod === "bank" ? "border-[#1a1a1a]" : "border-[#c8c8c8]"}`}>
                      <label className="flex items-center gap-3 px-4 py-3 cursor-pointer">
                        <input type="radio" name="payment" value="bank" checked={paymentMethod === "bank"} onChange={() => setPaymentMethod("bank")} className="w-4 h-4 accent-[#1a1a1a]" />
                        <span className="text-sm text-[#1a1a1a]">Bank Transfer (5% OFF)</span>
                      </label>
                    </div>
                    <div className={`rounded-b-lg border ${paymentMethod === "cod" ? "border-[#1a1a1a]" : "border-[#c8c8c8]"}`}>
                      <label className="flex items-center gap-3 px-4 py-3 cursor-pointer">
                        <input type="radio" name="payment" value="cod" checked={paymentMethod === "cod"} onChange={() => setPaymentMethod("cod")} className="w-4 h-4 accent-[#1a1a1a]" />
                        <span className="text-sm text-[#1a1a1a]">Cash on Delivery (COD)</span>
                      </label>
                    </div>
                  </div>
                </div>

                <div className="lg:hidden pt-4">
                  <Button type="submit" disabled={loading || displayItems.length === 0}
                    className="w-full h-[50px] bg-[#1a1a1a] hover:bg-[#333] text-white text-sm font-medium rounded-lg">
                    {loading ? "Processing..." : paymentMethod === "stripe" ? "Continue to payment" : "Complete order"}
                  </Button>
                </div>
              </form>
            ) : (
              <div className="max-w-[560px] mx-auto space-y-6">
                <div>
                  <button onClick={() => setStep("form")} className="text-sm text-[#767676] hover:text-[#1a1a1a] mb-4">&larr; Back to checkout</button>
                  <h2 className="text-[17px] font-semibold text-[#1a1a1a] mb-3">Payment</h2>
                  <p className="text-sm text-[#767676] mb-4">Order #{orderId}</p>
                </div>
                {clientSecret && (
                  <Elements stripe={stripePromise} options={{ clientSecret, appearance: { theme: "stripe" } }}>
                    <StripePaymentForm onSuccess={handlePaymentSuccess} onError={(msg) => toast.error(msg)} />
                  </Elements>
                )}
              </div>
            )}
          </div>

          <div className="bg-white border-l border-[#e8e8e8] px-8 py-8 lg:py-12">
            <div className="max-w-[400px] mx-auto lg:sticky lg:top-8">
              <div className="space-y-4 mb-6">
                {displayItems.map((item: any) => (
                  <div key={item.id} className="flex items-start gap-4">
                    <div className="relative w-[72px] h-[72px] bg-[#f5f5f5] rounded-lg overflow-hidden flex-shrink-0">
                      {item.product?.images?.[0] && (
                        <img src={resolveImageUrl(item.product.images[0].imageUrl)} alt={item.product?.name || "Product"} className="w-full h-full object-cover" loading="lazy" />
                      )}
                      <span className="absolute -top-1.5 -right-1.5 w-5 h-5 bg-[#1a1a1a] text-white text-[10px] rounded-full flex items-center justify-center font-medium">
                        {item.quantity}
                      </span>
                    </div>
                    <div className="flex-1 min-w-0 pt-0.5">
                      <p className="text-sm text-[#1a1a1a] font-medium leading-tight">{item.product?.name || "Product"}</p>
                      <p className="text-xs text-[#767676] mt-0.5">{item.variant?.size}</p>
                    </div>
                    <span className="text-sm text-[#1a1a1a] pt-0.5">Rs {item.unitPrice.toLocaleString()}.00</span>
                  </div>
                ))}
              </div>
              <div className="flex gap-2 mb-6">
                <Input placeholder="Discount code" value={couponCode} onChange={(e) => setCouponCode(e.target.value.toUpperCase())} className="h-[44px] rounded-lg border-[#c8c8c8] bg-white text-sm placeholder:text-[#767676] flex-1 focus:border-[#1a1a1a] focus:ring-1 focus:ring-[#1a1a1a]" />
                <Button type="button" variant="outline" className="h-[44px] px-6 rounded-lg border-[#c8c8c8] text-sm font-medium hover:bg-[#f5f5f5]" onClick={handleApplyCoupon} disabled={couponApplied}>{couponApplied ? "Applied" : "Apply"}</Button>
              </div>
              {couponError && <p className="text-xs text-red-500 mb-2">{couponError}</p>}
              <div className="space-y-3 pt-4 border-t border-[#e8e8e8]">
                <div className="flex justify-between text-sm">
                  <span className="text-[#1a1a1a]">Subtotal</span>
                  <span className="text-[#1a1a1a]">Rs {total.toLocaleString()}.00</span>
                </div>
                {couponApplied && (
                  <div className="flex justify-between text-sm">
                    <span className="text-green-600">Discount</span>
                    <span className="text-green-600">-Rs {couponDiscount.toLocaleString()}.00</span>
                  </div>
                )}
                <div className="flex justify-between text-sm">
                  <span className="text-[#1a1a1a]">Shipping</span>
                  <span className="text-[#1a1a1a] uppercase text-xs font-medium">FREE</span>
                </div>
                <div className="flex justify-between items-baseline pt-4 border-t border-[#e8e8e8]">
                  <span className="text-base font-semibold text-[#1a1a1a]">Total</span>
                  <div className="text-right">
                    <span className="text-xs text-[#767676] mr-1">PKR</span>
                    <span className="text-xl font-semibold text-[#1a1a1a]">Rs {finalTotal.toLocaleString()}.00</span>
                  </div>
                </div>
              </div>
              <div className="hidden lg:block pt-6">
                <Button type="submit" disabled={loading || displayItems.length === 0} onClick={handleSubmit}
                  className="w-full h-[50px] bg-[#1a1a1a] hover:bg-[#333] text-white text-sm font-medium rounded-lg">
                  {loading ? "Processing..." : paymentMethod === "stripe" ? "Continue to payment" : "Complete order"}
                </Button>
                <p className="text-center text-xs text-[#767676] mt-3">
                  <Link to="/collections/all" className="underline underline-offset-2 hover:text-[#1a1a1a]">Continue shopping</Link>
                </p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
