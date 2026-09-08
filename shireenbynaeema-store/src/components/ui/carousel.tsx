import { useEffect, useState, useCallback } from "react";
import { ChevronLeft, ChevronRight } from "lucide-react";
import { cn } from "@/lib/utils";
import { resolveImageUrl } from "@/lib/imageUtils";

interface CarouselSlide {
  imageUrl: string;
  title?: string;
  subtitle?: string;
  linkUrl?: string;
  buttonText?: string;
}

interface CarouselProps {
  slides: CarouselSlide[];
  autoPlay?: boolean;
  interval?: number;
  className?: string;
  showOverlay?: boolean;
  height?: string;
  showDots?: boolean;
  showArrows?: boolean;
}

export default function Carousel({
  slides,
  autoPlay = true,
  interval = 5000,
  className,
  showOverlay = false,
  height = "70vh",
  showDots = true,
  showArrows = true,
}: CarouselProps) {
  const [current, setCurrent] = useState(0);
  const [isPaused, setIsPaused] = useState(false);

  const next = useCallback(() => {
    setCurrent((prev) => (prev + 1) % slides.length);
  }, [slides.length]);

  const prev = useCallback(() => {
    setCurrent((prev) => (prev - 1 + slides.length) % slides.length);
  }, [slides.length]);

  useEffect(() => {
    if (!autoPlay || isPaused || slides.length <= 1) return;
    const timer = setInterval(next, interval);
    return () => clearInterval(timer);
  }, [autoPlay, isPaused, interval, next, slides.length]);

  if (!slides.length) return null;
  if (slides.length === 1) {
    const slide = slides[0];
    return (
      <div className={cn("relative overflow-hidden", className)} style={{ height }}>
        <img src={resolveImageUrl(slide.imageUrl)} alt={slide.title || ""} className="absolute inset-0 w-full h-full object-cover" />
        {showOverlay && <div className="absolute inset-0 bg-black/30" />}
        {slide.title && (
          <div className="absolute inset-0 flex items-center justify-center text-center text-white z-10 px-4">
            <div>
              {slide.subtitle && <p className="text-xs tracking-[3px] uppercase mb-4 opacity-80">{slide.subtitle}</p>}
              <h1 className="text-4xl sm:text-5xl lg:text-6xl font-light mb-4" style={{ fontFamily: "Georgia, serif" }}>{slide.title}</h1>
              {slide.buttonText && (
                <a href={slide.linkUrl || "#"} className="inline-block bg-white text-black px-8 py-3 text-xs font-medium tracking-[2px] uppercase hover:bg-gray-100 transition-colors">{slide.buttonText}</a>
              )}
            </div>
          </div>
        )}
      </div>
    );
  }

  return (
    <div
      className={cn("relative overflow-hidden group", className)}
      style={{ height }}
      onMouseEnter={() => setIsPaused(true)}
      onMouseLeave={() => setIsPaused(false)}
    >
      {slides.map((slide, i) => (
        <div key={i} className={cn("absolute inset-0 transition-opacity duration-1000 ease-in-out", i === current ? "opacity-100 z-10" : "opacity-0 z-0")}>
          <img src={resolveImageUrl(slide.imageUrl)} alt={slide.title || ""} className="w-full h-full object-cover" />
          {showOverlay && <div className="absolute inset-0 bg-black/30" />}
          {slide.title && (
            <div className="absolute inset-0 flex items-center justify-center text-center text-white z-10 px-4">
              <div className={cn("transition-all duration-700", i === current ? "opacity-100 translate-y-0" : "opacity-0 translate-y-4")}>
                {slide.subtitle && <p className="text-xs tracking-[3px] uppercase mb-4 opacity-80">{slide.subtitle}</p>}
                <h1 className="text-4xl sm:text-5xl lg:text-6xl font-light mb-4" style={{ fontFamily: "Georgia, serif" }}>{slide.title}</h1>
                {slide.buttonText && (
                  <a href={slide.linkUrl || "#"} className="inline-block bg-white text-black px-8 py-3 text-xs font-medium tracking-[2px] uppercase hover:bg-gray-100 transition-colors">{slide.buttonText}</a>
                )}
              </div>
            </div>
          )}
        </div>
      ))}

      {showArrows && (
        <>
          <button onClick={prev} className="absolute left-4 top-1/2 -translate-y-1/2 z-20 w-10 h-10 bg-white/80 hover:bg-white rounded-full flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity duration-300 shadow-lg">
            <ChevronLeft size={20} />
          </button>
          <button onClick={next} className="absolute right-4 top-1/2 -translate-y-1/2 z-20 w-10 h-10 bg-white/80 hover:bg-white rounded-full flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity duration-300 shadow-lg">
            <ChevronRight size={20} />
          </button>
        </>
      )}

      {showDots && slides.length > 1 && (
        <div className="absolute bottom-4 left-1/2 -translate-x-1/2 z-20 flex gap-2">
          {slides.map((_, i) => (
            <button key={i} onClick={() => setCurrent(i)} className={cn("w-2 h-2 rounded-full transition-all duration-300", i === current ? "bg-white w-6" : "bg-white/50 hover:bg-white/75")} />
          ))}
        </div>
      )}
    </div>
  );
}
