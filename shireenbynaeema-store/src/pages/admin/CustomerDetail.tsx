import { useEffect, useState } from "react";
import { useParams, Link, useNavigate } from "react-router-dom";
import { customerApi } from "@/services/api";
import type { Customer } from "@/types";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Skeleton } from "@/components/ui/skeleton";
import { statusColors } from "@/lib/statusColors";
import { Trash2 } from "lucide-react";
import toast from "react-hot-toast";

export default function AdminCustomerDetail() {
  const { id } = useParams();
  const [customer, setCustomer] = useState<Customer | null>(null);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  useEffect(() => {
    if (!id) return;
    customerApi.getById(id).then((res) => setCustomer(res.data.data || null)).finally(() => setLoading(false));
  }, [id]);

  if (loading) return <div className="space-y-4">{Array.from({ length: 4 }).map((_, i) => <Skeleton key={i} className="h-16 w-full" />)}</div>;
  if (!customer) return <div className="text-center py-16 text-muted-foreground">Customer not found</div>;

  const handleDelete = async () => {
    if (!id || !confirm("Delete this customer and all addresses?")) return;
    await customerApi.delete(id);
    toast.success("Customer deleted");
    navigate("/admin/customers");
  };

  return (
    <div className="max-w-[1400px] mx-auto px-4 sm:px-6 lg:px-8 py-8 lg:py-12">
      <div className="flex items-start justify-between mb-8 gap-4 flex-wrap">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">{customer.firstName} {customer.lastName}</h1>
          <p className="text-sm text-muted-foreground">{customer.email}</p>
        </div>
        <div className="flex gap-2 flex-wrap">
          <Button variant="outline" size="sm" onClick={handleDelete} className="text-destructive hover:text-destructive"><Trash2 size={14} /></Button>
          <Link to="/admin/customers"><Button variant="outline" size="sm">Back to Customers</Button></Link>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6 mb-8">
        <Card><CardContent className="p-6 space-y-3">
          <div className="text-sm"><span className="text-muted-foreground">Phone:</span> <span className="ml-2">{customer.phone || "-"}</span></div>
          <div className="text-sm"><span className="text-muted-foreground">Email:</span> <span className="ml-2">{customer.email}</span></div>
          <div className="text-sm"><span className="text-muted-foreground">Registered:</span> <span className="ml-2">{customer.createdOn ? new Date(customer.createdOn * 1000).toLocaleDateString() : "-"}</span></div>
        </CardContent></Card>
        <Card><CardContent className="p-6 space-y-3">
          <div className="text-sm"><span className="text-muted-foreground">Total Orders:</span> <span className="ml-2 font-medium">{customer.orderCount}</span></div>
          <div className="text-sm"><span className="text-muted-foreground">Total Spent:</span> <span className="ml-2 font-medium">Rs. {customer.totalSpent.toLocaleString()}</span></div>
        </CardContent></Card>
        <Card><CardContent className="p-6 space-y-3">
          <h3 className="text-sm font-medium mb-2">Addresses</h3>
          {customer.addresses?.length ? customer.addresses.map((a) => (
            <div key={a.id} className="text-sm text-muted-foreground">
              <span className="font-medium text-foreground">{a.label}</span>{a.isDefault && <Badge variant="outline" className="ml-2 text-[10px]">Default</Badge>}
              <br />{a.street}<br />{a.city}, {a.state} {a.zipCode}<br />{a.country}
            </div>
          )) : <p className="text-sm text-muted-foreground">No addresses</p>}
        </CardContent></Card>
      </div>

      <div className="space-y-6">
        <Card>
          <CardHeader><CardTitle className="text-base">Orders ({customer.orders?.length || 0})</CardTitle></CardHeader>
          <CardContent className="p-0">
            <Table>
              <TableHeader><TableRow><TableHead>Order #</TableHead><TableHead>Date</TableHead><TableHead>Items</TableHead><TableHead>Total</TableHead><TableHead>Status</TableHead></TableRow></TableHeader>
              <TableBody>
                {!customer.orders?.length ? <TableRow><TableCell colSpan={5} className="text-center py-8 text-muted-foreground">No orders</TableCell></TableRow> :
                  customer.orders.map((o) => (
                    <TableRow key={o.id}>
                      <TableCell className="font-medium"><Link to={`/admin/orders/${o.id}`} className="hover:underline">{o.orderNumber}</Link></TableCell>
                      <TableCell className="text-muted-foreground">{o.createdOn ? new Date(o.createdOn * 1000).toLocaleDateString() : "-"}</TableCell>
                      <TableCell>{o.items?.length || 0}</TableCell>
                      <TableCell>Rs. {o.total.toLocaleString()}</TableCell>
                      <TableCell><Badge className={statusColors[o.status]}>{o.status}</Badge></TableCell>
                    </TableRow>
                  ))
                }
              </TableBody>
            </Table>
          </CardContent>
        </Card>

        <Card>
          <CardHeader><CardTitle className="text-base">Invoices ({customer.invoices?.length || 0})</CardTitle></CardHeader>
          <CardContent className="p-0">
            <Table>
              <TableHeader><TableRow><TableHead>Invoice #</TableHead><TableHead>Order</TableHead><TableHead>Amount</TableHead><TableHead>Due</TableHead><TableHead>Status</TableHead></TableRow></TableHeader>
              <TableBody>
                {!customer.invoices?.length ? <TableRow><TableCell colSpan={5} className="text-center py-8 text-muted-foreground">No invoices</TableCell></TableRow> :
                  customer.invoices.map((inv) => (
                    <TableRow key={inv.id}>
                      <TableCell className="font-medium">{inv.invoiceNumber}</TableCell>
                      <TableCell className="text-muted-foreground"><Link to={`/admin/orders/${inv.orderId}`} className="hover:underline">{inv.orderNumber}</Link></TableCell>
                      <TableCell>Rs. {inv.amount.toLocaleString()}</TableCell>
                      <TableCell className="text-muted-foreground">{inv.dueDate ? new Date(inv.dueDate).toLocaleDateString() : "-"}</TableCell>
                      <TableCell><Badge className={statusColors[inv.status]}>{inv.status}</Badge></TableCell>
                    </TableRow>
                  ))
                }
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
