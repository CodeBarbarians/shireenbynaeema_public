import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { customerApi } from "@/services/api";
import type { Customer } from "@/types";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Skeleton } from "@/components/ui/skeleton";
import { Trash2 } from "lucide-react";
import toast from "react-hot-toast";

export default function AdminCustomers() {
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [loading, setLoading] = useState(true);
  useEffect(() => { customerApi.list().then((res) => setCustomers(res.data.data?.entities || [])).finally(() => setLoading(false)); }, []);

  const handleDelete = async (id: string) => {
    if (!confirm("Delete this customer and all addresses?")) return;
    await customerApi.delete(id);
    setCustomers((prev) => prev.filter((c) => c.id !== id));
    toast.success("Customer deleted");
  };

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-semibold tracking-tight">Customers</h1>
      <Card><CardContent className="p-0">
        <div className="overflow-x-auto">
        <Table>
          <TableHeader><TableRow><TableHead>Name</TableHead><TableHead className="hidden sm:table-cell">Email</TableHead><TableHead className="hidden md:table-cell">Phone</TableHead><TableHead className="hidden sm:table-cell">Orders</TableHead><TableHead>Total Spent</TableHead><TableHead className="w-[50px]" /></TableRow></TableHeader>
          <TableBody>
            {loading ? Array.from({ length: 5 }).map((_, i) => <TableRow key={i}>{Array.from({ length: 6 }).map((__, j) => <TableCell key={j}><Skeleton className="h-4 w-full" /></TableCell>)}</TableRow>) :
              customers.length === 0 ? <TableRow><TableCell colSpan={6} className="text-center py-8 text-muted-foreground">No customers yet</TableCell></TableRow> :
              customers.map((c) => <TableRow key={c.id} className="hover:bg-muted/50"><TableCell className="font-medium"><Link to={`/admin/customers/${c.id}`} className="hover:underline">{c.firstName} {c.lastName}</Link><span className="block text-xs text-muted-foreground sm:hidden">{c.email}</span></TableCell><TableCell className="hidden sm:table-cell text-muted-foreground">{c.email}</TableCell><TableCell className="hidden md:table-cell text-muted-foreground">{c.phone || "-"}</TableCell><TableCell className="hidden sm:table-cell">{c.orderCount}</TableCell><TableCell>Rs. {c.totalSpent.toLocaleString()}</TableCell><TableCell><Button variant="ghost" size="icon" className="h-8 w-8 text-destructive hover:text-destructive" onClick={() => handleDelete(c.id)}><Trash2 size={14} /></Button></TableCell></TableRow>)}
          </TableBody>
        </Table>
        </div>
      </CardContent></Card>
    </div>
  );
}
