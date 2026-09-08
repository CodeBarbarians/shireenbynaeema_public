import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { categoryApi } from "@/services/api";
import type { Category } from "@/types";
import logo from "@/assets/logo.svg";

const footerLinks = {
  help: [
    { label: "Contact Us", to: "/contact" },
    { label: "Shipping & Returns", to: "/shipping" },
    { label: "Size Guide", to: "/size-guide" },
    { label: "FAQ", to: "/faq" },
  ],
  company: [
    { label: "About Us", to: "/about" },
    { label: "Blog", to: "/blog" },
    { label: "Privacy Policy", to: "/privacy" },
    { label: "Terms of Service", to: "/terms" },
  ],
};

const socialLinks = [
  { label: "Instagram", href: "https://instagram.com/shireenbynaeema", ariaLabel: "Follow us on Instagram" },
  { label: "Facebook", href: "https://facebook.com/shireenbynaeema", ariaLabel: "Follow us on Facebook" },
  { label: "TikTok", href: "https://tiktok.com/@shireenbynaeema", ariaLabel: "Follow us on TikTok" },
];

export default function Footer() {
  const [categories, setCategories] = useState<Category[]>([]);

  useEffect(() => {
    categoryApi.list().then((res) => setCategories(res.data.data?.entities || []));
  }, []);

  const shopLinks = [
    ...categories.map((c) => ({ label: c.name, to: `/collections/${c.slug}` })),
    { label: "All Products", to: "/collections/all" },
    { label: "New Arrivals", to: "/collections/new-arrivals" },
  ];

  const sections: { title: string; links: { label: string; to: string }[] }[] = [
    { title: "Shop", links: shopLinks },
    { title: "Help", links: footerLinks.help },
    { title: "Company", links: footerLinks.company },
  ];

  return (
    <footer className="bg-background border-t border-border" role="contentinfo">
      <div className="max-w-[1400px] mx-auto px-4 sm:px-6 lg:px-8 py-12 lg:py-16">
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-8 lg:gap-12">
          <div className="col-span-2 lg:col-span-1">
            <Link to="/" aria-label="Shireen by Naeema - Home">
              <img src={logo} alt="Shireen by Naeema - Premium Women's Western Clothing" className="h-10 w-auto object-contain mb-4 dark:invert" />
            </Link>
            <p className="text-sm text-muted-foreground leading-relaxed max-w-xs">
              Your dream closet awaits. Elegant silhouettes designed to make every entrance unforgettable.
            </p>
            <nav aria-label="Social media links" className="flex space-x-4 mt-6">
              {socialLinks.map((link) => (
                <a key={link.label} href={link.href} target="_blank" rel="noopener noreferrer" aria-label={link.ariaLabel} className="text-muted-foreground hover:text-foreground text-sm transition-colors">
                  {link.label}
                </a>
              ))}
            </nav>
          </div>

          {sections.map((section) => (
            <div key={section.title}>
              <h3 className="text-xs font-semibold tracking-[2px] uppercase text-foreground mb-4 capitalize">{section.title}</h3>
              <ul className="space-y-3">
                {section.links.map((link) => (
                  <li key={link.to}>
                    <Link to={link.to} className="text-sm text-muted-foreground hover:text-foreground transition-colors">{link.label}</Link>
                  </li>
                ))}
              </ul>
            </div>
          ))}
        </div>
      </div>

      <div className="border-t border-border">
        <div className="max-w-[1400px] mx-auto px-4 sm:px-6 lg:px-8 py-6 flex flex-col sm:flex-row items-center justify-between">
          <p className="text-xs text-muted-foreground">&copy; {new Date().getFullYear()} Shireen by Naeema. All rights reserved.</p>
          <div className="flex items-center space-x-3 mt-4 sm:mt-0" aria-label="Accepted payment methods">
            <span className="text-xs text-muted-foreground">Visa</span>
            <span className="text-xs text-muted-foreground">Mastercard</span>
            <span className="text-xs text-muted-foreground">Stripe</span>
          </div>
        </div>
      </div>
    </footer>
  );
}
