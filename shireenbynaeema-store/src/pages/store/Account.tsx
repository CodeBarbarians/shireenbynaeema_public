import { useAuthStore } from "@/store/authStore";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Link } from "react-router-dom";
import { ShoppingBag, User } from "lucide-react";

export default function Account() {
  const { user, logout } = useAuthStore();

  return (
    <div className="max-w-[1400px] mx-auto px-4 sm:px-6 lg:px-8 py-8 lg:py-12">
      <div className="flex items-center justify-between mb-8">
        <h1 className="text-2xl sm:text-3xl font-light" style={{ fontFamily: "Georgia, serif" }}>My Account</h1>
        <Button variant="ghost" onClick={logout} className="text-sm text-muted-foreground">Sign Out</Button>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        <div className="lg:col-span-1">
          <Card>
            <CardContent className="p-6 text-center">
              <div className="w-16 h-16 bg-muted rounded-full mx-auto flex items-center justify-center text-xl font-light text-muted-foreground mb-4">
                <User size={24} />
              </div>
              <p className="text-sm font-medium">{user?.displayName}</p>
              <p className="text-xs text-muted-foreground mb-4">{user?.email}</p>
              <nav className="space-y-1">
                <Link to="/account" className="block px-3 py-2 text-sm bg-muted rounded font-medium">Profile</Link>
                <Link to="/account/orders" className="block px-3 py-2 text-sm text-muted-foreground hover:bg-muted rounded">Order History</Link>
                <Link to="/account/returns" className="block px-3 py-2 text-sm text-muted-foreground hover:bg-muted rounded">My Returns</Link>
              </nav>
            </CardContent>
          </Card>
        </div>

        <div className="lg:col-span-2">
          <Card>
            <CardContent className="p-6 text-center py-12">
              <ShoppingBag size={48} className="mx-auto text-muted-foreground mb-4" />
              <p className="text-sm text-muted-foreground mb-4">Manage your profile and view order history.</p>
              <Link to="/collections/all"><Button variant="outline">Continue Shopping</Button></Link>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
