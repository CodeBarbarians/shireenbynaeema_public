import { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import { blogApi } from "@/services/api";
import SEO from "@/components/seo/SEO";
import { resolveImageUrl } from "@/lib/imageUtils";
import { Skeleton } from "@/components/ui/skeleton";

interface BlogPostData {
  title: string;
  slug: string;
  excerpt: string;
  content: string;
  coverImageUrl: string;
  author: string;
  publishedOn: number;
  viewCount: number;
}

export default function BlogPost() {
  const { slug } = useParams();
  const [post, setPost] = useState<BlogPostData | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!slug) return;
    blogApi.getBySlug(slug).then((res) => setPost(res.data.data || null)).finally(() => setLoading(false));
  }, [slug]);

  if (loading) return (
    <div className="max-w-[800px] mx-auto px-4 py-16">
      <Skeleton className="h-8 w-3/4 mx-auto mb-4" />
      <Skeleton className="aspect-video w-full mb-8" />
      <div className="space-y-4">{Array.from({ length: 5 }).map((_, i) => <Skeleton key={i} className="h-4 w-full" />)}</div>
    </div>
  );

  if (!post) return (
    <div className="max-w-[800px] mx-auto px-4 py-16 text-center">
      <h1 className="text-2xl font-light mb-4" style={{ fontFamily: "Georgia, serif" }}>Post Not Found</h1>
      <Link to="/blog" className="text-sm underline text-muted-foreground hover:text-foreground">Back to blog</Link>
    </div>
  );

  return (
    <article className="max-w-[800px] mx-auto px-4 sm:px-6 py-12 lg:py-16">
      <SEO
        title={post.title}
        description={post.excerpt || post.title}
        image={resolveImageUrl(post.coverImageUrl)}
        url={`https://shireenbynaeema.com/blog/${post.slug}`}
        type="article"
        breadcrumbs={[
          { name: "Home", url: "/" },
          { name: "Blog", url: "/blog" },
          { name: post.title, url: `/blog/${post.slug}` },
        ]}
      />

      <Link to="/blog" className="text-xs text-muted-foreground hover:text-foreground mb-8 inline-block">&larr; Back to blog</Link>

      <header className="mb-8">
        <h1 className="text-3xl sm:text-4xl font-light mb-4" style={{ fontFamily: "Georgia, serif" }}>{post.title}</h1>
        <div className="flex items-center gap-2 text-sm text-muted-foreground">
          <span>{post.author}</span>
          <span>·</span>
          <span>{post.publishedOn ? new Date(post.publishedOn * 1000).toLocaleDateString("en-US", { month: "long", day: "numeric", year: "numeric" }) : ""}</span>
          <span>·</span>
          <span>{post.viewCount} views</span>
        </div>
      </header>

      {post.coverImageUrl && (
        <div className="aspect-video bg-muted overflow-hidden rounded-lg mb-8">
          <img src={resolveImageUrl(post.coverImageUrl)} alt={post.title} className="w-full h-full object-cover" />
        </div>
      )}

      <div className="prose prose-sm max-w-none text-muted-foreground leading-relaxed" dangerouslySetInnerHTML={{ __html: post.content }} />
    </article>
  );
}
