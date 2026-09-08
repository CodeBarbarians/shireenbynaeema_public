import { useEffect, useState } from "react";
import { useParams, Link, useNavigate } from "react-router-dom";
import { invoiceApi } from "@/services/api";
import type { Invoice } from "@/types";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import toast from "react-hot-toast";
import { statusColors } from "@/lib/statusColors";
import { Trash2 } from "lucide-react";

export default function AdminInvoiceDetail() {
  const { id } = useParams();
  const [invoice, setInvoice] = useState<Invoice | null>(null);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  useEffect(() => {
    if (!id) return;
    invoiceApi.getById(id).then((res) => setInvoice(res.data.data || null)).finally(() => setLoading(false));
  }, [id]);

  const handleMarkPaid = async () => {
    if (!id) return;
    try { await invoiceApi.markPaid(id, "manual"); toast.success("Marked as paid"); setInvoice((inv) => inv ? { ...inv, status: "Paid" as const } : inv); } catch { toast.error("Failed"); }
  };

  const handleDelete = async () => {
    if (!id || !confirm("Delete this invoice?")) return;
    await invoiceApi.delete(id);
    toast.success("Invoice deleted");
    navigate("/admin/invoices");
  };

  if (loading) return <div className="space-y-4">{Array.from({ length: 4 }).map((_, i) => <Skeleton key={i} className="h-16 w-full" />)}</div>;
  if (!invoice) return <div className="text-center py-16 text-muted-foreground">Invoice not found</div>;

  return (
    <div className="max-w-[1400px] mx-auto px-4 sm:px-6 lg:px-8 py-8 lg:py-12">
      <div className="flex items-start justify-between mb-8 gap-4 flex-wrap">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">Invoice {invoice.invoiceNumber}</h1>
          <p className="text-sm text-muted-foreground">{invoice.createdOn ? new Date(invoice.createdOn * 1000).toLocaleString() : ""}</p>
        </div>
        <div className="flex gap-2 flex-wrap">
          {invoice.status === "Pending" && <Button size="sm" onClick={handleMarkPaid}>Mark as Paid</Button>}
          <Button variant="outline" size="sm" onClick={handleDelete} className="text-destructive hover:text-destructive"><Trash2 size={14} /></Button>
          <Link to="/admin/invoices"><Button variant="outline" size="sm">Back</Button></Link>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-2 space-y-6">
          <Card>
            <CardHeader><CardTitle className="text-base">Invoice Details</CardTitle></CardHeader>
            <CardContent className="space-y-4">
              <div className="grid grid-cols-2 gap-4 text-sm">
                <div><span className="text-muted-foreground">Invoice #:</span> <span className="ml-2 font-medium">{invoice.invoiceNumber}</span></div>
                <div><span className="text-muted-foreground">Order #:</span> <span className="ml-2"><Link to={`/admin/orders/${invoice.orderId}`} className="hover:underline">{invoice.orderNumber}</Link></span></div>
                <div><span className="text-muted-foreground">Status:</span> <span className="ml-2"><Badge className={statusColors[invoice.status]}>{invoice.status}</Badge></span></div>
                <div><span className="text-muted-foreground">Amount:</span> <span className="ml-2 font-medium">Rs. {invoice.amount.toLocaleString()}</span></div>
                {invoice.dueDate && <div><span className="text-muted-foreground">Due Date:</span> <span className="ml-2">{new Date(invoice.dueDate).toLocaleDateString()}</span></div>}
                {invoice.paidDate && <div><span className="text-muted-foreground">Paid Date:</span> <span className="ml-2">{new Date(invoice.paidDate).toLocaleDateString()}</span></div>}
              </div>
            </CardContent>
          </Card>
        </div>

        <div className="space-y-6">
          <Card><CardContent className="p-6 space-y-3">
            <div className="flex justify-between text-sm"><span className="text-muted-foreground">Subtotal</span><span>Rs. {(invoice.amount - invoice.tax).toLocaleString()}</span></div>
            <div className="flex justify-between text-sm"><span className="text-muted-foreground">Tax</span><span>Rs. {invoice.tax.toLocaleString()}</span></div>
            <div className="border-t border-border pt-2 flex justify-between font-medium"><span>Total</span><span>Rs. {invoice.amount.toLocaleString()} PKR</span></div>
          </CardContent></Card>

          <Card><CardHeader><CardTitle className="text-base">Actions</CardTitle></CardHeader><CardContent className="space-y-2">
            <Link to={`/admin/orders/${invoice.orderId}`}><Button variant="outline" className="w-full" size="sm">View Order</Button></Link>
            {invoice.status === "Pending" && <Button className="w-full" size="sm" onClick={handleMarkPaid}>Mark as Paid</Button>}
          </CardContent></Card>
        </div>
      </div>
    </div>
  );
}
