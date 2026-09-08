import { create } from "zustand";
import { authApi } from "@/services/api";

interface User {
  userId: string;
  email: string;
  displayName: string;
  roles: string[];
}

interface AuthStore {
  user: User | null;
  token: string | null;
  loading: boolean;
  isAuthenticated: boolean;
  isAdmin: boolean;
  otpRequired: boolean;
  otpEmail: string;
  login: (email: string, password: string) => Promise<boolean>;
  verifyOtp: (email: string, code: string, rememberMe?: boolean) => Promise<boolean>;
  register: (data: { firstName: string; lastName: string; email: string; password: string }) => Promise<boolean>;
  logout: () => void;
  loadUser: () => void;
  clearOtp: () => void;
}

const getInitialUser = (): User | null => {
  try {
    const str = localStorage.getItem("user");
    return str ? JSON.parse(str) : null;
  } catch {
    return null;
  }
};

const initialUser = getInitialUser();

const parseToken = (token: string): User => {
  const payload = JSON.parse(atob(token.split(".")[1]));
  const getClaim = (short: string, full: string) => payload[short] ?? payload[full] ?? "";
  return {
    userId: getClaim("nameidentifier", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"),
    email: getClaim("emailaddress", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress") || getClaim("email", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"),
    displayName: getClaim("displayName", "displayName") || "",
    roles: (() => {
      const r = payload.role ?? payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
      return r ? (Array.isArray(r) ? r : [r]) : [];
    })(),
  };
};

export const useAuthStore = create<AuthStore>((set, _get) => ({
  user: initialUser,
  token: localStorage.getItem("token"),
  loading: false,
  isAuthenticated: !!localStorage.getItem("token"),
  isAdmin: initialUser ? initialUser.roles.includes("Admin") || initialUser.roles.includes("SuperAdmin") : false,
  otpRequired: false,
  otpEmail: "",

  login: async (email, password) => {
    set({ loading: true });
    try {
      const res = await authApi.login({ email, password });
      const data = res.data.data || res.data;

      if (data.token) {
        const user = parseToken(data.token);
        localStorage.setItem("token", data.token);
        localStorage.setItem("user", JSON.stringify(user));
        set({ user, token: data.token, isAuthenticated: true, isAdmin: user.roles.includes("Admin") || user.roles.includes("SuperAdmin"), loading: false, otpRequired: false, otpEmail: "" });
        return true;
      }

      if (data.isOTPSent) {
        set({ loading: false, otpRequired: true, otpEmail: email });
        return false;
      }

      set({ loading: false });
      return false;
    } catch {
      set({ loading: false });
      return false;
    }
  },

  verifyOtp: async (email, code, rememberMe = false) => {
    set({ loading: true });
    try {
      const res = await authApi.verifyOtp({ email, code, rememberMe });
      const data = res.data.data || res.data;

      if (data.token) {
        const user = parseToken(data.token);
        localStorage.setItem("token", data.token);
        localStorage.setItem("user", JSON.stringify(user));
        set({ user, token: data.token, isAuthenticated: true, isAdmin: user.roles.includes("Admin") || user.roles.includes("SuperAdmin"), loading: false, otpRequired: false, otpEmail: "" });
        return true;
      }

      set({ loading: false });
      return false;
    } catch {
      set({ loading: false });
      return false;
    }
  },

  register: async (data) => {
    set({ loading: true });
    try {
      await authApi.register(data);
      set({ loading: false });
      return true;
    } catch {
      set({ loading: false });
      return false;
    }
  },

  logout: () => {
    localStorage.removeItem("token");
    localStorage.removeItem("user");
    set({ user: null, token: null, isAuthenticated: false, isAdmin: false, otpRequired: false, otpEmail: "" });
  },

  loadUser: () => {
    const token = localStorage.getItem("token");
    const userStr = localStorage.getItem("user");
    if (token && userStr) {
      const user = JSON.parse(userStr) as User;
      set({ user, token, isAuthenticated: true, isAdmin: user.roles.includes("Admin") || user.roles.includes("SuperAdmin") });
    }
  },

  clearOtp: () => set({ otpRequired: false, otpEmail: "" }),
}));
