import { useEffect, useState } from "react";
import { reportApi, categoryApi, productApi } from "@/services/api";
import type { Category, Product } from "@/types";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Skeleton } from "@/components/ui/skeleton";
import { Download, BarChart3, TrendingUp, Package } from "lucide-react";
import toast from "react-hot-toast";

type ReportType = "sales" | "category" | "product";

function ReportTable({ reportType, fromDate, toDate, productId, categoryId }: { reportType: ReportType; fromDate: string; toDate: string; productId: string; categoryId: string }) {
  const [loading, setLoading] = useState(true);
  const [items, setItems] = useState<any[]>([]);
  const [summary, setSummary] = useState<any>(null);

  useEffect(() => {
    let active = true;
    setLoading(true);

    const params: any = {};
    if (fromDate) params.fromDate = Math.floor(new Date(fromDate).getTime() / 1000);
    if (toDate) params.toDate = Math.floor(new Date(toDate + "T23:59:59").getTime() / 1000);
    if (productId !== "all") params.productId = productId;
    if (categoryId !== "all") params.categoryId = categoryId;

    const p = reportType === "sales" ? reportApi.sales(params)
      : reportType === "category" ? reportApi.category(params)
      : reportApi.product(params);

    p.then((res) => {
      if (!active) return;
      if (res.data.isSuccess) {
        const d = res.data.data;
        const rows = d.items || [];
        setItems(rows);

        if (d.summary) {
          setSummary(d.summary);
        } else {
          const totalRevenue = rows.reduce((s: number, r: any) => s + (r.totalRevenue || r.total || 0), 0);
          const totalItems = rows.reduce((s: number, r: any) => s + (r.totalQuantity || r.quantity || 0), 0);
          const totalOrders = rows.reduce((s: number, r: any) => s + (r.orderCount || 0), 0);
          setSummary({
            totalRevenue,
            totalItems,
            totalOrders,
            averageOrderValue: totalOrders > 0 ? totalRevenue / totalOrders : 0,
          });
        }
      }
      setLoading(false);
    }).catch(() => {
      if (active) setLoading(false);
    });

    return () => { active = false; };
  }, [reportType, fromDate, toDate, productId, categoryId]);

  if (loading) return <CardContent className="p-6 space-y-3">{Array.from({ length: 5 }).map((_, i) => <Skeleton key={i} className="h-10 w-full" />)}</CardContent>;

  return (
    <>
      {summary && (
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4 p-4 pb-0">
          {[
            { label: "Total Revenue", value: `Rs. ${summary.totalRevenue.toLocaleString()}` },
            { label: "Total Orders", value: summary.totalOrders },
            { label: "Items Sold", value: summary.totalItems },
            { label: "Avg Order Value", value: `Rs. ${summary.averageOrderValue.toLocaleString()}` },
          ].map((s) => (
            <div key={s.label} className="text-center p-3 bg-muted/50 rounded-lg">
              <p className="text-xl font-semibold">{s.value}</p>
              <p className="text-xs text-muted-foreground mt-1">{s.label}</p>
            </div>
          ))}
        </div>
      )}
      <CardContent className="p-0">
      <Table>
        <TableHeader>
          {reportType === "sales" && <TableRow><TableHead>Order #</TableHead><TableHead>Date</TableHead><TableHead>Product</TableHead><TableHead>Category</TableHead><TableHead>Qty</TableHead><TableHead>Total</TableHead><TableHead>Status</TableHead></TableRow>}
          {reportType === "category" && <TableRow><TableHead>Category</TableHead><TableHead>Orders</TableHead><TableHead>Items Sold</TableHead><TableHead>Revenue</TableHead></TableRow>}
          {reportType === "product" && <TableRow><TableHead>Product</TableHead><TableHead>Orders</TableHead><TableHead>Items Sold</TableHead><TableHead>Revenue</TableHead><TableHead>Avg Price</TableHead></TableRow>}
        </TableHeader>
        <TableBody>
          {items.length === 0 && <TableRow><TableCell colSpan={7} className="text-center py-8 text-muted-foreground">No data for selected filters</TableCell></TableRow>}
          {reportType === "sales" && items.map((r: any, i: number) => (
            <TableRow key={i}><TableCell className="font-medium text-xs">{r.orderNumber}</TableCell><TableCell className="text-xs text-muted-foreground">{r.orderDate}</TableCell><TableCell className="text-xs">{r.productName}</TableCell><TableCell className="text-xs text-muted-foreground">{r.category}</TableCell><TableCell className="text-xs">{r.quantity}</TableCell><TableCell className="text-xs font-medium">Rs. {r.total.toLocaleString()}</TableCell><TableCell className="text-xs text-muted-foreground">{r.status}</TableCell></TableRow>
          ))}
          {reportType === "category" && items.map((r: any, i: number) => (
            <TableRow key={i}><TableCell className="font-medium">{r.category}</TableCell><TableCell>{r.orderCount}</TableCell><TableCell>{r.totalQuantity}</TableCell><TableCell className="font-medium">Rs. {r.totalRevenue.toLocaleString()}</TableCell></TableRow>
          ))}
          {reportType === "product" && items.map((r: any, i: number) => (
            <TableRow key={i}><TableCell className="font-medium">{r.productName}</TableCell><TableCell>{r.orderCount}</TableCell><TableCell>{r.totalQuantity}</TableCell><TableCell className="font-medium">Rs. {r.totalRevenue.toLocaleString()}</TableCell><TableCell className="text-muted-foreground">Rs. {r.averagePrice.toLocaleString()}</TableCell></TableRow>
          ))}
        </TableBody>
      </Table>
    </CardContent>
    </>
  );
}

export default function AdminReports() {
  const [reportType, setReportType] = useState<ReportType>("sales");
  const [categories, setCategories] = useState<Category[]>([]);
  const [products, setProducts] = useState<Product[]>([]);
  const [fromDate, setFromDate] = useState("");
  const [toDate, setToDate] = useState("");
  const [productId, setProductId] = useState("all");
  const [categoryId, setCategoryId] = useState("all");

  useEffect(() => {
    categoryApi.list().then((res) => setCategories(res.data.data?.entities || []));
    productApi.list().then((res) => setProducts(res.data.data?.entities || []));
  }, []);

  const handleExport = async () => {
    const params: any = {};
    if (fromDate) params.fromDate = Math.floor(new Date(fromDate).getTime() / 1000);
    if (toDate) params.toDate = Math.floor(new Date(toDate + "T23:59:59").getTime() / 1000);
    if (productId !== "all") params.productId = productId;
    if (categoryId !== "all") params.categoryId = categoryId;
    try {
      let res;
      if (reportType === "sales") res = await reportApi.exportSales(params);
      else if (reportType === "category") res = await reportApi.exportCategory(params);
      else res = await reportApi.exportProduct(params);
      const blob = new Blob([res.data], { type: "application/pdf" });
      const url = URL.createObjectURL(blob);
      const a = document.createElement("a");
      a.href = url; a.download = `${reportType}-report.pdf`; a.click();
      URL.revokeObjectURL(url);
      toast.success("Report exported");
    } catch { toast.error("Export failed"); }
  };

  const tabs = [
    { key: "sales" as ReportType, label: "Sales", icon: TrendingUp },
    { key: "category" as ReportType, label: "By Category", icon: BarChart3 },
    { key: "product" as ReportType, label: "By Product", icon: Package },
  ];

  const filterKey = `${reportType}-${fromDate}-${toDate}-${productId}-${categoryId}`;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between flex-wrap gap-3">
        <h1 className="text-2xl font-semibold tracking-tight">Reports</h1>
        <Button size="sm" onClick={handleExport}><Download size={14} className="mr-1" />Export PDF</Button>
      </div>

      <div className="flex gap-2 flex-wrap">
        {tabs.map((tab) => {
          const Icon = tab.icon;
          return (
            <Button key={tab.key} variant={reportType === tab.key ? "default" : "outline"} size="sm" onClick={() => setReportType(tab.key)}>
              <Icon size={14} className="mr-1" />{tab.label}
            </Button>
          );
        })}
      </div>

      <Card><CardContent className="p-4">
        <div className="flex flex-wrap gap-4 items-end">
          <div className="w-full sm:w-40"><Label className="text-xs">From Date</Label><Input type="date" value={fromDate} onChange={(e) => setFromDate(e.target.value)} className="h-9 text-sm" /></div>
          <div className="w-full sm:w-40"><Label className="text-xs">To Date</Label><Input type="date" value={toDate} onChange={(e) => setToDate(e.target.value)} className="h-9 text-sm" /></div>
          <div className="w-full sm:w-48"><Label className="text-xs">Category</Label><Select value={categoryId} onValueChange={setCategoryId}><SelectTrigger className="h-9"><SelectValue placeholder="All" /></SelectTrigger><SelectContent><SelectItem value="all">All Categories</SelectItem>{categories.map((c) => <SelectItem key={c.id} value={c.id}>{c.name}</SelectItem>)}</SelectContent></Select></div>
          <div className="w-full sm:w-48"><Label className="text-xs">Product</Label><Select value={productId} onValueChange={setProductId}><SelectTrigger className="h-9"><SelectValue placeholder="All" /></SelectTrigger><SelectContent><SelectItem value="all">All Products</SelectItem>{products.slice(0, 50).map((p) => <SelectItem key={p.id} value={p.id}>{p.name}</SelectItem>)}</SelectContent></Select></div>
        </div>
      </CardContent></Card>

      <Card key={filterKey}>
        <ReportTable reportType={reportType} fromDate={fromDate} toDate={toDate} productId={productId} categoryId={categoryId} />
      </Card>
    </div>
  );
}
