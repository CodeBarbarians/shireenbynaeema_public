import axios from "axios";

const API_BASE = import.meta.env.VITE_API_URL || "https://shireenbynaeema-production.up.railway.app/api";

const api = axios.create({
  baseURL: API_BASE,
  headers: { "Content-Type": "application/json" },
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401 && localStorage.getItem("token")) {
      localStorage.removeItem("token");
      localStorage.removeItem("user");
      window.location.href = "/login";
    }
    return Promise.reject(error);
  }
);

export const authApi = {
  login: (data: { email: string; password: string }) => api.post("/Auth/Login", data),
  register: (data: { firstName: string; lastName: string; email: string; password: string }) => api.post("/Auth/Register", data),
  verifyOtp: (data: { email: string; code: string; rememberMe?: boolean }) => api.post("/Auth/VerifyOTP", data),
  sendOtp: (email: string) => api.post("/Auth/SendOTP", { email }),
};

export const productApi = {
  list: (params?: Record<string, unknown>) => api.post("/Product/List", params || {}),
  getById: (id: string) => api.get(`/Product/${id}`),
  getByCategory: (slug: string) => api.get(`/Product/ByCategory/${slug}`),
  featured: () => api.get("/Product/Featured"),
  newArrivals: () => api.get("/Product/NewArrivals"),
  search: (q: string) => api.get(`/Product/Search`, { params: { q } }),
  create: (data: unknown) => api.post("/Product/Add", data),
  update: (data: unknown) => api.post("/Product/Update", data),
  delete: (id: string) => api.delete(`/Product/Delete/${id}`),
};

export const categoryApi = {
  list: () => api.post("/Category/List", {}),
  tree: () => api.get("/Category/Tree"),
  getById: (id: string) => api.get(`/Category/${id}`),
  getBySlug: (slug: string) => api.get(`/Category/BySlug/${slug}`),
  create: (data: unknown) => api.post("/Category/Add", data),
  update: (data: unknown) => api.post("/Category/Update", data),
  delete: (id: string) => api.delete(`/Category/Delete/${id}`),
};

export const cartApi = {
  get: () => api.get("/Cart"),
  addItem: (variantId: string, quantity: number) => api.post("/Cart/Add", { productVariantId: variantId, quantity }),
  updateQuantity: (cartItemId: string, quantity: number) => api.put("/Cart/Update", { quantity }, { params: { cartItemId } }),
  removeItem: (cartItemId: string) => api.delete(`/Cart/Remove/${cartItemId}`),
  clear: () => api.delete("/Cart/Clear"),
};

export const orderApi = {
  place: (data: { paymentIntentId: string; firstName: string; lastName: string; email: string; phone: string; street: string; city: string; state: string; zipCode: string; country: string; couponCode?: string; items?: { productVariantId: string; quantity: number; unitPrice: number }[] }) => api.post("/Order/Place", data),
  history: () => api.get("/Order/History"),
  getById: (id: string) => api.get(`/Order/${id}`),
  list: (params?: Record<string, unknown>) => api.post("/Order/Admin/List", params || {}),
  updateStatus: (id: string, status: string) => api.put(`/Order/Admin/UpdateStatus/${id}`, { status }),
  delete: (id: string) => api.delete(`/Order/Admin/Delete/${id}`),
};

export const paymentApi = {
  createIntent: (orderId: string) => api.post("/Payment/CreateIntent", { orderId }),
  confirm: (paymentIntentId: string) => api.post("/Payment/Confirm", { paymentIntentId }),
  processRefund: (paymentIntentId: string, amountInCents: number) => api.post("/Payment/Refund", { paymentIntentId, amountInCents }),
};

export const customerApi = {
  getProfile: () => api.get("/Customer/Profile"),
  updateProfile: (data: unknown) => api.put("/Customer/Profile", data),
  addAddress: (data: unknown) => api.post("/Customer/Address", data),
  updateAddress: (id: string, data: unknown) => api.put(`/Customer/Address/${id}`, data),
  deleteAddress: (id: string) => api.delete(`/Customer/Address/${id}`),
  list: (params?: Record<string, unknown>) => api.post("/Customer/Admin/List", params || {}),
  getById: (id: string) => api.get(`/Customer/Admin/${id}`),
  delete: (id: string) => api.delete(`/Customer/Admin/Delete/${id}`),
};

export const invoiceApi = {
  list: (params?: Record<string, unknown>) => api.post("/Invoice/Admin/List", params || {}),
  getById: (id: string) => api.get(`/Invoice/Admin/${id}`),
  generate: (orderId: string) => api.post("/Invoice/Admin/Generate", { orderId }),
  markPaid: (id: string, paymentId: string) => api.put(`/Invoice/Admin/MarkPaid/${id}`, { paymentId }),
  delete: (id: string) => api.delete(`/Invoice/Admin/Delete/${id}`),
};

export const deliveryApi = {
  list: (params?: Record<string, unknown>) => api.post("/Delivery/Admin/List", params || {}),
  getById: (id: string) => api.get(`/Delivery/Admin/${id}`),
  create: (data: { orderId: string; carrier: string; trackingNumber: string; estimatedDelivery?: string; items?: { orderItemId: string; quantity: number }[] }) => api.post("/Delivery/Admin/Create", data),
  updateTracking: (id: string, data: { status: string }) => api.put(`/Delivery/Admin/UpdateTracking/${id}`, data),
  delete: (id: string) => api.delete(`/Delivery/Admin/Delete/${id}`),
};

export const returnApi = {
  list: (params?: Record<string, unknown>) => api.post("/Return/Admin/List", params || {}),
  getById: (id: string) => api.get(`/Return/Admin/${id}`),
  getMyReturns: () => api.get("/Return/MyReturns"),
  request: (data: { orderId: string; reason: string; bankAccount: string; bankName: string; accountHolderName: string; attachments?: string; items?: { orderItemId: string; quantity: number; reason: string }[] }) => api.post("/Return/Request", data),
  process: (id: string, data: { status: string; refundAmount?: number; notes?: string }) => api.put(`/Return/Admin/Process/${id}`, data),
  delete: (id: string) => api.delete(`/Return/Admin/Delete/${id}`),
  uploadAttachment: (file: File) => {
    const formData = new FormData();
    formData.append("file", file);
    return api.post("/Return/UploadAttachment", formData, {
      headers: { "Content-Type": "multipart/form-data" },
    });
  },
};

export const imageApi = {
  upload: (file: File, productId?: string) => {
    const formData = new FormData();
    formData.append("file", file);
    return api.post("/Image/Upload", formData, {
      headers: { "Content-Type": "multipart/form-data" },
      params: productId ? { productId } : undefined,
    });
  },
  uploadCategory: (file: File, categoryId: string) => {
    const formData = new FormData();
    formData.append("file", file);
    return api.post("/Image/UploadCategory", formData, {
      headers: { "Content-Type": "multipart/form-data" },
      params: { categoryId },
    });
  },
  deleteProductImage: (id: string) => api.delete(`/Image/DeleteProductImage/${id}`),
  deleteCategoryImage: (categoryId: string) => api.delete(`/Image/DeleteCategoryImage/${categoryId}`),
  uploadCategoryImages: (file: File, categoryId: string) => {
    const formData = new FormData();
    formData.append("file", file);
    return api.post("/Image/UploadCategoryImages", formData, {
      headers: { "Content-Type": "multipart/form-data" },
      params: { categoryId },
    });
  },
  deleteCategoryImageById: (id: string) => api.delete(`/Image/DeleteCategoryImageById/${id}`),
};

export const cmsPageApi = {
  list: (params?: Record<string, unknown>) => api.post("/CmsPage/List", params || {}),
  getBySlug: (slug: string) => api.get(`/CmsPage/BySlug/${slug}`),
  create: (data: unknown) => api.post("/CmsPage/Add", data),
  update: (data: unknown) => api.post("/CmsPage/Update", data),
  delete: (id: string) => api.delete(`/CmsPage/Delete/${id}`),
};

export const settingApi = {
  getAll: () => api.get("/Setting"),
  getByKey: (key: string) => api.get(`/Setting/${key}`),
  update: (data: { key: string; value: string; description?: string }) => api.put("/Setting", data),
  bulkUpdate: (data: { key: string; value: string; description?: string }[]) => api.put("/Setting/Bulk", data),
  getSmtp: () => api.get("/Setting/Smtp"),
  saveSmtp: (config: any) => api.post("/Setting/Smtp", config),
  getHeroSlides: () => api.get("/Setting/HeroSlides"),
  saveHeroSlides: (slides: any[]) => api.post("/Setting/HeroSlides", slides),
};

export const dashboardApi = {
  stats: () => api.get("/Dashboard/Stats"),
};

export const reviewApi = {
  getByProduct: (productId: string) => api.get(`/Review/Product/${productId}`),
  create: (data: { productId: string; score: number; comment: string }) => api.post("/Review", data),
};

export const couponApi = {
  list: (params?: Record<string, unknown>) => api.post("/Coupon/Admin/List", params || {}),
  add: (data: unknown) => api.post("/Coupon/Admin/Add", data),
  validate: (code: string, orderTotal: number) => api.post("/Coupon/Validate", { code, orderTotal }),
  apply: (data: { orderId: string; code: string; discount: number }) => api.post("/Coupon/Admin/Apply", data),
  delete: (id: string) => api.delete(`/Coupon/Admin/Delete/${id}`),
};

export const blogApi = {
  published: (skip = 0, take = 10) => api.get("/Blog/Published", { params: { skip, take } }),
  getBySlug: (slug: string) => api.get(`/Blog/BySlug/${slug}`),
  list: (params?: Record<string, unknown>) => api.post("/Blog/Admin/List", params || {}),
  add: (data: unknown) => api.post("/Blog/Admin/Add", data),
  delete: (id: string) => api.delete(`/Blog/Admin/Delete/${id}`),
};

export const reportApi = {
  sales: (params: Record<string, unknown>) => api.post("/Report/Sales", params),
  category: (params: Record<string, unknown>) => api.post("/Report/Category", params),
  product: (params: Record<string, unknown>) => api.post("/Report/Product", params),
  exportSales: (params: Record<string, unknown>) => api.post("/Report/Export/Sales", params, { responseType: "blob" }),
  exportCategory: (params: Record<string, unknown>) => api.post("/Report/Export/Category", params, { responseType: "blob" }),
  exportProduct: (params: Record<string, unknown>) => api.post("/Report/Export/Product", params, { responseType: "blob" }),
};

export default api;
