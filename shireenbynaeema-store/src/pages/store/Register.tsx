import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuthStore } from "@/store/authStore";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Card, CardContent } from "@/components/ui/card";
import toast from "react-hot-toast";
import SEO from "@/components/seo/SEO";

export default function Register() {
  const [form, setForm] = useState({ firstName: "", lastName: "", email: "", password: "", confirmPassword: "" });
  const { register, loading } = useAuthStore();
  const navigate = useNavigate();

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => setForm({ ...form, [e.target.name]: e.target.value });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (form.password !== form.confirmPassword) { toast.error("Passwords do not match"); return; }
    const success = await register({ firstName: form.firstName, lastName: form.lastName, email: form.email, password: form.password });
    if (success) { toast.success("Account created! Please sign in."); navigate("/login"); }
    else { toast.error("Registration failed"); }
  };

  return (
    <div className="min-h-[70vh] flex items-center justify-center px-4 py-12">
      <SEO
        title="Create Account"
        description="Create your Shireen by Naeema account. Join us for exclusive access to new collections, order tracking, and more."
        url="https://shireenbynaeema.com/register"
      />
      <Card className="w-full max-w-md">
        <CardContent className="p-8">
          <h1 className="text-2xl font-light text-center mb-8" style={{ fontFamily: "Georgia, serif" }}>Create Account</h1>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div><Label className="mb-1.5 block">First Name</Label><Input name="firstName" value={form.firstName} onChange={handleChange} required /></div>
              <div><Label className="mb-1.5 block">Last Name</Label><Input name="lastName" value={form.lastName} onChange={handleChange} required /></div>
            </div>
            <div><Label className="mb-1.5 block">Email</Label><Input name="email" type="email" value={form.email} onChange={handleChange} required /></div>
            <div><Label className="mb-1.5 block">Password</Label><Input name="password" type="password" value={form.password} onChange={handleChange} required minLength={8} /></div>
            <div><Label className="mb-1.5 block">Confirm Password</Label><Input name="confirmPassword" type="password" value={form.confirmPassword} onChange={handleChange} required /></div>
            <Button type="submit" disabled={loading} className="w-full" size="lg">{loading ? "Creating account..." : "Create Account"}</Button>
          </form>
          <p className="text-center text-sm text-muted-foreground mt-6">
            Already have an account? <Link to="/login" className="text-foreground underline">Sign in</Link>
          </p>
        </CardContent>
      </Card>
    </div>
  );
}
