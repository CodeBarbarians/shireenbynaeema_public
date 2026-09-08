import { useEffect, useState } from "react";
import { blogApi, imageApi } from "@/services/api";
import type { BlogPost } from "@/types";
import { Pencil, Trash2, Plus } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Skeleton } from "@/components/ui/skeleton";
import toast from "react-hot-toast";

export default function AdminBlogPosts() {
  const [posts, setPosts] = useState<BlogPost[]>([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState<BlogPost | null>(null);
  const [form, setForm] = useState({ title: "", slug: "", excerpt: "", content: "", coverImageUrl: "", author: "Shireen by Naeema", isPublished: false });
  const [coverFile, setCoverFile] = useState<File | null>(null);

  const load = () => blogApi.list().then((res) => setPosts(res.data.data?.entities || [])).finally(() => setLoading(false));
  useEffect(() => { load(); }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    let coverUrl = form.coverImageUrl;
    if (coverFile) {
      const res = await imageApi.upload(coverFile);
      coverUrl = res.data.data?.url || coverUrl;
    }
    const slug = form.slug || form.title.toLowerCase().replace(/[^a-z0-9]+/g, "-").replace(/(^-|-$)/g, "");
    await blogApi.add({ ...form, slug, coverImageUrl: coverUrl, id: editing?.id });
    toast.success(editing ? "Post updated" : "Post created");
    setShowForm(false); setEditing(null); setCoverFile(null);
    setForm({ title: "", slug: "", excerpt: "", content: "", coverImageUrl: "", author: "Shireen by Naeema", isPublished: false });
    load();
  };

  const handleEdit = (p: BlogPost) => {
    setEditing(p);
    setForm({ title: p.title, slug: p.slug, excerpt: p.excerpt, content: "", coverImageUrl: p.coverImageUrl, author: p.author, isPublished: p.isPublished });
    setShowForm(true);
  };

  const handleDelete = async (id: string) => {
    if (!confirm("Delete this blog post?")) return;
    await blogApi.delete(id);
    setPosts((prev) => prev.filter((p) => p.id !== id));
    toast.success("Post deleted");
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between flex-wrap gap-3">
        <h1 className="text-2xl font-semibold tracking-tight">Blog Posts</h1>
        <Button size="sm" onClick={() => { setShowForm(!showForm); setEditing(null); setForm({ title: "", slug: "", excerpt: "", content: "", coverImageUrl: "", author: "Shireen by Naeema", isPublished: false }); }}><Plus size={14} className="mr-1" />{showForm ? "Cancel" : "New Post"}</Button>
      </div>

      {showForm && (
        <Card><CardHeader><CardTitle className="text-base">{editing ? "Edit" : "New"} Post</CardTitle></CardHeader><CardContent>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div><Label>Title</Label><Input value={form.title} onChange={(e) => setForm({ ...form, title: e.target.value })} required /></div>
              <div><Label>Slug</Label><Input value={form.slug} onChange={(e) => setForm({ ...form, slug: e.target.value })} placeholder="auto-generated from title" /></div>
            </div>
            <div><Label>Excerpt</Label><Input value={form.excerpt} onChange={(e) => setForm({ ...form, excerpt: e.target.value })} /></div>
            <div><Label>Content (HTML)</Label><Textarea value={form.content} onChange={(e) => setForm({ ...form, content: e.target.value })} rows={10} required /></div>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div><Label>Cover Image</Label><Input type="file" accept="image/*" onChange={(e) => setCoverFile(e.target.files?.[0] || null)} /></div>
              <div><Label>Author</Label><Input value={form.author} onChange={(e) => setForm({ ...form, author: e.target.value })} /></div>
            </div>
            <div className="flex items-center gap-2"><input type="checkbox" checked={form.isPublished} onChange={(e) => setForm({ ...form, isPublished: e.target.checked })} className="rounded" /><Label>Published</Label></div>
            <Button type="submit" className="w-full">{editing ? "Update" : "Create"}</Button>
          </form>
        </CardContent></Card>
      )}

      <Card><CardContent className="p-0">
        <div className="overflow-x-auto">
        <Table>
          <TableHeader><TableRow><TableHead>Title</TableHead><TableHead className="hidden sm:table-cell">Author</TableHead><TableHead className="hidden md:table-cell">Views</TableHead><TableHead>Status</TableHead><TableHead className="w-[80px]" /></TableRow></TableHeader>
          <TableBody>
            {loading ? Array.from({ length: 3 }).map((_, i) => <TableRow key={i}>{Array.from({ length: 5 }).map((__, j) => <TableCell key={j}><Skeleton className="h-4 w-full" /></TableCell>)}</TableRow>) :
              posts.length === 0 ? <TableRow><TableCell colSpan={5} className="text-center py-8 text-muted-foreground">No posts yet</TableCell></TableRow> :
              posts.map((p) => <TableRow key={p.id}><TableCell className="font-medium">{p.title}</TableCell><TableCell className="text-muted-foreground">{p.author}</TableCell><TableCell>{p.viewCount}</TableCell><TableCell><Badge variant={p.isPublished ? "default" : "secondary"}>{p.isPublished ? "Published" : "Draft"}</Badge></TableCell><TableCell><Button variant="ghost" size="icon" className="h-8 w-8" onClick={() => handleEdit(p)}><Pencil size={14} /></Button><Button variant="ghost" size="icon" className="h-8 w-8 text-destructive" onClick={() => handleDelete(p.id)}><Trash2 size={14} /></Button></TableCell></TableRow>)}
          </TableBody>
        </Table>
        </div>
      </CardContent></Card>
    </div>
  );
}
