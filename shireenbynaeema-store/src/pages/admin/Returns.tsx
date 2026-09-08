import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { returnApi } from "@/services/api";
import type { Return } from "@/types";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Skeleton } from "@/components/ui/skeleton";
import { Trash2 } from "lucide-react";
import { statusColors } from "@/lib/statusColors";
import toast from "react-hot-toast";

export default function AdminReturns() {
  const [returns, setReturns] = useState<Return[]>([]);
  const [loading, setLoading] = useState(true);
  const [expandedId, setExpandedId] = useState<string | null>(null);
  useEffect(() => { returnApi.list().then((res) => setReturns(res.data.data?.entities || [])).finally(() => setLoading(false)); }, []);

  const handleDelete = async (id: string) => {
    if (!confirm("Delete this return request?")) return;
    await returnApi.delete(id);
    setReturns((prev) => prev.filter((r) => r.id !== id));
    toast.success("Return deleted");
  };

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-semibold tracking-tight">Returns</h1>
      <Card><CardContent className="p-0">
        <div className="overflow-x-auto">
        <Table>
          <TableHeader><TableRow><TableHead>Order</TableHead><TableHead className="hidden sm:table-cell">Reason</TableHead><TableHead className="hidden md:table-cell">Bank</TableHead><TableHead className="hidden sm:table-cell">Items</TableHead><TableHead>Status</TableHead><TableHead className="hidden lg:table-cell">Requested</TableHead><TableHead className="w-[50px]" /></TableRow></TableHeader>
          <TableBody>
            {loading ? Array.from({ length: 5 }).map((_, i) => <TableRow key={i}>{Array.from({ length: 7 }).map((__, j) => <TableCell key={j}><Skeleton className="h-4 w-full" /></TableCell>)}</TableRow>) :
              returns.length === 0 ? <TableRow><TableCell colSpan={7} className="text-center py-8 text-muted-foreground">No return requests</TableCell></TableRow> :
              returns.map((r) => <>
                <TableRow key={r.id} className="cursor-pointer hover:bg-muted/50" onClick={() => setExpandedId(expandedId === r.id ? null : r.id)}>
                  <TableCell className="font-medium"><Link to={`/admin/returns/${r.id}`} className="hover:underline" onClick={(e) => e.stopPropagation()}>{r.orderNumber || "-"}</Link></TableCell>
                  <TableCell className="text-muted-foreground max-w-xs truncate">{r.reason}</TableCell>
                  <TableCell className="text-muted-foreground text-xs">{r.bankName || "-"}</TableCell>
                  <TableCell>{r.items?.length || 0} item(s)</TableCell>
                  <TableCell><Badge className={statusColors[r.status]}>{r.status}</Badge></TableCell>
                  <TableCell className="text-muted-foreground">{r.requestedOn || "-"}</TableCell>
                  <TableCell onClick={(e) => e.stopPropagation()}><Button variant="ghost" size="icon" className="h-8 w-8 text-destructive hover:text-destructive" onClick={() => handleDelete(r.id)}><Trash2 size={14} /></Button></TableCell>
                </TableRow>
                {expandedId === r.id && r.items && r.items.length > 0 && (
                  <TableRow key={`${r.id}-items`}><TableCell colSpan={7} className="bg-muted/30 p-0">
                    <Table><TableHeader><TableRow><TableHead className="pl-8">Product</TableHead><TableHead>Size</TableHead><TableHead>Color</TableHead><TableHead>Qty</TableHead><TableHead>Reason</TableHead></TableRow></TableHeader>
                      <TableBody>{r.items.map((item) => <TableRow key={item.id}><TableCell className="pl-8 font-medium">{item.productName}</TableCell><TableCell>{item.size}</TableCell><TableCell>{item.color}</TableCell><TableCell>{item.quantity}</TableCell><TableCell className="text-muted-foreground">{item.reason}</TableCell></TableRow>)}</TableBody>
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
