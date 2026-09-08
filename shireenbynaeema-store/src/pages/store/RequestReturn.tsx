import { useState, useRef, useEffect } from "react";
import { useParams, useNavigate, Link } from "react-router-dom";
import { returnApi, orderApi } from "@/services/api";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import toast from "react-hot-toast";
import { Upload, X } from "lucide-react";

export default function RequestReturn() {
  const { orderId } = useParams<{ orderId: string }>();
  const navigate = useNavigate();
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [loading, setLoading] = useState(false);
  const [uploading, setUploading] = useState(false);
  const [reason, setReason] = useState("");
  const [bankAccount, setBankAccount] = useState("");
  const [bankName, setBankName] = useState("");
  const [accountHolder, setAccountHolder] = useState("");
  const [selectedItems, setSelectedItems] = useState<Record<string, { quantity: number; reason: string }>>({});
  const [attachments, setAttachments] = useState<string[]>([]);
  const [order, setOrder] = useState<any>(null);

  useEffect(() => {
    if (orderId) {
      orderApi.getById(orderId).then((res) => setOrder(res.data.data || null));
    }
  }, [orderId]);

  const handleFileUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = e.target.files;
    if (!files) return;
    setUploading(true);
    try {
      for (const file of Array.from(files)) {
        const res = await returnApi.uploadAttachment(file);
        if (res.data.isSuccess) setAttachments((prev) => [...prev, res.data.data.url]);
      }
    } catch { toast.error("Upload failed"); }
    setUploading(false);
    if (fileInputRef.current) fileInputRef.current.value = "";
  };

  const removeAttachment = (url: string) => setAttachments((prev) => prev.filter((a) => a !== url));

  const toggleItem = (orderItemId: string) => {
    setSelectedItems((prev) => {
      const copy = { ...prev };
      if (copy[orderItemId]) delete copy[orderItemId];
      else copy[orderItemId] = { quantity: 1, reason: "" };
      return copy;
    });
  };

  const updateItemQty = (orderItemId: string, qty: number) => {
    setSelectedItems((prev) => ({ ...prev, [orderItemId]: { ...prev[orderItemId], quantity: qty } }));
  };

  const handleSubmit = async () => {
    if (!reason.trim()) { toast.error("Please enter a reason"); return; }
    if (!bankAccount.trim()) { toast.error("Please enter bank account number"); return; }
    if (!bankName.trim()) { toast.error("Please enter bank name"); return; }
    if (!accountHolder.trim()) { toast.error("Please enter account holder name"); return; }

    setLoading(true);
    try {
      const items = Object.entries(selectedItems).map(([orderItemId, val]) => ({
        orderItemId,
        quantity: val.quantity,
        reason: val.reason || reason,
      }));
      await returnApi.request({
        orderId: orderId!,
        reason,
        bankAccount,
        bankName,
        accountHolderName: accountHolder,
        attachments: attachments.length > 0 ? JSON.stringify(attachments) : undefined,
        items: items.length > 0 ? items : undefined,
      });
      toast.success("Return request submitted!");
      navigate("/account/returns");
    } catch { toast.error("Failed to submit return request"); }
    setLoading(false);
  };

  return (
    <div className="max-w-[800px] mx-auto px-4 py-8 lg:py-12">
      <div className="mb-6">
        <Link to="/account/orders" className="text-sm text-muted-foreground hover:text-foreground">&larr; Back to orders</Link>
        <h1 className="text-2xl font-light mt-2" style={{ fontFamily: "Georgia, serif" }}>Request Return</h1>
        {order && <p className="text-sm text-muted-foreground mt-1">Order: {order.orderNumber}</p>}
      </div>

      <div className="space-y-6">
        {/* Items to return */}
        {order?.items && (
          <Card>
            <CardHeader><CardTitle className="text-base">Select items to return</CardTitle></CardHeader>
            <CardContent className="space-y-3">
              {order.items.map((item: any) => (
                <div key={item.id} className="flex items-center gap-3 p-3 rounded-lg border border-border">
                  <input
                    type="checkbox"
                    checked={!!selectedItems[item.id]}
                    onChange={() => toggleItem(item.id)}
                    className="w-4 h-4 accent-foreground"
                  />
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-medium truncate">{item.productName}</p>
                    <p className="text-xs text-muted-foreground">{item.size} / {item.color} — Rs. {item.unitPrice.toLocaleString()}</p>
                  </div>
                  {selectedItems[item.id] && (
                    <input
                      type="number"
                      min={1}
                      max={item.quantity}
                      value={selectedItems[item.id].quantity}
                      onChange={(e) => updateItemQty(item.id, parseInt(e.target.value) || 1)}
                      className="w-16 h-8 text-sm text-center border border-border rounded"
                    />
                  )}
                </div>
              ))}
              {order.items.length === 0 && <p className="text-sm text-muted-foreground text-center py-4">No items in this order</p>}
            </CardContent>
          </Card>
        )}

        {/* Reason */}
        <Card>
          <CardHeader><CardTitle className="text-base">Reason for return</CardTitle></CardHeader>
          <CardContent>
            <textarea
              value={reason}
              onChange={(e) => setReason(e.target.value)}
              placeholder="Describe the reason for your return..."
              className="w-full h-24 text-sm border border-border rounded-lg p-3 resize-none focus:outline-none focus:ring-1 focus:ring-foreground"
            />
          </CardContent>
        </Card>

        {/* Bank details */}
        <Card>
          <CardHeader><CardTitle className="text-base">Refund bank details</CardTitle></CardHeader>
          <CardContent className="space-y-3">
            <p className="text-xs text-muted-foreground mb-2">Provide a one-time bank account for refund. This is not saved after processing.</p>
            <Input placeholder="Bank account number (IBAN)" value={bankAccount} onChange={(e) => setBankAccount(e.target.value)}
              className="h-10 text-sm" />
            <Input placeholder="Bank name" value={bankName} onChange={(e) => setBankName(e.target.value)}
              className="h-10 text-sm" />
            <Input placeholder="Account holder name" value={accountHolder} onChange={(e) => setAccountHolder(e.target.value)}
              className="h-10 text-sm" />
          </CardContent>
        </Card>

        {/* Attachments */}
        <Card>
          <CardHeader><CardTitle className="text-base">Attachments (optional)</CardTitle></CardHeader>
          <CardContent>
            <p className="text-xs text-muted-foreground mb-3">Upload photos as reference for your return.</p>
            <div className="flex flex-wrap gap-2 mb-3">
              {attachments.map((url) => (
                <div key={url} className="relative w-20 h-20 rounded-lg overflow-hidden border border-border group">
                  <img src={url} alt="" className="w-full h-full object-cover" />
                  <button onClick={() => removeAttachment(url)}
                    className="absolute top-1 right-1 w-5 h-5 bg-black/60 rounded-full flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity">
                    <X size={12} className="text-white" />
                  </button>
                </div>
              ))}
            </div>
            <input ref={fileInputRef} type="file" accept="image/*" multiple onChange={handleFileUpload} className="hidden" />
            <Button variant="outline" size="sm" onClick={() => fileInputRef.current?.click()} disabled={uploading}>
              <Upload size={14} className="mr-1.5" />
              {uploading ? "Uploading..." : "Upload images"}
            </Button>
          </CardContent>
        </Card>

        {/* Submit */}
        <Button onClick={handleSubmit} disabled={loading || !reason.trim() || !bankAccount.trim()}
          className="w-full h-12 text-sm font-medium">
          {loading ? "Submitting..." : "Submit return request"}
        </Button>
      </div>
    </div>
  );
}
