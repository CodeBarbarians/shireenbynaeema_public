import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { productApi } from "@/services/api";
import type { Product } from "@/types";
import { Plus, Pencil, Trash2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent } from "@/components/ui/card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { resolveImageUrl } from "@/lib/imageUtils";
import { Skeleton } from "@/components/ui/skeleton";
import toast from "react-hot-toast";

export default function AdminProducts() {
  const [products, setProducts] = useState<Product[]>([]);
  const [search, setSearch] = useState("");
  const [loading, setLoading] = useState(true);

  const fetchProducts = async () => {
    setLoading(true);
    try { const res = await productApi.list({ search }); setProducts(res.data.data?.entities || []); } finally { setLoading(false); }
  };

  useEffect(() => { fetchProducts(); }, []);

  const handleDelete = async (id: string) => {
    if (!confirm("Delete this product?")) return;
    try { await productApi.delete(id); toast.success("Product deleted"); fetchProducts(); } catch { toast.error("Failed to delete"); }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between flex-wrap gap-3">
        <h1 className="text-2xl font-semibold tracking-tight">Products</h1>
        <Link to="/admin/products/new"><Button><Plus size={16} className="mr-2" /> Add Product</Button></Link>
      </div>
      <Card><CardContent className="p-4"><Input placeholder="Search products..." value={search} onChange={(e) => setSearch(e.target.value)} onKeyDown={(e) => e.key === "Enter" && fetchProducts()} className="max-w-sm" /></CardContent></Card>
      <Card>
        <CardContent className="p-0">
          <div className="overflow-x-auto">
          <Table>
            <TableHeader><TableRow><TableHead>Product</TableHead><TableHead className="hidden sm:table-cell">SKU</TableHead><TableHead>Price</TableHead><TableHead className="hidden md:table-cell">Stock</TableHead><TableHead className="hidden lg:table-cell">Booked</TableHead><TableHead className="hidden lg:table-cell">Available</TableHead><TableHead>Status</TableHead><TableHead className="text-right">Actions</TableHead></TableRow></TableHeader>
            <TableBody>
              {loading ? Array.from({ length: 5 }).map((_, i) => <TableRow key={i}>{Array.from({ length: 8 }).map((__, j) => <TableCell key={j}><Skeleton className="h-10 w-full" /></TableCell>)}</TableRow>) :
                products.length === 0 ? <TableRow><TableCell colSpan={8} className="text-center py-8 text-muted-foreground">No products found</TableCell></TableRow> :
                products.map((p) => {
                  const totalStock = p.variants?.reduce((s, v) => s + v.stock, 0) || 0;
                  const totalBooked = p.variants?.reduce((s, v) => s + (v.bookedStock || 0), 0) || 0;
                  const available = totalStock - totalBooked;
                  return (
                  <TableRow key={p.id}>
                    <TableCell><div className="flex items-center gap-3">{p.images?.[0] && <img src={resolveImageUrl(p.images[0].imageUrl)} alt="" className="w-10 h-10 object-cover rounded" />}<div className="min-w-0"><span className="font-medium block truncate max-w-[140px] sm:max-w-none">{p.name}</span><span className="text-xs text-muted-foreground sm:hidden">{p.sku}</span></div></div></TableCell>
                    <TableCell className="hidden sm:table-cell text-muted-foreground font-mono text-xs">{p.sku}</TableCell>
                    <TableCell>Rs. {p.basePrice.toLocaleString()}</TableCell>
                    <TableCell className="hidden md:table-cell">{totalStock}</TableCell>
                    <TableCell className="hidden lg:table-cell">{totalBooked > 0 ? <span className="text-amber-600">{totalBooked}</span> : "0"}</TableCell>
                    <TableCell className="hidden lg:table-cell">{available <= 0 ? <span className="text-destructive">0</span> : available}</TableCell>
                    <TableCell><Badge variant={p.isActive ? "default" : "secondary"}>{p.isActive ? "Active" : "Inactive"}</Badge></TableCell>
                    <TableCell className="text-right"><div className="flex items-center justify-end gap-1"><Link to={`/admin/products/${p.id}/edit`}><Button variant="ghost" size="icon" className="h-8 w-8"><Pencil size={14} /></Button></Link><Button variant="ghost" size="icon" className="h-8 w-8 text-destructive" onClick={() => handleDelete(p.id)}><Trash2 size={14} /></Button></div></TableCell>
                  </TableRow>
                  );
                })}
            </TableBody>
          </Table>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
