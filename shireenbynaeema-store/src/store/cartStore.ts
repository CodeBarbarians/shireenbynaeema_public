import { create } from "zustand";
import { cartApi } from "@/services/api";
import type { CartItem, Product, ProductVariant } from "@/types";

interface GuestCartItem {
  id: string;
  productVariantId: string;
  variant?: ProductVariant;
  product?: Product;
  quantity: number;
  unitPrice: number;
}

interface CartStore {
  items: CartItem[];
  guestItems: GuestCartItem[];
  isOpen: boolean;
  loading: boolean;
  total: number;
  itemCount: number;
  isGuest: boolean;
  toggleCart: () => void;
  openCart: () => void;
  closeCart: () => void;
  fetchCart: () => Promise<void>;
  addItem: (variant: ProductVariant, product: Product, quantity?: number) => Promise<void>;
  updateQuantity: (cartItemId: string, quantity: number) => Promise<void>;
  removeItem: (cartItemId: string) => Promise<void>;
  getGuestItems: () => GuestCartItem[];
  addGuestItem: (variant: ProductVariant, product: Product, quantity?: number) => void;
  updateGuestQuantity: (variantId: string, quantity: number) => void;
  removeGuestItem: (variantId: string) => void;
  clearGuestCart: () => void;
}

const getGuestCart = (): GuestCartItem[] => {
  try {
    const str = localStorage.getItem("guestCart");
    return str ? JSON.parse(str) : [];
  } catch { return []; }
};

const saveGuestCart = (items: GuestCartItem[]) => {
  localStorage.setItem("guestCart", JSON.stringify(items));
};

export const useCartStore = create<CartStore>((set, get) => ({
  items: [],
  guestItems: getGuestCart(),
  isOpen: false,
  loading: false,
  total: 0,
  itemCount: 0,
  isGuest: !localStorage.getItem("token"),

  toggleCart: () => set((s) => ({ isOpen: !s.isOpen })),
  openCart: () => set({ isOpen: true }),
  closeCart: () => set({ isOpen: false }),

  fetchCart: async () => {
    const token = localStorage.getItem("token");
    if (!token) {
      const guestItems = getGuestCart();
      const total = guestItems.reduce((sum, item) => sum + item.unitPrice * item.quantity, 0);
      const itemCount = guestItems.reduce((sum, item) => sum + item.quantity, 0);
      set({ guestItems, total, itemCount, isGuest: true });
      return;
    }
    set({ loading: true });
    try {
      const res = await cartApi.get();
      const items = res.data.data || [];
      const total = items.reduce((sum: number, item: CartItem) => sum + item.unitPrice * item.quantity, 0);
      const itemCount = items.reduce((sum: number, item: CartItem) => sum + item.quantity, 0);
      set({ items, total, itemCount, loading: false, isGuest: false });
    } catch {
      set({ loading: false });
    }
  },

  addItem: async (variant, product, quantity = 1) => {
    const token = localStorage.getItem("token");
    if (!token) {
      get().addGuestItem(variant, product, quantity);
      return;
    }
    set({ loading: true });
    try {
      await cartApi.addItem(variant.id, quantity);
      await get().fetchCart();
      set({ isOpen: true, loading: false });
    } catch {
      set({ loading: false });
    }
  },

  updateQuantity: async (cartItemId, quantity) => {
    if (quantity < 1) return get().removeItem(cartItemId);
    set({ loading: true });
    try {
      await cartApi.updateQuantity(cartItemId, quantity);
      await get().fetchCart();
    } catch {
      set({ loading: false });
    }
  },

  removeItem: async (cartItemId) => {
    set({ loading: true });
    try {
      await cartApi.removeItem(cartItemId);
      await get().fetchCart();
    } catch {
      set({ loading: false });
    }
  },

  getGuestItems: () => get().guestItems,

  addGuestItem: (variant, product, quantity = 1) => {
    const items = get().guestItems;
    const existing = items.find((i) => i.productVariantId === variant.id);
    let updated: GuestCartItem[];
    if (existing) {
      updated = items.map((i) => i.productVariantId === variant.id ? { ...i, quantity: i.quantity + quantity } : i);
    } else {
      updated = [...items, { id: crypto.randomUUID(), productVariantId: variant.id, variant, product, quantity, unitPrice: variant.price || product.salePrice || product.basePrice }];
    }
    saveGuestCart(updated);
    const total = updated.reduce((sum, item) => sum + item.unitPrice * item.quantity, 0);
    const itemCount = updated.reduce((sum, item) => sum + item.quantity, 0);
    set({ guestItems: updated, total, itemCount, isOpen: true });
  },

  updateGuestQuantity: (variantId, quantity) => {
    if (quantity < 1) return get().removeGuestItem(variantId);
    const updated = get().guestItems.map((i) => i.productVariantId === variantId ? { ...i, quantity } : i);
    saveGuestCart(updated);
    const total = updated.reduce((sum, item) => sum + item.unitPrice * item.quantity, 0);
    const itemCount = updated.reduce((sum, item) => sum + item.quantity, 0);
    set({ guestItems: updated, total, itemCount });
  },

  removeGuestItem: (variantId) => {
    const updated = get().guestItems.filter((i) => i.productVariantId !== variantId);
    saveGuestCart(updated);
    const total = updated.reduce((sum, item) => sum + item.unitPrice * item.quantity, 0);
    const itemCount = updated.reduce((sum, item) => sum + item.quantity, 0);
    set({ guestItems: updated, total, itemCount });
  },

  clearGuestCart: () => {
    localStorage.removeItem("guestCart");
    set({ guestItems: [], total: 0, itemCount: 0 });
  },
}));
