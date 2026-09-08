import { useEffect, useState } from "react";
import { settingApi } from "@/services/api";
import { useThemeStore, type CustomTheme, type ThemeColors } from "@/store/themeStore";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Skeleton } from "@/components/ui/skeleton";
import { Moon, Sun, Palette, Save, Plus, Trash2, Pencil, X, Check } from "lucide-react";
import toast from "react-hot-toast";
import { cn } from "@/lib/utils";

interface Setting {
  id: string;
  key: string;
  value: string;
  description?: string;
}

const DEFAULT_COLORS: ThemeColors = {
  background: "#ffffff",
  foreground: "#1a1a1a",
  card: "#ffffff",
  cardForeground: "#1a1a1a",
  primary: "#1a1a1a",
  primaryForeground: "#ffffff",
  secondary: "#f5f5f5",
  secondaryForeground: "#1a1a1a",
  muted: "#f5f5f5",
  mutedForeground: "#737373",
  accent: "#f5f5f5",
  accentForeground: "#1a1a1a",
  destructive: "#dc2626",
  border: "#e5e5e5",
  input: "#e5e5e5",
  ring: "#1a1a1a",
};

const COLOR_FIELDS: { key: keyof ThemeColors; label: string }[] = [
  { key: "background", label: "Background" },
  { key: "foreground", label: "Text" },
  { key: "card", label: "Card" },
  { key: "cardForeground", label: "Card Text" },
  { key: "primary", label: "Primary" },
  { key: "primaryForeground", label: "Primary Text" },
  { key: "secondary", label: "Secondary" },
  { key: "secondaryForeground", label: "Secondary Text" },
  { key: "muted", label: "Muted" },
  { key: "mutedForeground", label: "Muted Text" },
  { key: "accent", label: "Accent" },
  { key: "accentForeground", label: "Accent Text" },
  { key: "destructive", label: "Destructive" },
  { key: "border", label: "Border" },
  { key: "input", label: "Input" },
  { key: "ring", label: "Ring" },
];

function ColorPicker({ label, value, onChange }: { label: string; value: string; onChange: (v: string) => void }) {
  return (
    <div className="flex items-center gap-2">
      <input type="color" value={value} onChange={(e) => onChange(e.target.value)} className="w-8 h-8 rounded border border-border cursor-pointer" />
      <div className="flex-1 min-w-0">
        <p className="text-xs font-medium truncate">{label}</p>
        <p className="text-[10px] text-muted-foreground font-mono">{value}</p>
      </div>
    </div>
  );
}

export default function Settings() {
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const { theme, setTheme, customThemes, loadCustomThemes } = useThemeStore();

  const [storeName, setStoreName] = useState("");
  const [storeEmail, setStoreEmail] = useState("");
  const [storePhone, setStorePhone] = useState("");
  const [currency, setCurrency] = useState("PKR");
  const [taxRate, setTaxRate] = useState("0");
  const [freeShippingThreshold, setFreeShippingThreshold] = useState("0");

  // Theme editor state
  const [editingTheme, setEditingTheme] = useState<CustomTheme | null>(null);
  const [themeName, setThemeName] = useState("");
  const [themeColors, setThemeColors] = useState<ThemeColors>({ ...DEFAULT_COLORS });
  const [showEditor, setShowEditor] = useState(false);

  useEffect(() => {
    settingApi.getAll().then((res) => {
      const data: Setting[] = res.data.data || [];
      const get = (key: string) => data.find((s) => s.key === key)?.value || "";
      setStoreName(get("store_name"));
      setStoreEmail(get("store_email"));
      setStorePhone(get("store_phone"));
      setCurrency(get("currency") || "PKR");
      setTaxRate(get("tax_rate") || "0");
      setFreeShippingThreshold(get("free_shipping_threshold") || "0");

      const themesJson = get("custom_themes");
      if (themesJson) {
        try { loadCustomThemes(JSON.parse(themesJson)); } catch {}
      }
    }).finally(() => setLoading(false));
  }, []);

  const handleSave = async () => {
    setSaving(true);
    try {
      await settingApi.bulkUpdate([
        { key: "store_name", value: storeName, description: "Store display name" },
        { key: "store_email", value: storeEmail, description: "Store contact email" },
        { key: "store_phone", value: storePhone, description: "Store contact phone" },
        { key: "currency", value: currency, description: "Currency code" },
        { key: "tax_rate", value: taxRate, description: "Tax rate percentage" },
        { key: "free_shipping_threshold", value: freeShippingThreshold, description: "Minimum order for free shipping" },
        { key: "theme", value: theme, description: "UI theme" },
        { key: "custom_themes", value: JSON.stringify(customThemes), description: "Custom themes JSON" },
      ]);
      toast.success("Settings saved");
    } catch {
      toast.error("Failed to save settings");
    } finally {
      setSaving(false);
    }
  };

  const startCreateTheme = () => {
    setEditingTheme(null);
    setThemeName("");
    setThemeColors({ ...DEFAULT_COLORS });
    setShowEditor(true);
  };

  const startEditTheme = (t: CustomTheme) => {
    setEditingTheme(t);
    setThemeName(t.name);
    setThemeColors({ ...t.colors });
    setShowEditor(true);
  };

  const saveTheme = () => {
    if (!themeName.trim()) { toast.error("Theme name required"); return; }
    const id = editingTheme?.id || `custom-${Date.now()}`;
    const updated: CustomTheme[] = editingTheme
      ? customThemes.map((t) => t.id === editingTheme.id ? { ...t, name: themeName, colors: themeColors } : t)
      : [...customThemes, { id, name: themeName, colors: themeColors }];
    loadCustomThemes(updated);
    setShowEditor(false);
    setEditingTheme(null);
    toast.success(editingTheme ? "Theme updated" : "Theme created");
  };

  const deleteTheme = (id: string) => {
    const updated = customThemes.filter((t) => t.id !== id);
    loadCustomThemes(updated);
    if (theme === id) setTheme("light");
    toast.success("Theme deleted");
  };

  const previewTheme = (colors: ThemeColors) => {
    const root = document.documentElement;
    const vars: Record<string, string> = {
      "--background": colors.background, "--foreground": colors.foreground,
      "--card": colors.card, "--card-foreground": colors.cardForeground,
      "--primary": colors.primary, "--primary-foreground": colors.primaryForeground,
      "--secondary": colors.secondary, "--secondary-foreground": colors.secondaryForeground,
      "--muted": colors.muted, "--muted-foreground": colors.mutedForeground,
      "--accent": colors.accent, "--accent-foreground": colors.accentForeground,
      "--destructive": colors.destructive, "--border": colors.border,
      "--input": colors.input, "--ring": colors.ring,
    };
    Object.entries(vars).forEach(([k, v]) => root.style.setProperty(k, v));
  };

  if (loading) {
    return (
      <div className="space-y-6">
        <h1 className="text-2xl font-semibold tracking-tight">Settings</h1>
        {Array.from({ length: 3 }).map((_, i) => <Skeleton key={i} className="h-48 w-full" />)}
      </div>
    );
  }

  return (
    <div className="max-w-3xl space-y-6">
      <div className="flex items-center justify-between flex-wrap gap-3">
        <h1 className="text-2xl font-semibold tracking-tight">Settings</h1>
        <Button onClick={handleSave} disabled={saving} className="gap-2">
          <Save size={16} />
          {saving ? "Saving..." : "Save Changes"}
        </Button>
      </div>

      {/* Theme Selection */}
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle className="text-base">Theme</CardTitle>
              <CardDescription>Choose or create a color theme for the store.</CardDescription>
            </div>
            <Button variant="outline" size="sm" onClick={startCreateTheme} className="gap-1.5">
              <Plus size={14} /> New Theme
            </Button>
          </div>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-3">
            {/* Built-in themes */}
            {[
              { id: "light", label: "Light", icon: Sun, colors: { bg: "#fff", fg: "#1a1a1a", primary: "#1a1a1a" } },
              { id: "dark", label: "Dark", icon: Moon, colors: { bg: "#1a1a1a", fg: "#fff", primary: "#e5e5e5" } },
              { id: "peach", label: "Peach", icon: Palette, colors: { bg: "#fdf6ee", fg: "#3d2c1e", primary: "#d4845e" } },
            ].map((t) => {
              const isActive = theme === t.id;
              return (
                <button key={t.id} onClick={() => setTheme(t.id)}
                  className={cn("flex flex-col items-center gap-2 p-4 rounded-xl border-2 transition-all", isActive ? "border-primary" : "border-border hover:border-primary/50")}>
                  <div className="flex gap-1">
                    <span className="w-4 h-4 rounded-full border" style={{ backgroundColor: t.colors.bg }} />
                    <span className="w-4 h-4 rounded-full border" style={{ backgroundColor: t.colors.primary }} />
                    <span className="w-4 h-4 rounded-full border" style={{ backgroundColor: t.colors.fg }} />
                  </div>
                  <p className="text-sm font-medium">{t.label}</p>
                </button>
              );
            })}

            {/* Custom themes */}
            {customThemes.map((t) => {
              const isActive = theme === t.id;
              return (
                <div key={t.id} className={cn("relative group rounded-xl border-2 transition-all", isActive ? "border-primary" : "border-border hover:border-primary/50")}>
                  <button onClick={() => setTheme(t.id)} className="flex flex-col items-center gap-2 p-4 w-full">
                    <div className="flex gap-1">
                      <span className="w-4 h-4 rounded-full border" style={{ backgroundColor: t.colors.background }} />
                      <span className="w-4 h-4 rounded-full border" style={{ backgroundColor: t.colors.primary }} />
                      <span className="w-4 h-4 rounded-full border" style={{ backgroundColor: t.colors.foreground }} />
                    </div>
                    <p className="text-sm font-medium">{t.name}</p>
                  </button>
                  <div className="absolute top-2 right-2 flex gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                    <button onClick={() => startEditTheme(t)} className="w-6 h-6 rounded bg-background border border-border flex items-center justify-center hover:bg-accent"><Pencil size={12} /></button>
                    <button onClick={() => deleteTheme(t.id)} className="w-6 h-6 rounded bg-background border border-border flex items-center justify-center hover:bg-destructive/10 text-destructive"><Trash2 size={12} /></button>
                  </div>
                </div>
              );
            })}
          </div>
        </CardContent>
      </Card>

      {/* Theme Editor Modal */}
      {showEditor && (
        <Card className="border-primary">
          <CardHeader>
            <div className="flex items-center justify-between">
              <CardTitle className="text-base">{editingTheme ? "Edit Theme" : "Create Theme"}</CardTitle>
              <Button variant="ghost" size="icon" className="h-8 w-8" onClick={() => { setShowEditor(false); setEditingTheme(null); }}><X size={16} /></Button>
            </div>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-2">
              <Label className="text-xs">Theme Name</Label>
              <Input value={themeName} onChange={(e) => setThemeName(e.target.value)} placeholder="My Custom Theme" />
            </div>

            <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-3">
              {COLOR_FIELDS.map((f) => (
                <ColorPicker key={f.key} label={f.label} value={themeColors[f.key]}
                  onChange={(v) => {
                    const updated = { ...themeColors, [f.key]: v };
                    setThemeColors(updated);
                    previewTheme(updated);
                  }} />
              ))}
            </div>

            {/* Live preview */}
            <div className="rounded-xl border p-4 space-y-2" style={{ background: themeColors.background, color: themeColors.foreground, borderColor: themeColors.border }}>
              <p className="text-sm font-medium" style={{ color: themeColors.foreground }}>Preview</p>
              <div className="flex gap-2">
                <button className="px-3 py-1.5 rounded-lg text-xs font-medium" style={{ background: themeColors.primary, color: themeColors.primaryForeground }}>Primary</button>
                <button className="px-3 py-1.5 rounded-lg text-xs font-medium" style={{ background: themeColors.secondary, color: themeColors.secondaryForeground }}>Secondary</button>
                <button className="px-3 py-1.5 rounded-lg text-xs font-medium" style={{ background: themeColors.accent, color: themeColors.accentForeground }}>Accent</button>
                <button className="px-3 py-1.5 rounded-lg text-xs font-medium" style={{ background: themeColors.destructive, color: "#fff" }}>Destructive</button>
              </div>
              <div className="flex gap-2 text-xs" style={{ color: themeColors.mutedForeground }}>
                <span>Muted text</span>
                <span>•</span>
                <span>Regular text</span>
              </div>
            </div>

            <div className="flex gap-2">
              <Button onClick={saveTheme} className="gap-1.5"><Check size={14} /> {editingTheme ? "Update" : "Create"} Theme</Button>
              <Button variant="outline" onClick={() => { setShowEditor(false); setEditingTheme(null); }}>Cancel</Button>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Store Info */}
      <Card>
        <CardHeader>
          <CardTitle className="text-base">Store Information</CardTitle>
          <CardDescription>Basic store details displayed to customers.</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label className="text-xs">Store Name</Label>
              <Input value={storeName} onChange={(e) => setStoreName(e.target.value)} placeholder="Shireen By Naeema" />
            </div>
            <div className="space-y-2">
              <Label className="text-xs">Contact Email</Label>
              <Input type="email" value={storeEmail} onChange={(e) => setStoreEmail(e.target.value)} placeholder="info@example.com" />
            </div>
            <div className="space-y-2">
              <Label className="text-xs">Phone Number</Label>
              <Input type="tel" value={storePhone} onChange={(e) => setStorePhone(e.target.value)} placeholder="+92 300 1234567" />
            </div>
            <div className="space-y-2">
              <Label className="text-xs">Currency</Label>
              <Input value={currency} onChange={(e) => setCurrency(e.target.value)} placeholder="PKR" />
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Tax & Shipping */}
      <Card>
        <CardHeader>
          <CardTitle className="text-base">Tax & Shipping</CardTitle>
          <CardDescription>Configure tax rates and shipping rules.</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label className="text-xs">Tax Rate (%)</Label>
              <Input type="number" value={taxRate} onChange={(e) => setTaxRate(e.target.value)} placeholder="0" min="0" max="100" />
            </div>
            <div className="space-y-2">
              <Label className="text-xs">Free Shipping Threshold (Rs.)</Label>
              <Input type="number" value={freeShippingThreshold} onChange={(e) => setFreeShippingThreshold(e.target.value)} placeholder="0" min="0" />
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
