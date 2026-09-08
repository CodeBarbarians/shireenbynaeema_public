import { useState, useRef, useEffect } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuthStore } from "@/store/authStore";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Card, CardContent } from "@/components/ui/card";
import toast from "react-hot-toast";
import SEO from "@/components/seo/SEO";
import { ArrowLeft, Shield } from "lucide-react";

export default function Login() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [otp, setOtp] = useState(["", "", "", ""]);
  const otpRefs = [useRef<HTMLInputElement>(null), useRef<HTMLInputElement>(null), useRef<HTMLInputElement>(null), useRef<HTMLInputElement>(null)];
  const { login, verifyOtp, loading, otpRequired, otpEmail, clearOtp } = useAuthStore();
  const navigate = useNavigate();

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    const success = await login(email, password);
    if (success) {
      toast.success("Welcome back!");
      navigate("/");
    }
  };

  const handleOtpChange = (index: number, value: string) => {
    if (value.length > 1) value = value.slice(-1);
    if (!/^\d*$/.test(value)) return;
    const newOtp = [...otp];
    newOtp[index] = value;
    setOtp(newOtp);
    if (value && index < 3) otpRefs[index + 1].current?.focus();
  };

  const handleOtpKeyDown = (index: number, e: React.KeyboardEvent) => {
    if (e.key === "Backspace" && !otp[index] && index > 0) otpRefs[index - 1].current?.focus();
  };

  const handleOtpPaste = (e: React.ClipboardEvent) => {
    e.preventDefault();
    const pasted = e.clipboardData.getData("text").replace(/\D/g, "").slice(0, 4);
    const newOtp = pasted.split("").concat(Array(4).fill("")).slice(0, 4);
    setOtp(newOtp);
    otpRefs[Math.min(pasted.length, 3)].current?.focus();
  };

  const handleVerifyOtp = async () => {
    const code = otp.join("");
    if (code.length !== 4) { toast.error("Enter 4-digit code"); return; }
    const success = await verifyOtp(otpEmail, code);
    if (success) {
      toast.success("Welcome back!");
      navigate("/");
    } else {
      toast.error("Invalid or expired code");
      setOtp(["", "", "", ""]);
      otpRefs[0].current?.focus();
    }
  };

  useEffect(() => {
    if (otpRequired) otpRefs[0].current?.focus();
  }, [otpRequired]);

  if (otpRequired) {
    return (
      <div className="min-h-[70vh] flex items-center justify-center px-4 py-12">
        <SEO title="Verify Code" description="Enter your verification code to complete sign in." url="https://shireenbynaeema.com/login" />
        <Card className="w-full max-w-md">
          <CardContent className="p-8">
            <button onClick={() => { clearOtp(); setOtp(["", "", "", ""]); }} className="flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground mb-6">
              <ArrowLeft size={14} /> Back to sign in
            </button>
            <div className="text-center mb-6">
              <div className="w-12 h-12 bg-muted rounded-full flex items-center justify-center mx-auto mb-4">
                <Shield size={20} className="text-muted-foreground" />
              </div>
              <h1 className="text-2xl font-light" style={{ fontFamily: "Georgia, serif" }}>Verification Code</h1>
              <p className="text-sm text-muted-foreground mt-2">
                We sent a 4-digit code to<br />
                <span className="font-medium text-foreground">{otpEmail}</span>
              </p>
            </div>
            <div className="flex justify-center gap-3 mb-6">
              {otp.map((digit, i) => (
                <Input
                  key={i}
                  ref={otpRefs[i]}
                  type="text"
                  inputMode="numeric"
                  maxLength={1}
                  value={digit}
                  onChange={(e) => handleOtpChange(i, e.target.value)}
                  onKeyDown={(e) => handleOtpKeyDown(i, e)}
                  onPaste={handleOtpPaste}
                  className="w-14 h-14 text-center text-xl font-medium"
                />
              ))}
            </div>
            <Button onClick={handleVerifyOtp} disabled={loading || otp.join("").length !== 4} className="w-full" size="lg">
              {loading ? "Verifying..." : "Verify"}
            </Button>
            <p className="text-center text-xs text-muted-foreground mt-4">
              Code expires in 10 minutes
            </p>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="min-h-[70vh] flex items-center justify-center px-4 py-12">
      <SEO
        title="Sign In"
        description="Sign in to your Shireen by Naeema account. Access your orders, track deliveries, and manage your profile."
        url="https://shireenbynaeema.com/login"
      />
      <Card className="w-full max-w-md">
        <CardContent className="p-8">
          <h1 className="text-2xl font-light text-center mb-8" style={{ fontFamily: "Georgia, serif" }}>Sign In</h1>
          <form onSubmit={handleLogin} className="space-y-4">
            <div><Label className="mb-1.5 block">Email</Label><Input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required /></div>
            <div><Label className="mb-1.5 block">Password</Label><Input type="password" value={password} onChange={(e) => setPassword(e.target.value)} required /></div>
            <Button type="submit" disabled={loading} className="w-full" size="lg">{loading ? "Signing in..." : "Sign In"}</Button>
          </form>
          <p className="text-center text-sm text-muted-foreground mt-6">
            Don&apos;t have an account? <Link to="/register" className="text-foreground underline">Create one</Link>
          </p>
        </CardContent>
      </Card>
    </div>
  );
}
