import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { productApi, categoryApi, imageApi } from "@/services/api";
import type { Category, SizeChartEntry, ProductImage } from "@/types";
import { Trash2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Checkbox } from "@/components/ui/checkbox";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import toast from "react-hot-toast";
import { resolveImageUrl } from "@/lib/imageUtils";

export default function ProductForm() {
  const { id } = useParams();
  const isEdit = Boolean(id);
  const navigate = useNavigate();
  const [categories, setCategories] = useState<Category[]>([]);
  const [saving, setSaving] = useState(false);
  const [form, setForm] = useState({ name: "", slug: "", description: "", sku: "", basePrice: "", salePrice: "", categoryId: "", brand: "", isFeatured: false, isActive: true });
  const [variants, setVariants] = useState([{ id: "", size: "", color: "", colorHex: "#000000", stock: "", price: "", sku: "" }]);
  const [images, setImages] = useState<File[]>([]);
  const handleDeleteImage = async (imageId: string) => {
    if (!confirm("Delete this image?")) return;
    await imageApi.deleteProductImage(imageId);
    setExistingImages((prev) => prev.filter((img) => img.id !== imageId));
    toast.success("Image deleted");
  };

  const [existingImages, setExistingImages] = useState<ProductImage[]>([]);
  const [sizeChart, setSizeChart] = useState<SizeChartEntry[]>([]);

  useEffect(() => { categoryApi.list().then((res) => setCategories(res.data.data?.entities || [])); }, []);

  useEffect(() => {
    if (!id) return;
    productApi.getById(id).then((res) => {
      const p = res.data.data;
      setForm({
        name: p.name || "", slug: p.slug || "", description: p.description || "",
        sku: p.sku || "", basePrice: String(p.basePrice ?? ""), salePrice: p.salePrice ? String(p.salePrice) : "",
        categoryId: p.categoryId || "", brand: p.brand || "", isFeatured: p.isFeatured ?? false, isActive: p.isActive ?? true,
      });
      if (p.variants?.length > 0) {
        setVariants(p.variants.map((v: { id?: string; size: string; color: string; colorHex: string; stock: number; price: number; sku: string }) => ({
          id: v.id || "", size: v.size, color: v.color, colorHex: v.colorHex || "#000000",
          stock: String(v.stock), price: String(v.price), sku: v.sku,
        })));
      }
      try { setSizeChart(JSON.parse(p.sizeChartJson || "[]")); } catch { setSizeChart([]); }
      if (p.images?.length > 0) setExistingImages(p.images);
    });
  }, [id]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => setForm({ ...form, [e.target.name]: e.target.value });
  const handleVariantChange = (i: number, field: string, value: string) => { const u = [...variants]; (u[i] as Record<string, string>)[field] = value; setVariants(u); };
  const addVariant = () => setVariants([...variants, { id: "", size: "", color: "", colorHex: "#000000", stock: "", price: "", sku: "" }]);
  const removeVariant = (i: number) => setVariants(variants.filter((_, idx) => idx !== i));
  const handleSizeChartChange = (i: number, field: string, value: string) => { const u = [...sizeChart]; (u[i] as unknown as Record<string, string>)[field] = value; setSizeChart(u); };
  const addSizeChartRow = () => setSizeChart([...sizeChart, { size: "", bust: "", waist: "", hips: "", length: "" }]);
  const removeSizeChartRow = (i: number) => setSizeChart(sizeChart.filter((_, idx) => idx !== i));

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    try {
      const payload = {
        ...form, id: isEdit ? id : undefined,
        basePrice: parseFloat(form.basePrice),
        salePrice: form.salePrice ? parseFloat(form.salePrice) : null,
        sizeChartJson: JSON.stringify(sizeChart),
        variants: variants.map((v) => ({
          id: v.id || undefined, size: v.size, color: v.color, colorHex: v.colorHex,
          stock: parseInt(v.stock), price: parseFloat(v.price), sku: v.sku,
        })),
      };
      const res = isEdit ? await productApi.update(payload) : await productApi.create(payload);
      const productId = isEdit ? id : res.data.data;
      for (const img of images) await imageApi.upload(img, productId);
      toast.success(isEdit ? "Product updated" : "Product created");
      navigate("/admin/products");
    } catch { toast.error("Failed"); } finally { setSaving(false); }
  };

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-semibold tracking-tight">{isEdit ? "Edit Product" : "Add Product"}</h1>
      <form onSubmit={handleSubmit} className="space-y-6">
        <Card><CardHeader><CardTitle className="text-base">Basic Info</CardTitle></CardHeader><CardContent className="space-y-4">
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div className="sm:col-span-2"><Label className="mb-1.5 block">Product Name</Label><Input name="name" value={form.name} onChange={handleChange} required /></div>
            <div><Label className="mb-1.5 block">SKU</Label><Input name="sku" value={form.sku} onChange={handleChange} required /></div>
            <div><Label className="mb-1.5 block">Brand</Label><Input name="brand" value={form.brand} onChange={handleChange} /></div>
            <div><Label className="mb-1.5 block">Category</Label><Select value={form.categoryId} onValueChange={(v) => setForm({ ...form, categoryId: v })}><SelectTrigger className="w-full"><SelectValue placeholder="Select" /></SelectTrigger><SelectContent>{categories.map((c) => <SelectItem key={c.id} value={c.id}>{c.name}</SelectItem>)}</SelectContent></Select></div>
            <div><Label className="mb-1.5 block">Base Price (PKR)</Label><Input name="basePrice" type="number" value={form.basePrice} onChange={handleChange} required /></div>
            <div><Label className="mb-1.5 block">Sale Price (PKR)</Label><Input name="salePrice" type="number" value={form.salePrice} onChange={handleChange} /></div>
            <div className="sm:col-span-2"><Label className="mb-1.5 block">Description</Label><Textarea name="description" value={form.description} onChange={handleChange} rows={4} /></div>
          </div>
          <div className="flex items-center gap-6">
            <label className="flex items-center gap-2 text-sm"><Checkbox checked={form.isFeatured} onCheckedChange={(v) => setForm({ ...form, isFeatured: v === true })} /> Featured</label>
            <label className="flex items-center gap-2 text-sm"><Checkbox checked={form.isActive} onCheckedChange={(v) => setForm({ ...form, isActive: v === true })} /> Active</label>
          </div>
        </CardContent></Card>
        <Card><CardHeader className="flex flex-row items-center justify-between"><CardTitle className="text-base">Variants</CardTitle><Button type="button" variant="ghost" size="sm" onClick={addVariant}>+ Add Variant</Button></CardHeader><CardContent className="space-y-3">
          {variants.map((v, i) => (
            <div key={i} className="flex items-center gap-2 flex-wrap">
              <Select value={v.size} onValueChange={(val) => handleVariantChange(i, "size", val)}><SelectTrigger className="w-20 shrink-0"><SelectValue placeholder="Size" /></SelectTrigger><SelectContent>{["XS", "S", "M", "L", "XL", "XXL"].map((s) => <SelectItem key={s} value={s}>{s}</SelectItem>)}</SelectContent></Select>
              <Input value={v.color} onChange={(e) => handleVariantChange(i, "color", e.target.value)} placeholder="Color" className="w-24 sm:w-28" />
              <input type="color" value={v.colorHex} onChange={(e) => handleVariantChange(i, "colorHex", e.target.value)} className="w-8 h-10 border rounded cursor-pointer shrink-0" />
              <Input type="number" value={v.stock} onChange={(e) => handleVariantChange(i, "stock", e.target.value)} placeholder="Stock" className="w-16 sm:w-20" />
              <Input type="number" value={v.price} onChange={(e) => handleVariantChange(i, "price", e.target.value)} placeholder="Price" className="w-20 sm:w-24" />
              <Input value={v.sku} onChange={(e) => handleVariantChange(i, "sku", e.target.value)} placeholder="SKU" className="w-24 sm:w-28" />
              {variants.length > 1 && <Button type="button" variant="ghost" size="sm" onClick={() => removeVariant(i)} className="text-destructive">Remove</Button>}
            </div>
          ))}
        </CardContent></Card>
        <Card><CardHeader className="flex flex-row items-center justify-between"><CardTitle className="text-base">Size Chart</CardTitle><Button type="button" variant="ghost" size="sm" onClick={addSizeChartRow}>+ Add Size</Button></CardHeader><CardContent>
          {sizeChart.length > 0 ? (
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead><tr className="border-b border-border">
                  <th className="text-left py-2 pr-3 text-xs font-medium tracking-wider uppercase">Size</th>
                  <th className="text-left py-2 px-3 text-xs font-medium tracking-wider uppercase">Bust</th>
                  <th className="text-left py-2 px-3 text-xs font-medium tracking-wider uppercase">Waist</th>
                  <th className="text-left py-2 px-3 text-xs font-medium tracking-wider uppercase">Hips</th>
                  <th className="text-left py-2 px-3 text-xs font-medium tracking-wider uppercase">Length</th>
                  <th className="py-2"></th>
                </tr></thead>
                <tbody>{sizeChart.map((row, i) => (
                  <tr key={i} className="border-b border-border">
                    <td className="py-1 pr-3"><Select value={row.size} onValueChange={(val) => handleSizeChartChange(i, "size", val)}><SelectTrigger className="h-8 w-16"><SelectValue placeholder="Size" /></SelectTrigger><SelectContent>{["XS", "S", "M", "L", "XL", "XXL"].map((s) => <SelectItem key={s} value={s}>{s}</SelectItem>)}</SelectContent></Select></td>
                    <td className="py-1 px-3"><Input value={row.bust} onChange={(e) => handleSizeChartChange(i, "bust", e.target.value)} placeholder="34" className="h-8 w-16" /></td>
                    <td className="py-1 px-3"><Input value={row.waist} onChange={(e) => handleSizeChartChange(i, "waist", e.target.value)} placeholder="28" className="h-8 w-16" /></td>
                    <td className="py-1 px-3"><Input value={row.hips} onChange={(e) => handleSizeChartChange(i, "hips", e.target.value)} placeholder="38" className="h-8 w-16" /></td>
                    <td className="py-1 px-3"><Input value={row.length} onChange={(e) => handleSizeChartChange(i, "length", e.target.value)} placeholder="42" className="h-8 w-16" /></td>
                    <td className="py-1"><Button type="button" variant="ghost" size="icon" className="h-8 w-8 text-destructive" onClick={() => removeSizeChartRow(i)}><Trash2 size={14} /></Button></td>
                  </tr>
                ))}</tbody>
              </table>
            </div>
          ) : <p className="text-xs text-muted-foreground">No size chart data. Click "+ Add Size" to add measurements.</p>}
        </CardContent></Card>
        <Card><CardHeader><CardTitle className="text-base">Images</CardTitle></CardHeader><CardContent>
          {existingImages.length > 0 && (
            <div className="mb-3">
              <p className="text-xs text-muted-foreground mb-2">Existing images</p>
              <div className="flex gap-2 flex-wrap">
                {existingImages.map((img) => (
                  <div key={img.id} className="relative group">
                    <img src={resolveImageUrl(img.imageUrl)} alt={img.altText} className="w-20 h-20 object-cover rounded border" />
                    {img.isPrimary && <span className="absolute top-0.5 left-0.5 bg-primary text-primary-foreground text-[8px] px-1 rounded">Primary</span>}
                    <button type="button" onClick={() => handleDeleteImage(img.id)} className="absolute -top-1.5 -right-1.5 w-5 h-5 bg-destructive text-white rounded-full flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity"><Trash2 size={10} /></button>
                  </div>
                ))}
              </div>
            </div>
          )}
          <Input type="file" multiple accept="image/*" onChange={(e) => setImages(Array.from(e.target.files || []))} />
          {images.length > 0 && (
            <div className="flex gap-2 mt-3 flex-wrap">
              {images.map((f, i) => <img key={i} src={URL.createObjectURL(f)} alt="" className="w-20 h-20 object-cover rounded border" />)}
            </div>
          )}
        </CardContent></Card>
        <div className="flex justify-end gap-3"><Button type="button" variant="outline" onClick={() => navigate("/admin/products")}>Cancel</Button><Button type="submit" disabled={saving}>{saving ? "Saving..." : isEdit ? "Update Product" : "Save Product"}</Button></div>
      </form>
    </div>
  );
}
