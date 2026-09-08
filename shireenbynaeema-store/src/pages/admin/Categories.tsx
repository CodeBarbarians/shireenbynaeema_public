import { useEffect, useState } from "react";
import { categoryApi, imageApi } from "@/services/api";
import type { Category } from "@/types";
import { Pencil, Trash2, Upload, X } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Checkbox } from "@/components/ui/checkbox";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Skeleton } from "@/components/ui/skeleton";
import { resolveImageUrl } from "@/lib/imageUtils";
import toast from "react-hot-toast";

interface CategoryImage {
  id: string;
  imageUrl: string;
  altText: string;
  sortOrder: number;
  isPrimary: boolean;
}

export default function AdminCategories() {
  const [categories, setCategories] = useState<Category[]>([]);
  const [loading, setLoading] = useState(true);
  const [editing, setEditing] = useState<Category | null>(null);
  const [form, setForm] = useState({ name: "", slug: "", description: "", imageUrl: "", showInNav: true });
  const [imageFile, setImageFile] = useState<File | null>(null);
  const [categoryImages, setCategoryImages] = useState<CategoryImage[]>([]);
  const [uploading, setUploading] = useState(false);

  const fetchCategories = () => categoryApi.list().then((res) => setCategories(res.data.data?.entities || [])).finally(() => setLoading(false));
  useEffect(() => { fetchCategories(); }, []);

  const loadCategoryImages = async (categoryId: string) => {
    try {
      const res = await categoryApi.getById(categoryId);
      const data = res.data.data;
      setCategoryImages(data?.images || []);
    } catch { setCategoryImages([]); }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      let categoryId: string;
      if (editing) {
        await categoryApi.update({ ...form, id: editing.id });
        categoryId = editing.id;
        toast.success("Category updated");
      } else {
        const res = await categoryApi.create(form);
        categoryId = res.data.data;
        toast.success("Category created");
      }
      if (imageFile) {
        await imageApi.uploadCategory(imageFile, categoryId);
      }
      setForm({ name: "", slug: "", description: "", imageUrl: "", showInNav: true });
      setImageFile(null);
      setEditing(null);
      setCategoryImages([]);
      fetchCategories();
    } catch { toast.error("Failed"); }
  };

  const handleDelete = async (id: string) => {
    if (!confirm("Delete this category?")) return;
    try { await categoryApi.delete(id); toast.success("Deleted"); fetchCategories(); } catch { toast.error("Failed"); }
  };

  const handleEdit = async (cat: Category) => {
    setEditing(cat);
    setForm({ name: cat.name, slug: cat.slug, description: cat.description, imageUrl: cat.imageUrl, showInNav: cat.showInNav });
    setImageFile(null);
    await loadCategoryImages(cat.id);
  };

  const resetForm = () => {
    setEditing(null);
    setForm({ name: "", slug: "", description: "", imageUrl: "", showInNav: true });
    setImageFile(null);
    setCategoryImages([]);
  };

  const handleMultiImageUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    if (!editing || !e.target.files?.length) return;
    setUploading(true);
    try {
      for (const file of Array.from(e.target.files)) {
        await imageApi.uploadCategoryImages(file, editing.id);
      }
      toast.success("Images uploaded");
      await loadCategoryImages(editing.id);
    } catch { toast.error("Upload failed"); }
    setUploading(false);
    e.target.value = "";
  };

  const handleDeleteImage = async (imageId: string) => {
    if (!confirm("Delete this image?")) return;
    try {
      await imageApi.deleteCategoryImageById(imageId);
      setCategoryImages((prev) => prev.filter((img) => img.id !== imageId));
      toast.success("Image deleted");
    } catch { toast.error("Failed"); }
  };

  const previewUrl = imageFile ? URL.createObjectURL(imageFile) : resolveImageUrl(form.imageUrl);

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-semibold tracking-tight">Categories</h1>

      <Card>
        <CardHeader><CardTitle className="text-base">{editing ? "Edit Category" : "Add Category"}</CardTitle></CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3">
              <div><label className="text-xs font-medium tracking-wider uppercase block mb-1.5">Name</label><Input placeholder="Name" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} required /></div>
              <div><label className="text-xs font-medium tracking-wider uppercase block mb-1.5">Slug</label><Input placeholder="Slug" value={form.slug} onChange={(e) => setForm({ ...form, slug: e.target.value })} required /></div>
              <div><label className="text-xs font-medium tracking-wider uppercase block mb-1.5">Description</label><Input placeholder="Description" value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} /></div>
              <div className="flex items-end"><label className="flex items-center gap-2 text-sm whitespace-nowrap h-8"><Checkbox checked={form.showInNav} onCheckedChange={(v) => setForm({ ...form, showInNav: v === true })} /> Show in Nav</label></div>
            </div>
            <div className="flex items-end gap-4">
              <div>
                <label className="text-xs font-medium tracking-wider uppercase block mb-1.5">Primary Image</label>
                <div className="flex items-center gap-3">
                  {previewUrl && <img src={previewUrl} alt="" className="w-16 h-16 object-cover rounded-lg border border-border" />}
                  <label className="flex h-16 items-center justify-center rounded-lg border border-dashed border-input bg-background px-4 py-2 text-sm cursor-pointer hover:bg-accent transition-colors">
                    <input type="file" accept="image/*" className="hidden" onChange={(e) => setImageFile(e.target.files?.[0] || null)} />
                    {previewUrl ? "Change" : "Upload Image"}
                  </label>
                </div>
              </div>
              <div className="flex gap-2">
                <Button type="submit">{editing ? "Update" : "Add"}</Button>
                {editing && <Button type="button" variant="outline" onClick={resetForm}>Cancel</Button>}
              </div>
            </div>
          </form>
        </CardContent>
      </Card>

      {editing && (
        <Card>
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle className="text-base">Category Images ({categoryImages.length})</CardTitle>
            <label className="flex items-center gap-2">
              <Button variant="outline" size="sm" disabled={uploading} onClick={() => document.getElementById("cat-multi-upload")?.click()}>
                <Upload size={14} className="mr-1" />{uploading ? "Uploading..." : "Add Images"}
              </Button>
              <input id="cat-multi-upload" type="file" accept="image/*" multiple className="hidden" onChange={handleMultiImageUpload} />
            </label>
          </CardHeader>
          <CardContent>
            {categoryImages.length === 0 ? (
              <p className="text-sm text-muted-foreground text-center py-4">No images yet. Upload images for the carousel.</p>
            ) : (
              <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-3">
                {categoryImages.sort((a, b) => a.sortOrder - b.sortOrder).map((img) => (
                  <div key={img.id} className="relative group aspect-square rounded-lg overflow-hidden border border-border">
                    <img src={resolveImageUrl(img.imageUrl)} alt={img.altText} className="w-full h-full object-cover" />
                    <button type="button" onClick={() => handleDeleteImage(img.id)} className="absolute top-1 right-1 w-6 h-6 bg-destructive text-white rounded-full flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity"><X size={12} /></button>
                    {img.isPrimary && <span className="absolute top-1 left-1 bg-primary text-primary-foreground text-[9px] px-1.5 py-0.5 rounded">Primary</span>}
                  </div>
                ))}
              </div>
            )}
          </CardContent>
        </Card>
      )}

      <Card>
        <CardContent className="p-0">
          <Table>
            <TableHeader><TableRow><TableHead>Image</TableHead><TableHead>Name</TableHead><TableHead>Slug</TableHead><TableHead>Status</TableHead><TableHead>Nav</TableHead><TableHead className="text-right">Actions</TableHead></TableRow></TableHeader>
            <TableBody>
              {loading ? Array.from({ length: 3 }).map((_, i) => <TableRow key={i}>{Array.from({ length: 6 }).map((__, j) => <TableCell key={j}><Skeleton className="h-4 w-full" /></TableCell>)}</TableRow>) :
                categories.length === 0 ? <TableRow><TableCell colSpan={6} className="text-center py-8 text-muted-foreground">No categories</TableCell></TableRow> :
                categories.map((c) => (
                  <TableRow key={c.id}>
                    <TableCell>{c.imageUrl ? <img src={resolveImageUrl(c.imageUrl)} alt={c.name} className="w-10 h-10 object-cover rounded" /> : <div className="w-10 h-10 rounded bg-muted" />}</TableCell>
                    <TableCell className="font-medium">{c.name}</TableCell>
                    <TableCell className="text-muted-foreground font-mono text-xs">{c.slug}</TableCell>
                    <TableCell><Badge variant={c.isActive ? "default" : "secondary"}>{c.isActive ? "Active" : "Inactive"}</Badge></TableCell>
                    <TableCell><Badge variant={c.showInNav ? "default" : "secondary"}>{c.showInNav ? "Yes" : "No"}</Badge></TableCell>
                    <TableCell className="text-right">
                      <div className="flex items-center justify-end gap-1">
                        <Button variant="ghost" size="icon" className="h-8 w-8" onClick={() => handleEdit(c)}><Pencil size={14} /></Button>
                        <Button variant="ghost" size="icon" className="h-8 w-8 text-destructive" onClick={() => handleDelete(c.id)}><Trash2 size={14} /></Button>
                      </div>
                    </TableCell>
                  </TableRow>
                ))}
            </TableBody>
          </Table>
        </CardContent>
      </Card>
    </div>
  );
}
