import { computed, signal, Signal } from '@angular/core';

/** Reactive client-side pagination state derived from an items signal. */
export interface PaginationHelper<T> {
  readonly currentPage: Signal<number>;
  readonly totalItems: Signal<number>;
  readonly totalPages: Signal<number>;
  readonly pagedItems: Signal<T[]>;
  readonly pageNumbers: Signal<number[]>;
  readonly startIndex: Signal<number>;
  readonly endIndex: Signal<number>;
  goToPage: (page: number) => void;
  prevPage: () => void;
  nextPage: () => void;
  reset: () => void;
}

/** Creates a reactive pagination helper over a signal array. */
export function createPagination<T>(items: Signal<T[]>, pageSize = 10): PaginationHelper<T> {
  const currentPage = signal(1);
  const totalItems = computed(() => items().length);
  const totalPages = computed(() => Math.max(1, Math.ceil(totalItems() / pageSize)));
  const safePage = computed(() => Math.min(currentPage(), totalPages()));

  const pagedItems = computed(() => {
    const start = (safePage() - 1) * pageSize;
    return items().slice(start, start + pageSize);
  });

  const pageNumbers = computed(() =>
    Array.from({ length: totalPages() }, (_, i) => i + 1)
  );

  const startIndex = computed(() =>
    totalItems() === 0 ? 0 : (safePage() - 1) * pageSize + 1
  );

  const endIndex = computed(() =>
    Math.min(safePage() * pageSize, totalItems())
  );

  return {
    currentPage: safePage,
    totalItems,
    totalPages,
    pagedItems,
    pageNumbers,
    startIndex,
    endIndex,
    goToPage: (page: number) => {
      if (page >= 1 && page <= totalPages()) {
        currentPage.set(page);
      }
    },
    prevPage: () => {
      if (safePage() > 1) currentPage.set(safePage() - 1);
    },
    nextPage: () => {
      if (safePage() < totalPages()) currentPage.set(safePage() + 1);
    },
    reset: () => currentPage.set(1)
  };
}
