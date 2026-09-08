import { useEffect, useState } from "react";
import { useParams, Link, useNavigate } from "react-router-dom";
import { orderApi, deliveryApi, returnApi } from "@/services/api";
import type { Order } from "@/types";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Skeleton } from "@/components/ui/skeleton";
import { resolveImageUrl } from "@/lib/imageUtils";
import { Trash2 } from "lucide-react";
import toast from "react-hot-toast";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { statusColors } from "@/lib/statusColors";

const statuses = ["Pending", "Confirmed", "Processing", "Shipped", "Delivered", "Cancelled", "Returned"];

export default function AdminOrderDetail() {
  const { id } = useParams();
  const [order, setOrder] = useState<Order | null>(null);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();
  const [showDelivery, setShowDelivery] = useState(false);
  const [showReturn, setShowReturn] = useState(false);
  const [carrier, setCarrier] = useState("");
  const [tracking, setTracking] = useState("");
  const [returnReason, setReturnReason] = useState("");
  const [deliveryItems, setDeliveryItems] = useState<Record<string, number>>({});
  const [returnItems, setReturnItems] = useState<Record<string, number>>({});

  useEffect(() => {
    if (!id) return;
    orderApi.getById(id).then((res) => {
      setOrder(res.data.data || null);
    }).finally(() => setLoading(false));
  }, [id]);

  const handleStatusUpdate = async (status: string) => {
    if (!id) return;
    try { await orderApi.updateStatus(id, status); toast.success("Status updated"); setOrder((o) => o ? { ...o, status: status as Order["status"] } : o); } catch { toast.error("Failed"); }
  };

  const handleDelete = async () => {
    if (!id || !confirm("Delete this order and ALL related data (invoices, deliveries, returns)?")) return;
    await orderApi.delete(id);
    toast.success("Order deleted");
    navigate("/admin/orders");
  };

  const handleCreateDelivery = async () => {
    if (!id || !carrier || !tracking) { toast.error("Carrier and tracking required"); return; }
    const items = Object.entries(deliveryItems).filter(([, qty]) => qty > 0).map(([orderItemId, quantity]) => ({ orderItemId, quantity }));
    try {
      await deliveryApi.create({ orderId: id, carrier, trackingNumber: tracking, items: items.length > 0 ? items : undefined });
      toast.success("Delivery created");
      setShowDelivery(false); setCarrier(""); setTracking(""); setDeliveryItems({});
      const res = await orderApi.getById(id); setOrder(res.data.data || null);
    } catch { toast.error("Failed"); }
  };

  const handleCreateReturn = async () => {
    if (!id || !returnReason) { toast.error("Reason required"); return; }
    const items = Object.entries(returnItems).filter(([, qty]) => qty > 0).map(([orderItemId, quantity]) => ({ orderItemId, quantity, reason: returnReason }));
    try {
      await returnApi.request({ orderId: id, reason: returnReason, bankAccount: "", bankName: "", accountHolderName: "", items: items.length > 0 ? items : undefined });
      toast.success("Return requested");
      setShowReturn(false); setReturnReason(""); setReturnItems({});
      const res = await orderApi.getById(id); setOrder(res.data.data || null);
    } catch { toast.error("Failed"); }
  };

  if (loading) return <div className="space-y-4">{Array.from({ length: 4 }).map((_, i) => <Skeleton key={i} className="h-16 w-full" />)}</div>;
  if (!order) return <div className="text-center py-16 text-muted-foreground">Order not found</div>;

  return (
    <div className="max-w-[1400px] mx-auto px-4 sm:px-6 lg:px-8 py-8 lg:py-12">
      <div className="flex items-start justify-between mb-8 gap-4 flex-wrap">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">Order {order.orderNumber}</h1>
          <p className="text-sm text-muted-foreground">{order.createdOn ? new Date(order.createdOn * 1000).toLocaleString() : ""}</p>
        </div>
        <div className="flex gap-2 flex-wrap">
          <Button variant="outline" size="sm" onClick={() => { setShowDelivery(!showDelivery); setShowReturn(false); }}>Create Delivery</Button>
          <Button variant="outline" size="sm" onClick={() => { setShowReturn(!showReturn); setShowDelivery(false); }}>Request Return</Button>
          <Button variant="outline" size="sm" onClick={handleDelete} className="text-destructive hover:text-destructive"><Trash2 size={14} /></Button>
          <Link to="/admin/orders"><Button variant="outline" size="sm">Back</Button></Link>
        </div>
      </div>

      {showDelivery && (
        <Card className="mb-6"><CardContent className="p-6 space-y-4">
          <h3 className="text-sm font-medium">Create Delivery</h3>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <Input placeholder="Carrier" value={carrier} onChange={(e) => setCarrier(e.target.value)} />
            <Input placeholder="Tracking Number" value={tracking} onChange={(e) => setTracking(e.target.value)} />
          </div>
          <Table><TableHeader><TableRow><TableHead>Product</TableHead><TableHead>Ordered</TableHead><TableHead>Already Delivered</TableHead><TableHead>Remaining</TableHead><TableHead>Ship Qty</TableHead></TableRow></TableHeader>
            <TableBody>{order.items?.map((item) => {
              const remaining = item.quantity - (item.deliveredQuantity || 0);
              return <TableRow key={item.id}><TableCell>{item.productName} ({item.size}/{item.color})</TableCell><TableCell>{item.quantity}</TableCell><TableCell>{item.deliveredQuantity || 0}</TableCell><TableCell>{remaining}</TableCell><TableCell><Input type="number" min={0} max={remaining} className="w-20 h-8" value={deliveryItems[item.id] || ""} onChange={(e) => setDeliveryItems({ ...deliveryItems, [item.id]: parseInt(e.target.value) || 0 })} disabled={remaining <= 0} /></TableCell></TableRow>;
            })}</TableBody></Table>
          <Button onClick={handleCreateDelivery} disabled={!carrier || !tracking}>Create Delivery</Button>
        </CardContent></Card>
      )}

      {showReturn && (
        <Card className="mb-6"><CardContent className="p-6 space-y-4">
          <h3 className="text-sm font-medium">Request Return</h3>
          <Input placeholder="Reason for return" value={returnReason} onChange={(e) => setReturnReason(e.target.value)} />
          <Table><TableHeader><TableRow><TableHead>Product</TableHead><TableHead>Ordered</TableHead><TableHead>Already Returned</TableHead><TableHead>Remaining</TableHead><TableHead>Return Qty</TableHead></TableRow></TableHeader>
            <TableBody>{order.items?.map((item) => {
              const remaining = item.quantity - (item.returnedQuantity || 0);
              return <TableRow key={item.id}><TableCell>{item.productName} ({item.size}/{item.color})</TableCell><TableCell>{item.quantity}</TableCell><TableCell>{item.returnedQuantity || 0}</TableCell><TableCell>{remaining}</TableCell><TableCell><Input type="number" min={0} max={remaining} className="w-20 h-8" value={returnItems[item.id] || ""} onChange={(e) => setReturnItems({ ...returnItems, [item.id]: parseInt(e.target.value) || 0 })} disabled={remaining <= 0} /></TableCell></TableRow>;
            })}</TableBody></Table>
          <Button onClick={handleCreateReturn} disabled={!returnReason}>Submit Return</Button>
        </CardContent></Card>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-2 space-y-6">
          <Card>
            <CardHeader className="flex flex-row items-center justify-between">
              <CardTitle className="text-base">Items</CardTitle>
              {order.customerName && <span className="text-sm text-muted-foreground">{order.customerName}</span>}
            </CardHeader>
            <CardContent className="p-0">
              <Table>
                <TableHeader><TableRow><TableHead>Product</TableHead><TableHead>Size / Color</TableHead><TableHead>Qty</TableHead><TableHead>Delivered</TableHead><TableHead>Returned</TableHead><TableHead>Total</TableHead></TableRow></TableHeader>
                <TableBody>
                  {order.items?.map((item) => (
                    <TableRow key={item.id}>
                      <TableCell className="font-medium">
                        <div className="flex items-center gap-3">
                          {item.imageUrl && <img src={resolveImageUrl(item.imageUrl)} alt="" className="w-10 h-10 object-cover rounded" />}
                          <span>{item.productName || "Unknown Product"}</span>
                        </div>
                      </TableCell>
                      <TableCell className="text-muted-foreground">{item.size} / {item.color}</TableCell>
                      <TableCell>{item.quantity}</TableCell>
                      <TableCell>{item.deliveredQuantity || 0}</TableCell>
                      <TableCell>{item.returnedQuantity || 0}</TableCell>
                      <TableCell>Rs. {item.total.toLocaleString()}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </div>

        <div className="space-y-6">
          <Card><CardContent className="p-6 space-y-3">
            <div className="flex justify-between text-sm"><span className="text-muted-foreground">Status</span><Badge className={statusColors[order.status]}>{order.status}</Badge></div>
            <div className="flex justify-between text-sm"><span className="text-muted-foreground">Subtotal</span><span>Rs. {order.subtotal.toLocaleString()}</span></div>
            <div className="flex justify-between text-sm"><span className="text-muted-foreground">Tax</span><span>Rs. {order.tax.toLocaleString()}</span></div>
            <div className="flex justify-between text-sm"><span className="text-muted-foreground">Shipping</span><span>Free</span></div>
            {order.discount > 0 && <div className="flex justify-between text-sm"><span className="text-muted-foreground">Discount{order.couponCode ? ` (${order.couponCode})` : ""}</span><span className="text-green-600">-Rs. {order.discount.toLocaleString()}</span></div>}
            <div className="border-t border-border pt-2 flex justify-between font-medium"><span>Total</span><span>Rs. {order.total.toLocaleString()} PKR</span></div>
          </CardContent></Card>

          <Card><CardHeader><CardTitle className="text-base">Update Status</CardTitle></CardHeader><CardContent>
            <Select value={order.status} onValueChange={handleStatusUpdate}>
              <SelectTrigger className="w-full"><SelectValue /></SelectTrigger>
              <SelectContent>
                {statuses.map((s) => <SelectItem key={s} value={s}>{s}</SelectItem>)}
              </SelectContent>
            </Select>
          </CardContent></Card>

          {order.shippingAddress && (
            <Card><CardHeader><CardTitle className="text-base">Shipping</CardTitle></CardHeader><CardContent>
              <p className="text-sm text-muted-foreground">{order.shippingAddress.street}<br />{order.shippingAddress.city}, {order.shippingAddress.state} {order.shippingAddress.zipCode}<br />{order.shippingAddress.country}</p>
            </CardContent></Card>
          )}

          {order.customerEmail && (
            <Card><CardHeader><CardTitle className="text-base">Customer</CardTitle></CardHeader><CardContent>
              <p className="text-sm text-muted-foreground">{order.customerName}<br />{order.customerEmail}</p>
            </CardContent></Card>
          )}
        </div>
      </div>
    </div>
  );
}
