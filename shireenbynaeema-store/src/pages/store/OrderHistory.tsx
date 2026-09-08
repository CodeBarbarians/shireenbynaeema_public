import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { orderApi } from "@/services/api";
import type { Order } from "@/types";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { statusColors } from "@/lib/statusColors";

export default function OrderHistory() {
  const [orders, setOrders] = useState<Order[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => { orderApi.history().then((res) => setOrders(res.data.data?.entities || [])).finally(() => setLoading(false)); }, []);

  return (
    <div className="max-w-[1400px] mx-auto px-4 sm:px-6 lg:px-8 py-8 lg:py-12">
      <h1 className="text-2xl sm:text-3xl font-light mb-8" style={{ fontFamily: "Georgia, serif" }}>Order History</h1>
      {loading ? (
        <div className="space-y-4">{Array.from({ length: 3 }).map((_, i) => <Skeleton key={i} className="h-24 w-full" />)}</div>
      ) : orders.length === 0 ? (
        <Card><CardContent className="p-8 text-center">
          <p className="text-muted-foreground mb-4">No orders yet.</p>
          <Link to="/collections/all" className="text-sm underline text-foreground">Start Shopping</Link>
        </CardContent></Card>
      ) : (
        <div className="space-y-4">
          {orders.map((order) => (
            <Card key={order.id}>
              <CardContent className="p-4">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="font-medium">{order.orderNumber}</p>
                    <p className="text-sm text-muted-foreground">{order.createdOn ? new Date(order.createdOn * 1000).toLocaleDateString() : ""} — {order.items?.length || 0} items</p>
                  </div>
                  <div className="text-right">
                    <Badge className={statusColors[order.status]}>{order.status}</Badge>
                    <p className="text-sm font-medium mt-1">Rs. {order.total.toLocaleString()} PKR</p>
                    {(order.status === "Delivered" || order.status === "Partially Delivered") && (
                      <Link to={`/account/returns/${order.id}`}>
                        <Button variant="ghost" size="sm" className="mt-1 text-xs h-7">Request Return</Button>
                      </Link>
                    )}
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
