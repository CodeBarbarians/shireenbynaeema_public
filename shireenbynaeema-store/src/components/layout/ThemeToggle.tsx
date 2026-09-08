import { Moon, Sun, Palette } from "lucide-react";
import { useThemeStore } from "@/store/themeStore";
import { Button } from "@/components/ui/button";

const builtInIcons: Record<string, typeof Sun> = { light: Moon, dark: Sun, peach: Palette };

export default function ThemeToggle() {
  const { theme, cycleTheme } = useThemeStore();
  const Icon = builtInIcons[theme] || Palette;

  return (
    <Button variant="ghost" size="icon" onClick={cycleTheme} className="h-8 w-8" title={`Theme: ${theme}`}>
      <Icon size={16} strokeWidth={1.5} />
    </Button>
  );
}
