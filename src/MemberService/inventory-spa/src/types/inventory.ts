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
}

export interface BorrowSession {
  id: string;
  borrowedByUserId: string;
  eventName: string;
  startedAt: string;
  completedAt?: string;
  type: 'Borrow' | 'Return' | 'InventoryCheck';
  items: BorrowItem[];
}

export interface BorrowItem {
  id: string;
  borrowId: string;
  assetId: string;
  asset?: InventoryAsset;
  scannedAt: string;
}
