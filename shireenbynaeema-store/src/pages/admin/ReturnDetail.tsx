import { useEffect, useState } from "react";
import { useParams, Link, useNavigate } from "react-router-dom";
import { returnApi, paymentApi } from "@/services/api";
import type { Return } from "@/types";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Skeleton } from "@/components/ui/skeleton";
import { statusColors } from "@/lib/statusColors";
import toast from "react-hot-toast";
import { Trash2 } from "lucide-react";

const nextStatuses: Record<string, string[]> = {
  Requested: ["Approved", "Rejected"],
  Approved: ["InCheckup"],
  InCheckup: ["Refunded", "Completed"],
  Refunded: ["Completed"],
};

export default function AdminReturnDetail() {
  const { id } = useParams();
  const [ret, setRet] = useState<Return | null>(null);
  const [loading, setLoading] = useState(true);
  const [refundAmount, setRefundAmount] = useState("");
  const [notes, setNotes] = useState("");
  const [processing, setProcessing] = useState(false);
  const navigate = useNavigate();

  useEffect(() => {
    if (!id) return;
    returnApi.getById(id).then((res) => setRet(res.data.data || null)).finally(() => setLoading(false));
  }, [id]);

  const handleProcess = async (status: string) => {
    if (!id) return;
    setProcessing(true);
    try {
      await returnApi.process(id, {
        status,
        refundAmount: refundAmount ? parseFloat(refundAmount) : undefined,
        notes: notes || undefined,
      });
      toast.success(`Return ${status.toLowerCase()}`);
      setRet((r) => r ? { ...r, status: status as Return["status"], refundAmount: refundAmount ? parseFloat(refundAmount) : r.refundAmount, notes: notes || r.notes } : r);
      setRefundAmount("");
      setNotes("");
    } catch { toast.error("Failed"); }
    setProcessing(false);
  };

  const handleStripeRefund = async () => {
    if (!ret?.refundPaymentId || !ret.refundAmount) { toast.error("No payment intent or refund amount"); return; }
    setProcessing(true);
    try {
      const amountInCents = Math.round(ret.refundAmount * 100);
      await paymentApi.processRefund(ret.refundPaymentId, amountInCents);
      await returnApi.process(ret.id, { status: "Completed", notes: "Refund processed via Stripe" });
      toast.success("Stripe refund processed!");
      setRet((r) => r ? { ...r, status: "Completed" } : r);
    } catch { toast.error("Stripe refund failed"); }
    setProcessing(false);
  };

  const handleDelete = async () => {
    if (!id || !confirm("Delete this return request?")) return;
    await returnApi.delete(id);
    toast.success("Return deleted");
    navigate("/admin/returns");
  };

  if (loading) return <div className="space-y-4">{Array.from({ length: 4 }).map((_, i) => <Skeleton key={i} className="h-16 w-full" />)}</div>;
  if (!ret) return <div className="text-center py-16 text-muted-foreground">Return not found</div>;

  const attachments = (() => { try { return JSON.parse(ret.attachments || "[]") as string[]; } catch { return []; } })();

  return (
    <div className="max-w-[1400px] mx-auto px-4 sm:px-6 lg:px-8 py-8 lg:py-12">
      <div className="flex items-start justify-between mb-8 gap-4 flex-wrap">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">Return Request</h1>
          <p className="text-sm text-muted-foreground">Order: <Link to={`/admin/orders/${ret.orderId}`} className="hover:underline">{ret.orderNumber}</Link></p>
        </div>
        <div className="flex gap-2 flex-wrap">
          <Button variant="outline" size="sm" onClick={handleDelete} className="text-destructive hover:text-destructive"><Trash2 size={14} /></Button>
          <Link to="/admin/returns"><Button variant="outline" size="sm">Back</Button></Link>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-2 space-y-6">
          <Card>
            <CardHeader><CardTitle className="text-base">Returned Items</CardTitle></CardHeader>
            <CardContent className="p-0">
              <Table>
                <TableHeader><TableRow><TableHead>Product</TableHead><TableHead>Size</TableHead><TableHead>Color</TableHead><TableHead>Qty</TableHead><TableHead>Reason</TableHead></TableRow></TableHeader>
                <TableBody>
                  {!ret.items?.length ? <TableRow><TableCell colSpan={5} className="text-center py-8 text-muted-foreground">No items</TableCell></TableRow> :
                    ret.items.map((item) => (
                      <TableRow key={item.id}>
                        <TableCell className="font-medium">{item.productName}</TableCell>
                        <TableCell>{item.size}</TableCell>
                        <TableCell>{item.color}</TableCell>
                        <TableCell>{item.quantity}</TableCell>
                        <TableCell className="text-muted-foreground">{item.reason}</TableCell>
                      </TableRow>
                    ))
                  }
                </TableBody>
              </Table>
            </CardContent>
          </Card>

          {attachments.length > 0 && (
            <Card>
              <CardHeader><CardTitle className="text-base">Attachments</CardTitle></CardHeader>
              <CardContent>
                <div className="flex flex-wrap gap-2">
                  {attachments.map((url) => (
                    <a key={url} href={url} target="_blank" rel="noopener noreferrer">
                      <img src={url} alt="" className="w-24 h-24 object-cover rounded-lg border border-border hover:opacity-80" />
                    </a>
                  ))}
                </div>
              </CardContent>
            </Card>
          )}
        </div>

        <div className="space-y-6">
          <Card><CardContent className="p-6 space-y-3">
            <div className="flex justify-between text-sm"><span className="text-muted-foreground">Status</span><Badge className={statusColors[ret.status]}>{ret.status}</Badge></div>
            <div className="flex justify-between text-sm"><span className="text-muted-foreground">Reason</span><span>{ret.reason}</span></div>
            {ret.requestedOn && <div className="flex justify-between text-sm"><span className="text-muted-foreground">Requested</span><span>{ret.requestedOn}</span></div>}
            {ret.processedOn && <div className="flex justify-between text-sm"><span className="text-muted-foreground">Processed</span><span>{ret.processedOn}</span></div>}
            {ret.refundAmount != null && <div className="flex justify-between text-sm"><span className="text-muted-foreground">Refund Amount</span><span className="font-medium">Rs. {ret.refundAmount.toLocaleString()}</span></div>}
          </CardContent></Card>

          <Card><CardHeader><CardTitle className="text-base">Bank Details</CardTitle></CardHeader><CardContent className="space-y-2">
            <div className="text-sm"><span className="text-muted-foreground">Bank: </span>{ret.bankName || "-"}</div>
            <div className="text-sm"><span className="text-muted-foreground">Account: </span>{ret.bankAccount || "-"}</div>
            <div className="text-sm"><span className="text-muted-foreground">Holder: </span>{ret.accountHolderName || "-"}</div>
          </CardContent></Card>

          {ret.notes && (
            <Card><CardHeader><CardTitle className="text-base">Notes</CardTitle></CardHeader><CardContent>
              <p className="text-sm text-muted-foreground">{ret.notes}</p>
            </CardContent></Card>
          )}

          <Card><CardHeader><CardTitle className="text-base">Process Return</CardTitle></CardHeader><CardContent className="space-y-3">
            {nextStatuses[ret.status]?.map((status) => (
              <div key={status} className="space-y-2">
                {status === "Refunded" && (
                  <Input placeholder="Refund amount (Rs.)" type="number" value={refundAmount} onChange={(e) => setRefundAmount(e.target.value)} className="h-9 text-sm" />
                )}
                {status === "Completed" && (
                  <Input placeholder="Notes / Stripe refund ID" value={notes} onChange={(e) => setNotes(e.target.value)} className="h-9 text-sm" />
                )}
                <Button className="w-full" size="sm" disabled={processing} onClick={() => handleProcess(status)}>
                  {status}
                </Button>
              </div>
            ))}

            {ret.status === "Refunded" && ret.refundPaymentId && (
              <Button variant="outline" className="w-full" size="sm" disabled={processing} onClick={handleStripeRefund}>
                Process Stripe Refund
              </Button>
            )}
          </CardContent></Card>

          <Card><CardContent className="p-6">
            <Link to={`/admin/orders/${ret.orderId}`}><Button variant="outline" className="w-full" size="sm">View Order</Button></Link>
          </CardContent></Card>
        </div>
      </div>
    </div>
  );
}
