import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { orderApi } from "@/services/api";
import type { Order } from "@/types";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Skeleton } from "@/components/ui/skeleton";
import { Trash2 } from "lucide-react";
import { statusColors } from "@/lib/statusColors";
import toast from "react-hot-toast";

export default function AdminOrders() {
  const [orders, setOrders] = useState<Order[]>([]);
  const [loading, setLoading] = useState(true);
  useEffect(() => { orderApi.list().then((res) => setOrders(res.data.data?.entities || [])).finally(() => setLoading(false)); }, []);

  const handleDelete = async (id: string) => {
    if (!confirm("Delete this order and all related data (invoices, deliveries, returns)?")) return;
    await orderApi.delete(id);
    setOrders((prev) => prev.filter((o) => o.id !== id));
    toast.success("Order deleted");
  };

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-semibold tracking-tight">Orders</h1>
      <Card><CardContent className="p-0">
        <div className="overflow-x-auto">
        <Table>
          <TableHeader><TableRow><TableHead>Order #</TableHead><TableHead className="hidden sm:table-cell">Customer</TableHead><TableHead className="hidden md:table-cell">Date</TableHead><TableHead>Total</TableHead><TableHead>Status</TableHead><TableHead className="w-[50px]" /></TableRow></TableHeader>
          <TableBody>
            {loading ? Array.from({ length: 5 }).map((_, i) => <TableRow key={i}>{Array.from({ length: 6 }).map((__, j) => <TableCell key={j}><Skeleton className="h-4 w-full" /></TableCell>)}</TableRow>) :
              orders.length === 0 ? <TableRow><TableCell colSpan={6} className="text-center py-8 text-muted-foreground">No orders yet</TableCell></TableRow> :
              orders.map((o) => <TableRow key={o.id} className="hover:bg-muted/50"><TableCell className="font-medium"><Link to={`/admin/orders/${o.id}`} className="hover:underline">{o.orderNumber}</Link><span className="block text-xs text-muted-foreground sm:hidden">{o.customerName || o.customerEmail || ""}</span></TableCell><TableCell className="hidden sm:table-cell text-muted-foreground">{o.customerName || o.customerEmail || "-"}</TableCell><TableCell className="hidden md:table-cell text-muted-foreground">{o.createdOn ? new Date(o.createdOn * 1000).toLocaleDateString() : "-"}</TableCell><TableCell>Rs. {o.total.toLocaleString()}</TableCell><TableCell><Badge className={statusColors[o.status]}>{o.status}</Badge></TableCell><TableCell><Button variant="ghost" size="icon" className="h-8 w-8 text-destructive hover:text-destructive" onClick={() => handleDelete(o.id)}><Trash2 size={14} /></Button></TableCell></TableRow>)}
          </TableBody>
        </Table>
        </div>
      </CardContent></Card>
    </div>
  );
}
