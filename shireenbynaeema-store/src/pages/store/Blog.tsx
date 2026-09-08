import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { blogApi } from "@/services/api";
import type { BlogPost } from "@/types";
import SEO from "@/components/seo/SEO";
import { resolveImageUrl } from "@/lib/imageUtils";
import { Skeleton } from "@/components/ui/skeleton";

export default function Blog() {
  const [posts, setPosts] = useState<BlogPost[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => { blogApi.published().then((res) => setPosts(res.data.data?.entities || [])).finally(() => setLoading(false)); }, []);

  return (
    <div className="max-w-[1400px] mx-auto px-4 sm:px-6 lg:px-8 py-8 lg:py-12">
      <SEO
        title="Blog"
        description="Style tips, trends, and stories from Shireen by Naeema. Explore the latest in western fashion for women in Pakistan."
        url="https://shireenbynaeema.com/blog"
        breadcrumbs={[{ name: "Home", url: "/" }, { name: "Blog", url: "/blog" }]}
      />

      <div className="text-center mb-12">
        <h1 className="text-3xl sm:text-4xl font-light" style={{ fontFamily: "Georgia, serif" }}>Our Blog</h1>
        <p className="text-sm text-muted-foreground mt-2">Style tips, trends, and stories</p>
      </div>

      {loading ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
          {Array.from({ length: 6 }).map((_, i) => (
            <div key={i} className="space-y-3"><Skeleton className="aspect-video w-full" /><Skeleton className="h-4 w-3/4" /><Skeleton className="h-3 w-1/2" /></div>
          ))}
        </div>
      ) : posts.length === 0 ? (
        <p className="text-center text-muted-foreground py-16">No blog posts yet. Check back soon!</p>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
          {posts.map((post) => (
            <Link key={post.id} to={`/blog/${post.slug}`} className="group block">
              <div className="aspect-video bg-muted overflow-hidden rounded-lg mb-3">
                {post.coverImageUrl && <img src={resolveImageUrl(post.coverImageUrl)} alt={post.title} className="w-full h-full object-cover transition-transform duration-500 group-hover:scale-105" loading="lazy" />}
              </div>
              <h2 className="text-lg font-medium group-hover:text-muted-foreground transition-colors">{post.title}</h2>
              <p className="text-sm text-muted-foreground mt-1 line-clamp-2">{post.excerpt}</p>
              <div className="flex items-center gap-2 mt-2 text-xs text-muted-foreground">
                <span>{post.author}</span>
                <span>·</span>
                <span>{post.publishedOn ? new Date(post.publishedOn * 1000).toLocaleDateString("en-US", { month: "long", day: "numeric", year: "numeric" }) : ""}</span>
              </div>
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}
