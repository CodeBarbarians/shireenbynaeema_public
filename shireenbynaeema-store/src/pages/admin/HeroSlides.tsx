import { useEffect, useState } from "react";
import { settingApi, imageApi } from "@/services/api";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Card, CardContent } from "@/components/ui/card";
import { Checkbox } from "@/components/ui/checkbox";
import { Trash2, Plus, GripVertical } from "lucide-react";
import { resolveImageUrl } from "@/lib/imageUtils";
import toast from "react-hot-toast";

interface HeroSlide {
  id: string;
  imageUrl: string;
  title: string;
  subtitle: string;
  linkUrl: string;
  buttonText: string;
  isActive: boolean;
  sortOrder: number;
}

export default function AdminHeroSlides() {
  const [slides, setSlides] = useState<HeroSlide[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    settingApi.getHeroSlides().then((res) => {
      if (res.data.isSuccess && res.data.data?.value) {
        try {
          const raw = JSON.parse(res.data.data.value);
          setSlides(raw.map((s: any) => ({
            id: s.id || s.Id || "",
            imageUrl: s.imageUrl || s.ImageUrl || "",
            title: s.title || s.Title || "",
            subtitle: s.subtitle || s.Subtitle || "",
            linkUrl: s.linkUrl || s.LinkUrl || "/collections/all",
            buttonText: s.buttonText || s.ButtonText || "Shop Now",
            isActive: s.isActive ?? s.IsActive ?? true,
            sortOrder: s.sortOrder ?? s.SortOrder ?? 0,
          })));
        } catch {}
      }
    }).finally(() => setLoading(false));
  }, []);

  const handleSave = async () => {
    setSaving(true);
    try {
      await settingApi.saveHeroSlides(slides);
      toast.success("Hero slides saved");
    } catch { toast.error("Failed to save"); }
    setSaving(false);
  };

  const addSlide = () => {
    setSlides([...slides, { id: crypto.randomUUID(), imageUrl: "", title: "", subtitle: "", linkUrl: "/collections/all", buttonText: "Shop Now", isActive: true, sortOrder: slides.length }]);
  };

  const updateSlide = (index: number, field: keyof HeroSlide, value: any) => {
    const updated = [...slides];
    (updated[index] as any)[field] = value;
    setSlides(updated);
  };

  const removeSlide = (index: number) => {
    setSlides(slides.filter((_, i) => i !== index));
  };

  const handleImageUpload = async (index: number, file: File) => {
    try {
      const res = await imageApi.upload(file);
      if (res.data.isSuccess) {
        updateSlide(index, "imageUrl", res.data.data.url);
        toast.success("Image uploaded");
      }
    } catch { toast.error("Upload failed"); }
  };

  const moveSlide = (index: number, direction: -1 | 1) => {
    const newIndex = index + direction;
    if (newIndex < 0 || newIndex >= slides.length) return;
    const updated = [...slides];
    [updated[index], updated[newIndex]] = [updated[newIndex], updated[index]];
    updated.forEach((s, i) => s.sortOrder = i);
    setSlides(updated);
  };

  if (loading) return <div className="space-y-4">{Array.from({ length: 2 }).map((_, i) => <div key={i} className="h-32 bg-muted rounded animate-pulse" />)}</div>;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between flex-wrap gap-3">
        <h1 className="text-2xl font-semibold tracking-tight">Hero Slides</h1>
        <div className="flex gap-2">
          <Button variant="outline" size="sm" onClick={addSlide}><Plus size={14} className="mr-1" />Add Slide</Button>
          <Button size="sm" onClick={handleSave} disabled={saving}>{saving ? "Saving..." : "Save"}</Button>
        </div>
      </div>

      <p className="text-sm text-muted-foreground">Manage the hero carousel on the landing page. Upload images and set titles, links, and order.</p>

      {slides.length === 0 && (
        <Card><CardContent className="p-8 text-center text-muted-foreground">
          No hero slides yet. Click "Add Slide" to create one.
        </CardContent></Card>
      )}

      {slides.map((slide, i) => (
        <Card key={slide.id}>
          <CardContent className="p-4">
            <div className="flex items-start gap-4">
              <div className="flex flex-col gap-1 pt-2">
                <button onClick={() => moveSlide(i, -1)} disabled={i === 0} className="text-muted-foreground hover:text-foreground disabled:opacity-30"><GripVertical size={14} /></button>
                <span className="text-xs text-muted-foreground text-center">{i + 1}</span>
              </div>

              <div className="w-40 h-24 bg-muted rounded-lg overflow-hidden flex-shrink-0">
                {slide.imageUrl ? (
                  <img src={resolveImageUrl(slide.imageUrl)} alt="" className="w-full h-full object-cover" />
                ) : (
                  <label className="w-full h-full flex items-center justify-center cursor-pointer text-xs text-muted-foreground hover:text-foreground">
                    Upload Image
                    <input type="file" accept="image/*" className="hidden" onChange={(e) => e.target.files?.[0] && handleImageUpload(i, e.target.files[0])} />
                  </label>
                )}
              </div>

              <div className="flex-1 space-y-2">
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-2">
                  <div><Label className="text-xs">Title</Label><Input value={slide.title} onChange={(e) => updateSlide(i, "title", e.target.value)} placeholder="Welcome to..." className="h-8 text-sm" /></div>
                  <div><Label className="text-xs">Subtitle</Label><Input value={slide.subtitle} onChange={(e) => updateSlide(i, "subtitle", e.target.value)} placeholder="Western Clothing Brand" className="h-8 text-sm" /></div>
                </div>
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-2">
                  <div><Label className="text-xs">Link URL</Label><Input value={slide.linkUrl} onChange={(e) => updateSlide(i, "linkUrl", e.target.value)} className="h-8 text-sm" /></div>
                  <div><Label className="text-xs">Button Text</Label><Input value={slide.buttonText} onChange={(e) => updateSlide(i, "buttonText", e.target.value)} className="h-8 text-sm" /></div>
                </div>
                <label className="flex items-center gap-2 text-xs">
                  <Checkbox checked={slide.isActive} onCheckedChange={(v) => updateSlide(i, "isActive", v === true)} /> Active
                </label>
              </div>

              <Button variant="ghost" size="icon" className="h-8 w-8 text-destructive flex-shrink-0" onClick={() => removeSlide(i)}><Trash2 size={14} /></Button>
            </div>
          </CardContent>
        </Card>
      ))}
    </div>
  );
}
