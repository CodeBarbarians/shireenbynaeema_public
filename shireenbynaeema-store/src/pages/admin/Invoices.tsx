import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { invoiceApi } from "@/services/api";
import type { Invoice } from "@/types";
import { Check, FileText, Trash2 } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Skeleton } from "@/components/ui/skeleton";
import toast from "react-hot-toast";
import { statusColors } from "@/lib/statusColors";

export default function AdminInvoices() {
  const [invoices, setInvoices] = useState<Invoice[]>([]);
  const [loading, setLoading] = useState(true);
  useEffect(() => { invoiceApi.list().then((res) => setInvoices(res.data.data?.entities || [])).finally(() => setLoading(false)); }, []);

  const handleMarkPaid = async (id: string) => {
    try { await invoiceApi.markPaid(id, "manual"); toast.success("Marked as paid"); setInvoices(invoices.map((inv) => inv.id === id ? { ...inv, status: "Paid" as const } : inv)); } catch { toast.error("Failed"); }
  };

  const handleDelete = async (id: string) => {
    if (!confirm("Delete this invoice?")) return;
    await invoiceApi.delete(id);
    setInvoices((prev) => prev.filter((inv) => inv.id !== id));
    toast.success("Invoice deleted");
  };

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-semibold tracking-tight">Invoices</h1>
      <Card><CardContent className="p-0">
        <div className="overflow-x-auto">
        <Table>
          <TableHeader><TableRow><TableHead>Invoice #</TableHead><TableHead className="hidden sm:table-cell">Order</TableHead><TableHead>Amount</TableHead><TableHead>Status</TableHead><TableHead className="hidden md:table-cell">Date</TableHead><TableHead className="text-right">Actions</TableHead></TableRow></TableHeader>
          <TableBody>
            {loading ? Array.from({ length: 5 }).map((_, i) => <TableRow key={i}>{Array.from({ length: 6 }).map((__, j) => <TableCell key={j}><Skeleton className="h-4 w-full" /></TableCell>)}</TableRow>) :
              invoices.length === 0 ? <TableRow><TableCell colSpan={6} className="text-center py-8 text-muted-foreground">No invoices yet</TableCell></TableRow> :
              invoices.map((inv) => <TableRow key={inv.id} className="hover:bg-muted/50"><TableCell className="font-medium"><Link to={`/admin/invoices/${inv.id}`} className="hover:underline flex items-center gap-2"><FileText size={14} className="text-muted-foreground" />{inv.invoiceNumber}</Link></TableCell><TableCell className="hidden sm:table-cell text-muted-foreground">{inv.orderNumber || "-"}</TableCell><TableCell>Rs. {inv.amount.toLocaleString()}</TableCell><TableCell><Badge className={statusColors[inv.status]}>{inv.status}</Badge></TableCell><TableCell className="hidden md:table-cell text-muted-foreground">{inv.createdOn ? new Date(inv.createdOn * 1000).toLocaleDateString() : "-"}</TableCell><TableCell className="text-right" onClick={(e) => e.stopPropagation()}>{inv.status === "Pending" && <Button variant="ghost" size="icon" className="h-8 w-8 text-green-600" onClick={() => handleMarkPaid(inv.id)}><Check size={14} /></Button>}<Button variant="ghost" size="icon" className="h-8 w-8 text-destructive hover:text-destructive" onClick={() => handleDelete(inv.id)}><Trash2 size={14} /></Button></TableCell></TableRow>)}
          </TableBody>
        </Table>
        </div>
      </CardContent></Card>
    </div>
  );
}
