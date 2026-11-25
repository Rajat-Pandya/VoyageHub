import { Injectable, PLATFORM_ID, computed, inject, signal } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

export type ThemeMode = 'light' | 'dark';

const THEME_STORAGE_KEY = 'voyage_theme';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private readonly platformId = inject(PLATFORM_ID);

  readonly theme = signal<ThemeMode>('light');
  readonly isDark = computed(() => this.theme() === 'dark');

  constructor() {
    if (isPlatformBrowser(this.platformId)) {
      const saved = this.readSavedTheme();
      const initial: ThemeMode = saved ?? (window.matchMedia?.('(prefers-color-scheme: dark)').matches ? 'dark' : 'light');
      this.setTheme(initial, saved !== null);
    }
  }

  toggleTheme(): void {
    const next: ThemeMode = this.theme() === 'dark' ? 'light' : 'dark';
    this.setTheme(next, true);
  }

  setTheme(mode: ThemeMode, persist = true): void {
    this.theme.set(mode);
    if (!isPlatformBrowser(this.platformId)) return;

    document.documentElement.setAttribute('data-theme', mode);
    document.documentElement.style.colorScheme = mode;
    if (persist) {
      try {
        localStorage.setItem(THEME_STORAGE_KEY, mode);
      } catch {
        // Ignore storage quota errors
      }
    }
  }

  private readSavedTheme(): ThemeMode | null {
    try {
      const val = localStorage.getItem(THEME_STORAGE_KEY);
      return val === 'dark' || val === 'light' ? val : null;
    } catch {
      return null;
    }
  }
}
