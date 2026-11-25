import { TestBed } from '@angular/core/testing';
import { ThemeService } from './theme.service';

describe('ThemeService', () => {
  beforeEach(() => {
    localStorage.clear();
    document.documentElement.removeAttribute('data-theme');
    TestBed.configureTestingModule({});
  });

  it('should toggle between light and dark and persist to localStorage', () => {
    const service = TestBed.inject(ThemeService);
    service.setTheme('light');
    expect(service.isDark()).toBe(false);
    expect(localStorage.getItem('voyage_theme')).toBe('light');

    service.toggleTheme();
    expect(service.isDark()).toBe(true);
    expect(document.documentElement.getAttribute('data-theme')).toBe('dark');
    expect(localStorage.getItem('voyage_theme')).toBe('dark');
  });

  it('should restore saved theme from localStorage on initialization', () => {
    localStorage.setItem('voyage_theme', 'dark');
    const service = TestBed.inject(ThemeService);
    expect(service.theme()).toBe('dark');
    expect(document.documentElement.getAttribute('data-theme')).toBe('dark');
  });
});
