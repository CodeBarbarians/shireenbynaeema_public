import { useEffect, useState } from "react";
import { dashboardApi } from "@/services/api";
import { TrendingUp, ShoppingCart, Users, Package } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";

interface Stats { totalRevenue: number; totalOrders: number; totalCustomers: number; totalProducts: number; }

export default function Dashboard() {
  const [stats, setStats] = useState<Stats>({ totalRevenue: 0, totalOrders: 0, totalCustomers: 0, totalProducts: 0 });
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    dashboardApi.stats().then((res) => setStats(res.data.data || { totalRevenue: 0, totalOrders: 0, totalCustomers: 0, totalProducts: 0 })).finally(() => setLoading(false));
  }, []);

  const cards = [
    { label: "Revenue", value: `Rs. ${(stats.totalRevenue || 0).toLocaleString()}`, icon: TrendingUp, color: "text-green-600 bg-green-50" },
    { label: "Orders", value: stats.totalOrders || 0, icon: ShoppingCart, color: "text-blue-600 bg-blue-50" },
    { label: "Customers", value: stats.totalCustomers || 0, icon: Users, color: "text-purple-600 bg-purple-50" },
    { label: "Products", value: stats.totalProducts || 0, icon: Package, color: "text-amber-600 bg-amber-50" },
  ];

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-semibold tracking-tight">Dashboard</h1>
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        {loading ? Array.from({ length: 4 }).map((_, i) => <Skeleton key={i} className="h-[108px] rounded-lg" />) :
          cards.map((card) => {
            const Icon = card.icon;
            return (
              <Card key={card.label}>
                <CardHeader className="flex flex-row items-center justify-between pb-2">
                  <CardTitle className="text-sm font-medium text-muted-foreground">{card.label}</CardTitle>
                  <div className={`p-2 rounded-lg ${card.color}`}><Icon size={18} /></div>
                </CardHeader>
                <CardContent><p className="text-2xl font-bold">{card.value}</p></CardContent>
              </Card>
            );
          })}
      </div>
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <Card><CardHeader><CardTitle className="text-sm font-medium">Recent Orders</CardTitle></CardHeader><CardContent><p className="text-sm text-muted-foreground">No recent orders yet.</p></CardContent></Card>
        <Card><CardHeader><CardTitle className="text-sm font-medium">Top Products</CardTitle></CardHeader><CardContent><p className="text-sm text-muted-foreground">No product data yet.</p></CardContent></Card>
      </div>
    </div>
  );
}
