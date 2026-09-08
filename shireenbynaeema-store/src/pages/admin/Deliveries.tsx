import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { deliveryApi } from "@/services/api";
import type { Delivery } from "@/types";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Skeleton } from "@/components/ui/skeleton";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Trash2 } from "lucide-react";
import toast from "react-hot-toast";
import { statusColors } from "@/lib/statusColors";

export default function AdminDeliveries() {
  const [deliveries, setDeliveries] = useState<Delivery[]>([]);
  const [loading, setLoading] = useState(true);
  const [expandedId, setExpandedId] = useState<string | null>(null);
  useEffect(() => { deliveryApi.list().then((res) => setDeliveries(res.data.data?.entities || [])).finally(() => setLoading(false)); }, []);

  const handleStatus = async (id: string, status: string) => {
    try { await deliveryApi.updateTracking(id, { status }); toast.success("Updated"); setDeliveries(deliveries.map((d) => d.id === id ? { ...d, status: status as Delivery["status"] } : d)); } catch { toast.error("Failed"); }
  };

  const handleDelete = async (id: string) => {
    if (!confirm("Delete this delivery?")) return;
    await deliveryApi.delete(id);
    setDeliveries((prev) => prev.filter((d) => d.id !== id));
    toast.success("Delivery deleted");
  };

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-semibold tracking-tight">Deliveries</h1>
      <Card><CardContent className="p-0">
        <div className="overflow-x-auto">
        <Table>
          <TableHeader><TableRow><TableHead>Order</TableHead><TableHead className="hidden sm:table-cell">Carrier</TableHead><TableHead className="hidden md:table-cell">Tracking</TableHead><TableHead className="hidden sm:table-cell">Items</TableHead><TableHead>Status</TableHead><TableHead className="hidden lg:table-cell">Shipped</TableHead><TableHead className="text-right">Actions</TableHead></TableRow></TableHeader>
          <TableBody>
            {loading ? Array.from({ length: 5 }).map((_, i) => <TableRow key={i}>{Array.from({ length: 7 }).map((__, j) => <TableCell key={j}><Skeleton className="h-4 w-full" /></TableCell>)}</TableRow>) :
              deliveries.length === 0 ? <TableRow><TableCell colSpan={7} className="text-center py-8 text-muted-foreground">No deliveries yet</TableCell></TableRow> :
              deliveries.map((d) => <>
                <TableRow key={d.id} className="cursor-pointer hover:bg-muted/50" onClick={() => setExpandedId(expandedId === d.id ? null : d.id)}>
                  <TableCell className="font-medium"><Link to={`/admin/deliveries/${d.id}`} className="hover:underline" onClick={(e) => e.stopPropagation()}>{d.orderNumber || "-"}</Link></TableCell>
                  <TableCell className="text-muted-foreground">{d.carrier}</TableCell>
                  <TableCell className="text-muted-foreground font-mono text-xs">{d.trackingNumber}</TableCell>
                  <TableCell>{d.items?.length || 0} item(s)</TableCell>
                  <TableCell><Badge className={statusColors[d.status]}>{d.status}</Badge></TableCell>
                  <TableCell className="text-muted-foreground">{d.shippedOn || "-"}</TableCell>
                  <TableCell className="text-right" onClick={(e) => e.stopPropagation()}><Select value={d.status} onValueChange={(v) => handleStatus(d.id, v)}><SelectTrigger className="h-9 text-xs w-36"><SelectValue /></SelectTrigger><SelectContent><SelectItem value="Pending">Pending</SelectItem><SelectItem value="InTransit">In Transit</SelectItem><SelectItem value="OutForDelivery">Out for Delivery</SelectItem><SelectItem value="Delivered">Delivered</SelectItem><SelectItem value="Exception">Exception</SelectItem></SelectContent></Select><Button variant="ghost" size="icon" className="h-8 w-8 text-destructive hover:text-destructive ml-1" onClick={() => handleDelete(d.id)}><Trash2 size={14} /></Button></TableCell>
                </TableRow>
                {expandedId === d.id && d.items && d.items.length > 0 && (
                  <TableRow key={`${d.id}-items`}><TableCell colSpan={7} className="bg-muted/30 p-0">
                    <Table><TableHeader><TableRow><TableHead className="pl-8">Product</TableHead><TableHead>Size</TableHead><TableHead>Color</TableHead><TableHead>Qty Shipped</TableHead><TableHead>Ordered Qty</TableHead></TableRow></TableHeader>
                      <TableBody>{d.items.map((item) => <TableRow key={item.id}><TableCell className="pl-8 font-medium">{item.productName}</TableCell><TableCell>{item.size}</TableCell><TableCell>{item.color}</TableCell><TableCell>{item.quantity}</TableCell><TableCell>{item.orderedQuantity}</TableCell></TableRow>)}</TableBody>
                    </Table>
                  </TableCell></TableRow>
                )}
              </>)}
          </TableBody>
        </Table>
        </div>
      </CardContent></Card>
    </div>
  );
}
