import { Link, useLocation } from "react-router-dom";
import { useEffect, useState } from "react";
import api from "@/services/api";
import SEO from "@/components/seo/SEO";

interface CmsPageData {
  title: string;
  content: string;
}

export default function CmsPage() {
  const location = useLocation();
  const slug = location.pathname.split("/").pop() || "";
  const [page, setPage] = useState<CmsPageData | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!slug) return;
    setLoading(true);
    api.get(`/CmsPage/BySlug/${slug}`)
      .then((res) => {
        if (res.data.isSuccess) setPage(res.data.data);
        else setPage(null);
      })
      .catch(() => setPage(null))
      .finally(() => setLoading(false));
  }, [slug]);

  if (loading) {
    return (
      <div className="max-w-[800px] mx-auto px-4 sm:px-6 py-12 lg:py-16">
        <div className="animate-pulse space-y-4">
          <div className="h-8 bg-muted rounded w-1/2 mx-auto" />
          <div className="h-4 bg-muted rounded w-full" />
          <div className="h-4 bg-muted rounded w-3/4" />
          <div className="h-4 bg-muted rounded w-5/6" />
        </div>
      </div>
    );
  }

  if (!page) {
    return (
      <div className="max-w-[800px] mx-auto px-4 py-16 text-center">
        <h1 className="text-2xl font-light mb-4" style={{ fontFamily: "Georgia, serif" }}>Page Not Found</h1>
        <Link to="/" className="text-sm underline text-muted-foreground hover:text-foreground">Go back home</Link>
      </div>
    );
  }

  const pageTitle = page?.title || "Page";
  const pageDescriptions: Record<string, string> = {
    contact: "Get in touch with Shireen by Naeema. Contact us via email, phone, or WhatsApp for orders, sizing, and inquiries.",
    shipping: "Shireen by Naeema shipping and returns policy. Free nationwide delivery, 14-day exchange policy across Pakistan.",
    "size-guide": "Shireen by Naeema size guide. Find your perfect fit with our detailed measurements for XS, S, M, L, XL, and XXL.",
    faq: "Frequently asked questions about Shireen by Naeema. Delivery, payments, exchanges, and more answered here.",
    about: "Learn about Shireen by Naeema - a premium women's western clothing brand celebrating elegance and timeless style.",
    privacy: "Shireen by Naeema privacy policy. How we collect, use, and protect your personal information.",
    terms: "Shireen by Naeema terms of service. Terms and conditions for using our website and purchasing our products.",
  };

  return (
    <div className="max-w-[800px] mx-auto px-4 sm:px-6 py-12 lg:py-16">
      <SEO
        title={pageTitle}
        description={pageDescriptions[slug] || `${pageTitle} - Shireen by Naeema`}
        url={`https://shireenbynaeema.com/${slug}`}
        breadcrumbs={[
          { name: "Home", url: "/" },
          { name: pageTitle, url: `/${slug}` },
        ]}
      />
      <h1 className="text-2xl sm:text-3xl font-light mb-8 text-center" style={{ fontFamily: "Georgia, serif" }}>{page.title}</h1>
      <div className="text-sm text-muted-foreground leading-relaxed whitespace-pre-line">{page.content}</div>
      <div className="mt-12 text-center">
        <Link to="/" className="text-xs underline text-muted-foreground hover:text-foreground">Return to Shop</Link>
      </div>
    </div>
  );
}
