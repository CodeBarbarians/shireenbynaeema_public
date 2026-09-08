import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { productApi, categoryApi, settingApi } from "@/services/api";
import type { Product, Category } from "@/types";
import ProductCard from "@/components/product/ProductCard";
import SEO from "@/components/seo/SEO";
import { resolveImageUrl } from "@/lib/imageUtils";
import Carousel from "@/components/ui/carousel";

interface HeroSlide {
  imageUrl: string;
  title: string;
  subtitle: string;
  linkUrl: string;
  buttonText: string;
  isActive?: boolean;
}

const fallbackSlides: HeroSlide[] = [
  { imageUrl: "https://images.unsplash.com/photo-1469334031218-e382a71b716b?w=1600", title: "Welcome to Your Dream Closet", subtitle: "Western Clothing Brand", linkUrl: "/collections/all", buttonText: "Shop Now" },
];

export default function Home() {
  const [featuredProducts, setFeaturedProducts] = useState<Product[]>([]);
  const [newArrivals, setNewArrivals] = useState<Product[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [heroSlides, setHeroSlides] = useState<HeroSlide[]>([]);
  const [categoryImages, setCategoryImages] = useState<Record<string, string[]>>({});

  useEffect(() => {
    productApi.featured().then((res) => setFeaturedProducts(res.data.data?.entities || []));
    productApi.newArrivals().then((res) => setNewArrivals(res.data.data?.entities || []));
    categoryApi.list().then((res) => {
      const cats = (res.data.data?.entities || []).filter((c: Category) => c.isActive && c.showInNav);
      setCategories(cats);
      cats.forEach((cat: Category) => {
        categoryApi.getById(cat.id).then((r) => {
          const data = r.data.data;
          const imgs: string[] = [];
          if (data.images?.length) data.images.forEach((i: any) => imgs.push(i.imageUrl));
          else if (data.imageUrl) imgs.push(data.imageUrl);
          setCategoryImages((prev) => ({ ...prev, [cat.id]: imgs }));
        });
      });
    });
    settingApi.getHeroSlides().then((res) => {
      if (res.data.isSuccess && res.data.data?.value) {
        try {
          const raw = JSON.parse(res.data.data.value);
          setHeroSlides(raw.map((s: any) => ({
            imageUrl: s.imageUrl || s.ImageUrl || "",
            title: s.title || s.Title || "",
            subtitle: s.subtitle || s.Subtitle || "",
            linkUrl: s.linkUrl || s.LinkUrl || "/collections/all",
            buttonText: s.buttonText || s.ButtonText || "Shop Now",
            isActive: s.isActive ?? s.IsActive ?? true,
          })));
        } catch {}
      }
    });
  }, []);

  const slides = heroSlides.length > 0 ? heroSlides.filter((s) => s.isActive) : fallbackSlides;

  return (
    <div>
      <SEO
        title="Premium Women's Western Clothing in Pakistan"
        description="Shireen by Naeema - Explore elegant western wear for women. Dresses, tops, and more with free nationwide delivery across Pakistan. Shop the latest collections."
        url="https://shireenbynaeema.com"
        breadcrumbs={[{ name: "Home", url: "/" }]}
      />

      {/* Hero Carousel */}
      <section className="animate-fade-in">
        <Carousel slides={slides} showOverlay height="70vh" />
      </section>

      {/* New Arrivals */}
      <section className="max-w-[1400px] mx-auto px-4 sm:px-6 lg:px-8 py-16">
        <div className="text-center mb-10 animate-slide-up">
          <p className="text-xs tracking-[2px] uppercase text-muted-foreground mb-2">Summer 2026</p>
          <h2 className="text-2xl sm:text-3xl font-light" style={{ fontFamily: "Georgia, serif" }}>New Arrivals</h2>
        </div>
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-4 lg:gap-6">
          {newArrivals.slice(0, 8).map((product, i) => (
            <div key={product.id} className="animate-slide-up" style={{ animationDelay: `${i * 80}ms` }}>
              <ProductCard product={product} />
            </div>
          ))}
        </div>
        <div className="text-center mt-10">
          <Link to="/collections/new-arrivals" className="inline-block border border-foreground text-foreground px-8 py-3 text-xs font-medium tracking-[2px] uppercase hover:bg-foreground hover:text-background transition-all duration-300">View All</Link>
        </div>
      </section>

      {/* Shop by Category with carousel */}
      <section className="bg-muted py-16">
        <div className="max-w-[1400px] mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-10 animate-slide-up">
            <p className="text-xs tracking-[2px] uppercase text-muted-foreground mb-2">Shop by Category</p>
            <h2 className="text-2xl sm:text-3xl font-light" style={{ fontFamily: "Georgia, serif" }}>Your Dream Closet Awaits</h2>
          </div>
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-6">
            {categories.map((cat, i) => {
              const imgs = categoryImages[cat.id] || [];
              return (
                <Link key={cat.slug} to={`/collections/${cat.slug}`} className="group relative aspect-[4/5] overflow-hidden bg-muted rounded-lg animate-slide-up" style={{ animationDelay: `${i * 100}ms` }}>
                  {imgs.length > 1 ? (
                    <Carousel
                      slides={imgs.map((url) => ({ imageUrl: url }))}
                      autoPlay
                      interval={3000 + i * 500}
                      height="100%"
                      showOverlay={false}
                      showDots={false}
                      showArrows={false}
                      className="rounded-lg"
                    />
                  ) : imgs.length === 1 ? (
                    <img src={resolveImageUrl(imgs[0])} alt={cat.name} className="absolute inset-0 w-full h-full object-cover transition-transform duration-700 group-hover:scale-105" loading="lazy" />
                  ) : cat.imageUrl ? (
                    <img src={resolveImageUrl(cat.imageUrl)} alt={cat.name} className="absolute inset-0 w-full h-full object-cover transition-transform duration-700 group-hover:scale-105" loading="lazy" />
                  ) : null}
                  <div className="absolute inset-0 bg-black/20 group-hover:bg-black/30 transition-colors duration-500 z-10" />
                  <div className="absolute bottom-6 left-6 z-10">
                    <span className="bg-white text-black text-xs font-medium tracking-[2px] uppercase px-5 py-2.5 inline-block transition-transform duration-300 group-hover:translate-y-[-2px]">Shop {cat.name}</span>
                  </div>
                </Link>
              );
            })}
          </div>
        </div>
      </section>

      {/* Featured */}
      <section className="max-w-[1400px] mx-auto px-4 sm:px-6 lg:px-8 py-16">
        <div className="text-center mb-10 animate-slide-up">
          <p className="text-xs tracking-[2px] uppercase text-muted-foreground mb-2">Limited Edition</p>
          <h2 className="text-2xl sm:text-3xl font-light" style={{ fontFamily: "Georgia, serif" }}>Featured</h2>
        </div>
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-4 lg:gap-6">
          {featuredProducts.slice(0, 8).map((product, i) => (
            <div key={product.id} className="animate-slide-up" style={{ animationDelay: `${i * 80}ms` }}>
              <ProductCard product={product} />
            </div>
          ))}
        </div>
      </section>

      {/* Trust bar */}
      <section className="bg-muted py-16">
        <div className="max-w-[1400px] mx-auto px-4 sm:px-6 lg:px-8 text-center">
          <div className="grid grid-cols-2 gap-8 max-w-md mx-auto animate-slide-up">
            <div>
              <p className="text-2xl font-light mb-1">14 Days</p>
              <p className="text-xs text-muted-foreground tracking-wider uppercase">Return Policy</p>
            </div>
            <div>
              <p className="text-2xl font-light mb-1">24/7</p>
              <p className="text-xs text-muted-foreground tracking-wider uppercase">Online Support</p>
            </div>
          </div>
        </div>
      </section>
    </div>
  );
}
