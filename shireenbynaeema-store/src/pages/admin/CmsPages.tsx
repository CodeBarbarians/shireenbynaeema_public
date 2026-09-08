import { useEffect, useState } from "react";
import { cmsPageApi } from "@/services/api";
import { Pencil, Trash2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Checkbox } from "@/components/ui/checkbox";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Skeleton } from "@/components/ui/skeleton";
import toast from "react-hot-toast";

interface CmsPageItem {
  id: string;
  slug: string;
  title: string;
  content: string;
  isActive: boolean;
}

export default function AdminCmsPages() {
  const [pages, setPages] = useState<CmsPageItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [editing, setEditing] = useState<CmsPageItem | null>(null);
  const [form, setForm] = useState({ slug: "", title: "", content: "", isActive: true });

  const fetchPages = async () => {
    setLoading(true);
    try { const res = await cmsPageApi.list(); setPages(res.data.data?.entities || []); } finally { setLoading(false); }
  };

  useEffect(() => { fetchPages(); }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      if (editing) {
        await cmsPageApi.update({ ...form, id: editing.id });
        toast.success("Page updated");
      } else {
        await cmsPageApi.create(form);
        toast.success("Page created");
      }
      setForm({ slug: "", title: "", content: "", isActive: true });
      setEditing(null);
      fetchPages();
    } catch { toast.error("Failed"); }
  };

  const handleDelete = async (id: string) => {
    if (!confirm("Delete this page?")) return;
    try { await cmsPageApi.delete(id); toast.success("Deleted"); fetchPages(); } catch { toast.error("Failed"); }
  };

  const handleEdit = (page: CmsPageItem) => {
    setEditing(page);
    setForm({ slug: page.slug, title: page.title, content: page.content, isActive: page.isActive });
  };

  const resetForm = () => {
    setEditing(null);
    setForm({ slug: "", title: "", content: "", isActive: true });
  };

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-semibold tracking-tight">CMS Pages</h1>

      <Card>
        <CardHeader><CardTitle className="text-base">{editing ? "Edit Page" : "Add Page"}</CardTitle></CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
              <div><label className="text-xs font-medium tracking-wider uppercase block mb-1.5">Slug</label><Input placeholder="e.g. contact, about" value={form.slug} onChange={(e) => setForm({ ...form, slug: e.target.value })} required /></div>
              <div><label className="text-xs font-medium tracking-wider uppercase block mb-1.5">Title</label><Input placeholder="Page title" value={form.title} onChange={(e) => setForm({ ...form, title: e.target.value })} required /></div>
            </div>
            <div><label className="text-xs font-medium tracking-wider uppercase block mb-1.5">Content</label><Textarea placeholder="Page content..." value={form.content} onChange={(e) => setForm({ ...form, content: e.target.value })} rows={8} required /></div>
            <div className="flex items-center gap-4 flex-wrap">
              <label className="flex items-center gap-2 text-sm"><Checkbox checked={form.isActive} onCheckedChange={(v) => setForm({ ...form, isActive: v === true })} /> Active</label>
              <Button type="submit">{editing ? "Update" : "Add"}</Button>
              {editing && <Button type="button" variant="outline" onClick={resetForm}>Cancel</Button>}
            </div>
          </form>
        </CardContent>
      </Card>

      <Card>
        <CardContent className="p-0">
          <div className="overflow-x-auto">
          <Table>
            <TableHeader><TableRow><TableHead>Slug</TableHead><TableHead>Title</TableHead><TableHead>Status</TableHead><TableHead className="text-right">Actions</TableHead></TableRow></TableHeader>
            <TableBody>
              {loading ? Array.from({ length: 3 }).map((_, i) => <TableRow key={i}>{Array.from({ length: 4 }).map((__, j) => <TableCell key={j}><Skeleton className="h-4 w-full" /></TableCell>)}</TableRow>) :
                pages.length === 0 ? <TableRow><TableCell colSpan={4} className="text-center py-8 text-muted-foreground">No pages</TableCell></TableRow> :
                pages.map((p) => (
                  <TableRow key={p.id}>
                    <TableCell className="font-mono text-xs text-muted-foreground">{p.slug}</TableCell>
                    <TableCell className="font-medium">{p.title}</TableCell>
                    <TableCell><Badge variant={p.isActive ? "default" : "secondary"}>{p.isActive ? "Active" : "Inactive"}</Badge></TableCell>
                    <TableCell className="text-right">
                      <div className="flex items-center justify-end gap-1">
                        <Button variant="ghost" size="icon" className="h-8 w-8" onClick={() => handleEdit(p)}><Pencil size={14} /></Button>
                        <Button variant="ghost" size="icon" className="h-8 w-8 text-destructive" onClick={() => handleDelete(p.id)}><Trash2 size={14} /></Button>
                      </div>
                    </TableCell>
                  </TableRow>
                ))}
            </TableBody>
          </Table>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
