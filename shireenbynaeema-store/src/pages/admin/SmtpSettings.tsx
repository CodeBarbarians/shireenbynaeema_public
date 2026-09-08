import { useEffect, useState } from "react";
import { settingApi } from "@/services/api";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Checkbox } from "@/components/ui/checkbox";
import toast from "react-hot-toast";
import { Mail } from "lucide-react";

interface SmtpConfig {
  useSmtp: boolean;
  host: string;
  port: number;
  enableSsl: boolean;
  username: string;
  password: string;
  fromEmail: string;
  fromName: string;
}

export default function SmtpSettings() {
  const [config, setConfig] = useState<SmtpConfig>({
    useSmtp: false, host: "", port: 587, enableSsl: true,
    username: "", password: "", fromEmail: "", fromName: "Shireen by Naeema",
  });
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    settingApi.getSmtp().then((res) => {
      if (res.data.isSuccess && res.data.data?.value) {
        try {
          const raw = JSON.parse(res.data.data.value);
          setConfig({
            useSmtp: raw.useSmtp ?? raw.UseSmtp ?? false,
            host: raw.host ?? raw.Host ?? "",
            port: raw.port ?? raw.Port ?? 587,
            enableSsl: raw.enableSsl ?? raw.EnableSsl ?? true,
            username: raw.username ?? raw.Username ?? "",
            password: raw.password ?? raw.Password ?? "",
            fromEmail: raw.fromEmail ?? raw.FromEmail ?? "",
            fromName: raw.fromName ?? raw.FromName ?? "Shireen by Naeema",
          });
        } catch {}
      }
    }).finally(() => setLoading(false));
  }, []);

  const handleSave = async () => {
    setSaving(true);
    try {
      await settingApi.saveSmtp(config);
      toast.success("SMTP settings saved");
    } catch { toast.error("Failed to save"); }
    setSaving(false);
  };

  if (loading) return <div className="space-y-4">{Array.from({ length: 3 }).map((_, i) => <div key={i} className="h-20 bg-muted rounded animate-pulse" />)}</div>;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold tracking-tight">Email Settings</h1>
        <Button size="sm" onClick={handleSave} disabled={saving}>{saving ? "Saving..." : "Save Settings"}</Button>
      </div>

      <Card>
        <CardHeader><CardTitle className="text-base flex items-center gap-2"><Mail size={16} /> SMTP Configuration</CardTitle></CardHeader>
        <CardContent className="space-y-4">
          <label className="flex items-center gap-2 text-sm">
            <Checkbox checked={config.useSmtp} onCheckedChange={(v) => setConfig({ ...config, useSmtp: v === true })} />
            Enable SMTP (primary email method — SendGrid used as fallback)
          </label>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div><Label className="text-xs">SMTP Host</Label><Input value={config.host} onChange={(e) => setConfig({ ...config, host: e.target.value })} placeholder="smtp.gmail.com" /></div>
            <div><Label className="text-xs">Port</Label><Input type="number" value={config.port} onChange={(e) => setConfig({ ...config, port: parseInt(e.target.value) || 587 })} /></div>
            <div><Label className="text-xs">Username / Email</Label><Input value={config.username} onChange={(e) => setConfig({ ...config, username: e.target.value })} placeholder="your@gmail.com" /></div>
            <div><Label className="text-xs">Password / App Password</Label><Input type="password" value={config.password} onChange={(e) => setConfig({ ...config, password: e.target.value })} placeholder="xxxx xxxx xxxx xxxx" /></div>
            <div><Label className="text-xs">From Email</Label><Input value={config.fromEmail} onChange={(e) => setConfig({ ...config, fromEmail: e.target.value })} placeholder="noreply@shireenbynaeema.com" /></div>
            <div><Label className="text-xs">From Name</Label><Input value={config.fromName} onChange={(e) => setConfig({ ...config, fromName: e.target.value })} /></div>
          </div>

          <label className="flex items-center gap-2 text-sm">
            <Checkbox checked={config.enableSsl} onCheckedChange={(v) => setConfig({ ...config, enableSsl: v === true })} />
            Enable SSL/TLS
          </label>

          <div className="bg-muted/50 rounded-lg p-4 text-xs text-muted-foreground space-y-1">
            <p><strong>Gmail:</strong> Host: smtp.gmail.com | Port: 587 | Enable SSL | Use App Password</p>
            <p><strong>Custom Domain:</strong> Host: mail.yourdomain.com | Port: 587/465 | Use domain credentials</p>
            <p className="mt-2">SMTP is tried first. If it fails, SendGrid is used as fallback automatically.</p>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
