import { useEffect, useState } from "react";
import { useParams, Link, useNavigate } from "react-router-dom";
import { deliveryApi } from "@/services/api";
import type { Delivery } from "@/types";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Skeleton } from "@/components/ui/skeleton";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import toast from "react-hot-toast";
import { statusColors } from "@/lib/statusColors";
import { Trash2 } from "lucide-react";

export default function AdminDeliveryDetail() {
  const { id } = useParams();
  const [delivery, setDelivery] = useState<Delivery | null>(null);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  useEffect(() => {
    if (!id) return;
    deliveryApi.getById(id).then((res) => setDelivery(res.data.data || null)).finally(() => setLoading(false));
  }, [id]);

  const handleStatus = async (status: string) => {
    if (!id) return;
    try { await deliveryApi.updateTracking(id, { status }); toast.success("Updated"); setDelivery((d) => d ? { ...d, status: status as Delivery["status"] } : d); } catch { toast.error("Failed"); }
  };

  const handleDelete = async () => {
    if (!id || !confirm("Delete this delivery?")) return;
    await deliveryApi.delete(id);
    toast.success("Delivery deleted");
    navigate("/admin/deliveries");
  };

  if (loading) return <div className="space-y-4">{Array.from({ length: 4 }).map((_, i) => <Skeleton key={i} className="h-16 w-full" />)}</div>;
  if (!delivery) return <div className="text-center py-16 text-muted-foreground">Delivery not found</div>;

  return (
    <div className="max-w-[1400px] mx-auto px-4 sm:px-6 lg:px-8 py-8 lg:py-12">
      <div className="flex items-start justify-between mb-8 gap-4 flex-wrap">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">Delivery {delivery.trackingNumber}</h1>
          <p className="text-sm text-muted-foreground">Order: <Link to={`/admin/orders/${delivery.orderId}`} className="hover:underline">{delivery.orderNumber}</Link></p>
        </div>
        <div className="flex gap-2 flex-wrap">
          <Button variant="outline" size="sm" onClick={handleDelete} className="text-destructive hover:text-destructive"><Trash2 size={14} /></Button>
          <Link to="/admin/deliveries"><Button variant="outline" size="sm">Back</Button></Link>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-2 space-y-6">
          <Card>
            <CardHeader><CardTitle className="text-base">Items</CardTitle></CardHeader>
            <CardContent className="p-0">
              <Table>
                <TableHeader><TableRow><TableHead>Product</TableHead><TableHead>Size</TableHead><TableHead>Color</TableHead><TableHead>Qty Shipped</TableHead><TableHead>Ordered Qty</TableHead></TableRow></TableHeader>
                <TableBody>
                  {!delivery.items?.length ? <TableRow><TableCell colSpan={5} className="text-center py-8 text-muted-foreground">No items</TableCell></TableRow> :
                    delivery.items.map((item) => (
                      <TableRow key={item.id}>
                        <TableCell className="font-medium">{item.productName}</TableCell>
                        <TableCell>{item.size}</TableCell>
                        <TableCell>{item.color}</TableCell>
                        <TableCell>{item.quantity}</TableCell>
                        <TableCell>{item.orderedQuantity}</TableCell>
                      </TableRow>
                    ))
                  }
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </div>

        <div className="space-y-6">
          <Card><CardContent className="p-6 space-y-3">
            <div className="flex justify-between text-sm"><span className="text-muted-foreground">Status</span><Badge className={statusColors[delivery.status]}>{delivery.status}</Badge></div>
            <div className="flex justify-between text-sm"><span className="text-muted-foreground">Carrier</span><span>{delivery.carrier}</span></div>
            <div className="flex justify-between text-sm"><span className="text-muted-foreground">Tracking</span><span className="font-mono text-xs">{delivery.trackingNumber}</span></div>
            {delivery.shippedOn && <div className="flex justify-between text-sm"><span className="text-muted-foreground">Shipped</span><span>{delivery.shippedOn}</span></div>}
            {delivery.deliveredOn && <div className="flex justify-between text-sm"><span className="text-muted-foreground">Delivered</span><span>{delivery.deliveredOn}</span></div>}
            {delivery.estimatedDelivery && <div className="flex justify-between text-sm"><span className="text-muted-foreground">Est. Delivery</span><span>{delivery.estimatedDelivery}</span></div>}
          </CardContent></Card>

          <Card><CardHeader><CardTitle className="text-base">Update Status</CardTitle></CardHeader><CardContent>
            <Select value={delivery.status} onValueChange={handleStatus}>
              <SelectTrigger className="w-full"><SelectValue /></SelectTrigger>
              <SelectContent>
                <SelectItem value="Pending">Pending</SelectItem>
                <SelectItem value="InTransit">In Transit</SelectItem>
                <SelectItem value="OutForDelivery">Out for Delivery</SelectItem>
                <SelectItem value="Delivered">Delivered</SelectItem>
                <SelectItem value="Exception">Exception</SelectItem>
              </SelectContent>
            </Select>
          </CardContent></Card>

          <Card><CardContent className="p-6">
            <Link to={`/admin/orders/${delivery.orderId}`}><Button variant="outline" className="w-full" size="sm">View Order</Button></Link>
          </CardContent></Card>
        </div>
      </div>
    </div>
  );
}
