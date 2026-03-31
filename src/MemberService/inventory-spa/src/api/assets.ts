import { apiFetch } from './client';

export interface InventoryAsset {
  id: string;
  tag: string;
  kategori?: string;
  subKategori?: string;
  beskrivelse: string;
  merke?: string;
  modell?: string;
  detaljer?: string;
  lengdeM?: number;
  diameter?: number;
  photoUrl?: string;
  inInventory: boolean;
  lokasjon?: string;
  createdAt: string;
  updatedAt: string;
  currentBorrowId?: string;
  borrowedByEventName?: string;
  borrowedByUserName?: string;
}

export interface CsvImportResult {
  successCount: number;
  errorCount: number;
  errors: { rowNumber: number; message: string }[];
}

export interface AssetLookupResult {
  assets: InventoryAsset[];
  notFound: string[];
}

export const assetsApi = {
  list: (search?: string, borrowedOnly?: boolean): Promise<InventoryAsset[]> => {
    const params = new URLSearchParams();
    if (search) params.set('search', search);
    if (borrowedOnly) params.set('borrowedOnly', 'true');
    const qs = params.toString();
    return apiFetch(`/api/inventory/assets${qs ? '?' + qs : ''}`);
  },

  get: (tag: string): Promise<InventoryAsset> =>
    apiFetch(`/api/inventory/assets/${encodeURIComponent(tag)}`),

  create: (data: Partial<InventoryAsset>): Promise<InventoryAsset> =>
    apiFetch('/api/inventory/assets', {
      method: 'POST',
      body: JSON.stringify(data),
    }),

  update: (tag: string, data: Partial<InventoryAsset>): Promise<InventoryAsset> =>
    apiFetch(`/api/inventory/assets/${encodeURIComponent(tag)}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    }),

  delete: (tag: string): Promise<void> =>
    apiFetch(`/api/inventory/assets/${encodeURIComponent(tag)}`, {
      method: 'DELETE',
    }),

  importCsv: (content: string): Promise<CsvImportResult> =>
    apiFetch('/api/inventory/assets/import', {
      method: 'POST',
      body: JSON.stringify({ content }),
    }),

  lookup: (tags: string[]): Promise<AssetLookupResult> =>
    apiFetch('/api/inventory/assets/lookup', {
      method: 'POST',
      body: JSON.stringify({ tags }),
    }),
};
