import { create } from "zustand";

export interface ThemeColors {
  background: string;
  foreground: string;
  card: string;
  cardForeground: string;
  primary: string;
  primaryForeground: string;
  secondary: string;
  secondaryForeground: string;
  muted: string;
  mutedForeground: string;
  accent: string;
  accentForeground: string;
  destructive: string;
  border: string;
  input: string;
  ring: string;
}

export interface CustomTheme {
  id: string;
  name: string;
  colors: ThemeColors;
}

const BUILT_IN: Record<string, string> = { light: "light", dark: "dark", peach: "peach" };

function applyThemeClass(theme: string) {
  document.documentElement.classList.remove("light", "dark", "peach");
  document.documentElement.removeAttribute("style");

  if (BUILT_IN[theme]) {
    document.documentElement.classList.add(theme);
  }
}

function applyCustomThemeColors(colors: ThemeColors) {
  const root = document.documentElement;
  const vars: Record<string, string> = {
    "--background": colors.background,
    "--foreground": colors.foreground,
    "--card": colors.card,
    "--card-foreground": colors.cardForeground,
    "--primary": colors.primary,
    "--primary-foreground": colors.primaryForeground,
    "--secondary": colors.secondary,
    "--secondary-foreground": colors.secondaryForeground,
    "--muted": colors.muted,
    "--muted-foreground": colors.mutedForeground,
    "--accent": colors.accent,
    "--accent-foreground": colors.accentForeground,
    "--destructive": colors.destructive,
    "--border": colors.border,
    "--input": colors.input,
    "--ring": colors.ring,
  };
  Object.entries(vars).forEach(([k, v]) => root.style.setProperty(k, v));
}

interface ThemeStore {
  theme: string;
  customThemes: CustomTheme[];
  setTheme: (theme: string) => void;
  cycleTheme: () => void;
  loadCustomThemes: (themes: CustomTheme[]) => void;
  getThemeList: () => { id: string; label: string }[];
}

export const useThemeStore = create<ThemeStore>((set, get) => ({
  theme: localStorage.getItem("theme") || "light",
  customThemes: JSON.parse(localStorage.getItem("customThemes") || "[]"),

  setTheme: (theme) => {
    localStorage.setItem("theme", theme);
    applyThemeClass(theme);
    if (!BUILT_IN[theme]) {
      const ct = get().customThemes.find((t) => t.id === theme);
      if (ct) applyCustomThemeColors(ct.colors);
    }
    set({ theme });
  },

  cycleTheme: () =>
    set((s) => {
      const all = ["light", "dark", "peach", ...s.customThemes.map((t) => t.id)];
      const idx = all.indexOf(s.theme);
      const next = all[(idx + 1) % all.length];
      localStorage.setItem("theme", next);
      applyThemeClass(next);
      if (!BUILT_IN[next]) {
        const ct = s.customThemes.find((t) => t.id === next);
        if (ct) applyCustomThemeColors(ct.colors);
      }
      return { theme: next };
    }),

  loadCustomThemes: (themes) => {
    localStorage.setItem("customThemes", JSON.stringify(themes));
    set({ customThemes: themes });
  },

  getThemeList: () => [
    { id: "light", label: "Light" },
    { id: "dark", label: "Dark" },
    { id: "peach", label: "Peach" },
    ...get().customThemes.map((t) => ({ id: t.id, label: t.name })),
  ],
}));

export function initTheme() {
  const stored = localStorage.getItem("theme") || "light";
  applyThemeClass(stored);
  if (!BUILT_IN[stored]) {
    const customThemes = JSON.parse(localStorage.getItem("customThemes") || "[]") as CustomTheme[];
    const ct = customThemes.find((t) => t.id === stored);
    if (ct) applyCustomThemeColors(ct.colors);
  }
}
