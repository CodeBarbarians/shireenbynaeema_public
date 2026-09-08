const API_BASE = import.meta.env.VITE_API_URL || "https://shireenbynaeema-production.up.railway.app/api";
const API_ORIGIN = API_BASE.replace(/\/api\/?$/, "");

export function resolveImageUrl(url: string | null | undefined): string {
  if (!url) return "";
  if (url.startsWith("http://") || url.startsWith("https://")) return url;
  return `${API_ORIGIN}${url}`;
}
