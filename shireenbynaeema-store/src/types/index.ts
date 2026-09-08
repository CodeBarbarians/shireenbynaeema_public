export interface Product {
  id: string;
  name: string;
  slug: string;
  description: string;
  sku: string;
  basePrice: number;
  salePrice?: number;
  categoryId: string;
  categoryName: string;
  category?: Category;
  brand: string;
  isFeatured: boolean;
  isActive: boolean;
  rating: number;
  reviewCount: number;
  variants: ProductVariant[];
  images: ProductImage[];
  sizeChartJson: string;
  createdOn?: number;
}

export interface ProductVariant {
  id: string;
  productId: string;
  size: string;
  color: string;
  colorHex: string;
  stock: number;
  bookedStock: number;
  price: number;
  sku: string;
  isActive: boolean;
}

export interface ProductImage {
  id: string;
  productId: string;
  imageUrl: string;
  altText: string;
  sortOrder: number;
  isPrimary: boolean;
}

export interface Category {
  id: string;
  name: string;
  slug: string;
  description: string;
  parentId?: string;
  imageUrl: string;
  sortOrder: number;
  isActive: boolean;
  showInNav: boolean;
  children?: Category[];
}

export interface Customer {
  id: string;
  userId?: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  addresses: Address[];
  orders?: Order[];
  invoices?: Invoice[];
  orderCount: number;
  totalSpent: number;
  createdOn?: number;
}

export interface Address {
  id: string;
  customerId: string;
  label: string;
  street: string;
  city: string;
  state: string;
  zipCode: string;
  country: string;
  isDefault: boolean;
}

export interface Cart {
  id: string;
  userId: string;
  items: CartItem[];
  subtotal: number;
}

export interface CartItem {
  id: string;
  cartId: string;
  productVariantId: string;
  variant?: ProductVariant;
  product?: Product;
  quantity: number;
  unitPrice: number;
}

export interface Order {
  id: string;
  orderNumber: string;
  customerId: string;
  customerName?: string;
  customerEmail?: string;
  customer?: Customer;
  status: OrderStatus;
  subtotal: number;
  tax: number;
  shipping: number;
  discount: number;
  couponCode?: string;
  total: number;
  shippingAddress?: Address;
  notes?: string;
  items: OrderItem[];
  createdOn?: number;
}

export interface OrderItem {
  id: string;
  orderId: string;
  productVariantId: string;
  productName?: string;
  size?: string;
  color?: string;
  imageUrl?: string;
  variant?: ProductVariant;
  product?: Product;
  quantity: number;
  unitPrice: number;
  total: number;
  deliveredQuantity?: number;
  returnedQuantity?: number;
}

export type OrderStatus = "Pending" | "Confirmed" | "Processing" | "Shipped" | "Delivered" | "Cancelled" | "Returned" | "Partially Delivered" | "Partially Returned" | "PaymentFailed";

export interface Invoice {
  id: string;
  invoiceNumber: string;
  orderId: string;
  orderNumber: string;
  order?: Order;
  amount: number;
  tax: number;
  status: InvoiceStatus;
  dueDate?: string;
  paidDate?: string;
  createdOn?: number;
}

export type InvoiceStatus = "Pending" | "Paid" | "Overdue" | "Cancelled";

export interface Delivery {
  id: string;
  orderId: string;
  orderNumber: string;
  order?: Order;
  carrier: string;
  trackingNumber: string;
  status: DeliveryStatus;
  shippedOn?: string;
  deliveredOn?: string;
  estimatedDelivery?: string;
  items?: DeliveryItem[];
}

export interface DeliveryItem {
  id: string;
  orderItemId: string;
  productName: string;
  size: string;
  color: string;
  quantity: number;
  orderedQuantity: number;
}

export type DeliveryStatus = "Pending" | "InTransit" | "OutForDelivery" | "Delivered" | "Exception";

export interface Return {
  id: string;
  orderId: string;
  orderNumber: string;
  order?: Order;
  reason: string;
  status: ReturnStatus;
  refundAmount?: number;
  refundPaymentId?: string;
  bankAccount: string;
  bankName: string;
  accountHolderName: string;
  attachments: string;
  notes?: string;
  requestedOn?: string;
  processedOn?: string;
  items?: ReturnItem[];
}

export interface ReturnItem {
  id: string;
  orderItemId: string;
  productName: string;
  size: string;
  color: string;
  quantity: number;
  orderedQuantity: number;
  reason: string;
}

export type ReturnStatus = "Requested" | "Approved" | "Rejected" | "InCheckup" | "Refunded" | "Completed";

export interface DashboardStats {
  totalRevenue: number;
  totalOrders: number;
  totalCustomers: number;
  totalProducts: number;
}

export interface Review {
  id: string;
  userName: string;
  score: number;
  comment: string;
  createdOn: number;
}

export interface SizeChartEntry {
  size: string;
  bust: string;
  waist: string;
  hips: string;
  length: string;
}

export interface PaginatedResponse<T> {
  entities: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}

export interface ApiResponse<T> {
  isSuccess: boolean;
  message: string;
  data: T;
}

export interface Coupon {
  id: string;
  code: string;
  description: string;
  discountType: string;
  discountValue: number;
  minOrderAmount?: number;
  maxUses?: number;
  usedCount: number;
  expiresOn?: string;
  isActive: boolean;
}

export interface BlogPost {
  id: string;
  title: string;
  slug: string;
  excerpt: string;
  content: string;
  coverImageUrl: string;
  author: string;
  isPublished: boolean;
  publishedOn?: number;
  viewCount: number;
}
