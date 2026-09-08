import { useEffect, useState } from "react";
import { couponApi } from "@/services/api";
import type { Coupon } from "@/types";
import { Pencil, Trash2, Plus } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Skeleton } from "@/components/ui/skeleton";
import toast from "react-hot-toast";

export default function AdminCoupons() {
  const [coupons, setCoupons] = useState<Coupon[]>([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState<Coupon | null>(null);
  const [form, setForm] = useState({ code: "", description: "", discountType: "Percentage", discountValue: "", minOrderAmount: "", maxUses: "", expiresOn: "", isActive: true });

  const load = () => couponApi.list().then((res) => setCoupons(res.data.data?.entities || [])).finally(() => setLoading(false));
  useEffect(() => { load(); }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    await couponApi.add({
      id: editing?.id, code: form.code, description: form.description,
      discountType: form.discountType, discountValue: parseFloat(form.discountValue),
      minOrderAmount: form.minOrderAmount ? parseFloat(form.minOrderAmount) : null,
      maxUses: form.maxUses ? parseInt(form.maxUses) : null,
      expiresOn: form.expiresOn ? new Date(form.expiresOn).toISOString() : null,
      isActive: form.isActive,
    });
    toast.success(editing ? "Coupon updated" : "Coupon created");
    setShowForm(false); setEditing(null);
    setForm({ code: "", description: "", discountType: "Percentage", discountValue: "", minOrderAmount: "", maxUses: "", expiresOn: "", isActive: true });
    load();
  };

  const handleEdit = (c: Coupon) => {
    setEditing(c);
    setForm({ code: c.code, description: c.description, discountType: c.discountType, discountValue: String(c.discountValue), minOrderAmount: c.minOrderAmount ? String(c.minOrderAmount) : "", maxUses: c.maxUses ? String(c.maxUses) : "", expiresOn: c.expiresOn ? new Date(c.expiresOn).toISOString().split("T")[0] : "", isActive: c.isActive });
    setShowForm(true);
  };

  const handleDelete = async (id: string) => {
    if (!confirm("Delete this coupon?")) return;
    await couponApi.delete(id);
    setCoupons((prev) => prev.filter((c) => c.id !== id));
    toast.success("Coupon deleted");
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between flex-wrap gap-3">
        <h1 className="text-2xl font-semibold tracking-tight">Coupons</h1>
        <Button size="sm" onClick={() => { setShowForm(!showForm); setEditing(null); setForm({ code: "", description: "", discountType: "Percentage", discountValue: "", minOrderAmount: "", maxUses: "", expiresOn: "", isActive: true }); }}><Plus size={14} className="mr-1" />{showForm ? "Cancel" : "New Coupon"}</Button>
      </div>

      {showForm && (
        <Card><CardHeader><CardTitle className="text-base">{editing ? "Edit" : "New"} Coupon</CardTitle></CardHeader><CardContent>
          <form onSubmit={handleSubmit} className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div><Label>Code</Label><Input value={form.code} onChange={(e) => setForm({ ...form, code: e.target.value.toUpperCase() })} required /></div>
            <div><Label>Description</Label><Input value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} /></div>
            <div><Label>Discount Type</Label><Select value={form.discountType} onValueChange={(v) => setForm({ ...form, discountType: v })}><SelectTrigger><SelectValue /></SelectTrigger><SelectContent><SelectItem value="Percentage">Percentage (%)</SelectItem><SelectItem value="Fixed">Fixed (Rs.)</SelectItem></SelectContent></Select></div>
            <div><Label>Discount Value</Label><Input type="number" value={form.discountValue} onChange={(e) => setForm({ ...form, discountValue: e.target.value })} required /></div>
            <div><Label>Min Order Amount</Label><Input type="number" value={form.minOrderAmount} onChange={(e) => setForm({ ...form, minOrderAmount: e.target.value })} /></div>
            <div><Label>Max Uses</Label><Input type="number" value={form.maxUses} onChange={(e) => setForm({ ...form, maxUses: e.target.value })} /></div>
            <div><Label>Expires On</Label><Input type="date" value={form.expiresOn} onChange={(e) => setForm({ ...form, expiresOn: e.target.value })} /></div>
            <div className="flex items-end"><Button type="submit" className="w-full">{editing ? "Update" : "Create"}</Button></div>
          </form>
        </CardContent></Card>
      )}

      <Card><CardContent className="p-0">
        <div className="overflow-x-auto">
        <Table>
          <TableHeader><TableRow><TableHead>Code</TableHead><TableHead>Discount</TableHead><TableHead className="hidden sm:table-cell">Min Order</TableHead><TableHead className="hidden sm:table-cell">Uses</TableHead><TableHead className="hidden md:table-cell">Expires</TableHead><TableHead>Status</TableHead><TableHead className="w-[80px]" /></TableRow></TableHeader>
          <TableBody>
            {loading ? Array.from({ length: 3 }).map((_, i) => <TableRow key={i}>{Array.from({ length: 7 }).map((__, j) => <TableCell key={j}><Skeleton className="h-4 w-full" /></TableCell>)}</TableRow>) :
              coupons.length === 0 ? <TableRow><TableCell colSpan={7} className="text-center py-8 text-muted-foreground">No coupons yet</TableCell></TableRow> :
              coupons.map((c) => <TableRow key={c.id}><TableCell className="font-mono font-medium">{c.code}</TableCell><TableCell>{c.discountType === "Percentage" ? `${c.discountValue}%` : `Rs. ${c.discountValue}`}</TableCell><TableCell>{c.minOrderAmount ? `Rs. ${c.minOrderAmount}` : "—"}</TableCell><TableCell>{c.usedCount}{c.maxUses ? `/${c.maxUses}` : ""}</TableCell><TableCell className="text-muted-foreground">{c.expiresOn ? new Date(c.expiresOn).toLocaleDateString() : "Never"}</TableCell><TableCell><Badge variant={c.isActive ? "default" : "secondary"}>{c.isActive ? "Active" : "Inactive"}</Badge></TableCell><TableCell><Button variant="ghost" size="icon" className="h-8 w-8" onClick={() => handleEdit(c)}><Pencil size={14} /></Button><Button variant="ghost" size="icon" className="h-8 w-8 text-destructive" onClick={() => handleDelete(c.id)}><Trash2 size={14} /></Button></TableCell></TableRow>)}
          </TableBody>
        </Table>
        </div>
      </CardContent></Card>
    </div>
  );
}
