import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { returnApi } from "@/services/api";
import type { Return } from "@/types";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { statusColors } from "@/lib/statusColors";

export default function MyReturns() {
  const [returns, setReturns] = useState<Return[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => { returnApi.getMyReturns().then((res) => setReturns(res.data.data || [])).finally(() => setLoading(false)); }, []);

  return (
    <div className="max-w-[1400px] mx-auto px-4 sm:px-6 lg:px-8 py-8 lg:py-12">
      <h1 className="text-2xl sm:text-3xl font-light mb-8" style={{ fontFamily: "Georgia, serif" }}>My Returns</h1>
      {loading ? (
        <div className="space-y-4">{Array.from({ length: 3 }).map((_, i) => <Skeleton key={i} className="h-24 w-full" />)}</div>
      ) : returns.length === 0 ? (
        <Card><CardContent className="p-8 text-center">
          <p className="text-muted-foreground mb-4">No return requests yet.</p>
          <Link to="/account/orders" className="text-sm underline text-foreground">View Orders</Link>
        </CardContent></Card>
      ) : (
        <div className="space-y-4">
          {returns.map((ret) => (
            <Card key={ret.id}>
              <CardContent className="p-4">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="font-medium">Order: {ret.orderNumber}</p>
                    <p className="text-sm text-muted-foreground">{ret.requestedOn || ""} — {ret.items?.length || 0} item(s)</p>
                    <p className="text-xs text-muted-foreground mt-1">{ret.reason}</p>
                  </div>
                  <div className="text-right">
                    <Badge className={statusColors[ret.status]}>{ret.status}</Badge>
                    {ret.refundAmount != null && <p className="text-sm font-medium mt-1">Rs. {ret.refundAmount.toLocaleString()}</p>}
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
