import { apiFetch } from './client';

export interface BorrowItem {
  id: string;
  assetId: string;
  tag: string;
  beskrivelse: string;
  scannedAt: string;
}

export interface BorrowSession {
  id: string;
  borrowedByUserId: string;
  borrowedByUserName: string;
  eventName: string;
  type: 'Borrow' | 'Return' | 'InventoryCheck';
  startedAt: string;
  completedAt?: string;
  items: BorrowItem[];
}

export const borrowsApi = {
  list: (): Promise<BorrowSession[]> =>
    apiFetch('/api/inventory/borrows'),

  get: (id: string): Promise<BorrowSession> =>
    apiFetch(`/api/inventory/borrows/${id}`),

  start: (eventName: string, type: BorrowSession['type']): Promise<BorrowSession> =>
    apiFetch('/api/inventory/borrows', {
      method: 'POST',
      body: JSON.stringify({ eventName, type }),
    }),

  scan: (id: string, tag: string): Promise<BorrowSession> =>
    apiFetch(`/api/inventory/borrows/${id}/scan`, {
      method: 'POST',
      body: JSON.stringify({ tag }),
    }),

  removeItem: (id: string, itemId: string): Promise<void> =>
    apiFetch(`/api/inventory/borrows/${id}/items/${itemId}`, {
      method: 'DELETE',
    }),

  complete: (id: string): Promise<BorrowSession> =>
    apiFetch(`/api/inventory/borrows/${id}/complete`, {
      method: 'POST',
    }),
};
