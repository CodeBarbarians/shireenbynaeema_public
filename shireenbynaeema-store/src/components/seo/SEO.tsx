import { Helmet } from "react-helmet-async";

const SITE_NAME = "Shireen by Naeema";
const SITE_URL = "https://shireenbynaeema.com";
const DEFAULT_DESCRIPTION = "Shireen by Naeema - Premium women's western clothing brand in Pakistan. Elegant silhouettes designed to make every entrance unforgettable. Free nationwide delivery.";
const DEFAULT_IMAGE = `${SITE_URL}/favicon.jpeg`;

export interface SEOProps {
  title?: string;
  description?: string;
  image?: string;
  url?: string;
  type?: "website" | "product" | "article";
  product?: {
    name: string;
    description: string;
    image: string;
    price: string;
    currency: string;
    availability: string;
    brand: string;
    rating?: number;
    reviewCount?: number;
  };
  breadcrumbs?: { name: string; url: string }[];
}

export default function SEO({
  title,
  description = DEFAULT_DESCRIPTION,
  image = DEFAULT_IMAGE,
  url = SITE_URL,
  type = "website",
  product,
  breadcrumbs,
}: SEOProps) {
  const fullTitle = title ? `${title} | ${SITE_NAME}` : SITE_NAME;

  const organizationSchema = {
    "@context": "https://schema.org",
    "@type": "Organization",
    name: SITE_NAME,
    url: SITE_URL,
    logo: `${SITE_URL}/favicon.jpeg`,
    description: DEFAULT_DESCRIPTION,
    sameAs: [
      "https://instagram.com/shireenbynaeema",
      "https://facebook.com/shireenbynaeema",
      "https://tiktok.com/@shireenbynaeema",
    ],
    contactPoint: {
      "@type": "ContactPoint",
      telephone: "+92-300-1234567",
      contactType: "customer service",
      availableLanguage: ["English", "Urdu"],
    },
    address: {
      "@type": "PostalAddress",
      addressCountry: "PK",
      addressLocality: "Karachi",
    },
  };

  const websiteSchema = {
    "@context": "https://schema.org",
    "@type": "WebSite",
    name: SITE_NAME,
    url: SITE_URL,
    potentialAction: {
      "@type": "SearchAction",
      target: `${SITE_URL}/collections/all?q={search_term_string}`,
      "query-input": "required name=search_term_string",
    },
  };

  const productSchema = product
    ? {
        "@context": "https://schema.org",
        "@type": "Product",
        name: product.name,
        description: product.description,
        image: product.image,
        brand: {
          "@type": "Brand",
          name: product.brand || SITE_NAME,
        },
        offers: {
          "@type": "Offer",
          url,
          priceCurrency: product.currency,
          price: product.price,
          availability: `https://schema.org/${product.availability}`,
          seller: {
            "@type": "Organization",
            name: SITE_NAME,
          },
        },
        ...(product.rating && {
          aggregateRating: {
            "@type": "AggregateRating",
            ratingValue: product.rating,
            reviewCount: product.reviewCount || 0,
          },
        }),
      }
    : null;

  const breadcrumbSchema = breadcrumbs
    ? {
        "@context": "https://schema.org",
        "@type": "BreadcrumbList",
        itemListElement: breadcrumbs.map((crumb, i) => ({
          "@type": "ListItem",
          position: i + 1,
          name: crumb.name,
          item: `${SITE_URL}${crumb.url}`,
        })),
      }
    : null;

  const localBusinessSchema = {
    "@context": "https://schema.org",
    "@type": "ClothingStore",
    name: SITE_NAME,
    url: SITE_URL,
    image: DEFAULT_IMAGE,
    description: DEFAULT_DESCRIPTION,
    address: {
      "@type": "PostalAddress",
      addressCountry: "PK",
      addressLocality: "Karachi",
    },
    geo: {
      "@type": "GeoCoordinates",
      latitude: 24.8607,
      longitude: 67.0011,
    },
    priceRange: "PKR",
    openingHoursSpecification: {
      "@type": "OpeningHoursSpecification",
      dayOfWeek: ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"],
      opens: "10:00",
      closes: "19:00",
    },
  };

  return (
    <Helmet>
      <title>{fullTitle}</title>
      <meta name="description" content={description} />
      <meta name="robots" content="index, follow" />
      <link rel="canonical" href={url} />

      <meta property="og:type" content={type} />
      <meta property="og:title" content={fullTitle} />
      <meta property="og:description" content={description} />
      <meta property="og:image" content={image} />
      <meta property="og:url" content={url} />
      <meta property="og:site_name" content={SITE_NAME} />
      <meta property="og:locale" content="en_PK" />

      <meta name="twitter:card" content="summary_large_image" />
      <meta name="twitter:title" content={fullTitle} />
      <meta name="twitter:description" content={description} />
      <meta name="twitter:image" content={image} />

      <script type="application/ld+json">
        {JSON.stringify(organizationSchema)}
      </script>
      <script type="application/ld+json">
        {JSON.stringify(websiteSchema)}
      </script>
      <script type="application/ld+json">
        {JSON.stringify(localBusinessSchema)}
      </script>
      {productSchema && (
        <script type="application/ld+json">
          {JSON.stringify(productSchema)}
        </script>
      )}
      {breadcrumbSchema && (
        <script type="application/ld+json">
          {JSON.stringify(breadcrumbSchema)}
        </script>
      )}
    </Helmet>
  );
}
