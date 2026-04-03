import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { assetsApi, InventoryAsset } from '../api/assets';
import { AssetPhoto } from '../components/AssetPhoto';
import { usePermissions } from '../App';

type SortKey = 'tag' | 'beskrivelse' | 'lastObservedAt';
type SortDir = 'asc' | 'desc';

const SORT_OPTIONS: { key: SortKey; label: string }[] = [
  { key: 'tag', label: 'Tag' },
  { key: 'beskrivelse', label: 'Navn' },
  { key: 'lastObservedAt', label: 'Observert' },
];

function sortAssets(assets: InventoryAsset[], key: SortKey, dir: SortDir): InventoryAsset[] {
  const sorted = [...assets].sort((a, b) => {
    if (key === 'lastObservedAt') {
      const av = a.lastObservedAt ? new Date(a.lastObservedAt).getTime() : 0;
      const bv = b.lastObservedAt ? new Date(b.lastObservedAt).getTime() : 0;
      return av - bv;
    }
    return (a[key] ?? '').localeCompare(b[key] ?? '', 'nb');
  });
  return dir === 'desc' ? sorted.reverse() : sorted;
}

export function AssetList() {
  const navigate = useNavigate();
  const { canManage } = usePermissions();
  const [assets, setAssets] = useState<InventoryAsset[]>([]);
  const [search, setSearch] = useState('');
  const [borrowedOnly, setBorrowedOnly] = useState(false);
  const [sortKey, setSortKey] = useState<SortKey>('tag');
  const [sortDir, setSortDir] = useState<SortDir>('asc');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const t = setTimeout(() => {
      setLoading(true);
      assetsApi.list(search, borrowedOnly).then(data => {
        setAssets(data);
        setLoading(false);
      }).catch((e: Error) => {
        setError(e.message);
        setLoading(false);
      });
    }, 300);
    return () => clearTimeout(t);
  }, [search, borrowedOnly]);

  const handleSort = (key: SortKey) => {
    if (key === sortKey) {
      setSortDir(d => d === 'asc' ? 'desc' : 'asc');
    } else {
      setSortKey(key);
      setSortDir('asc');
    }
  };

  const displayedAssets = sortAssets(assets, sortKey, sortDir);

  return (
    <div style={{ padding: '16px', maxWidth: '700px', margin: '0 auto' }}>
      <div style={{ display: 'flex', alignItems: 'center', gap: '12px', marginBottom: '12px' }}>
        <button onClick={() => navigate('/')} style={{ background: 'none', border: 'none', fontSize: '14px', color: '#1976d2', cursor: 'pointer', padding: 0, whiteSpace: 'nowrap' }}>
          ← Hjem
        </button>
        <input
          type="search"
          value={search}
          onChange={e => setSearch(e.target.value)}
          placeholder="Søk på tag, beskrivelse, merke..."
          style={{ flex: 1, padding: '10px', fontSize: '16px', borderRadius: '6px', border: '1px solid #ccc' }}
          autoFocus
        />
      </div>

      <button
        onClick={() => setBorrowedOnly(b => !b)}
        style={{
          marginBottom: '8px',
          padding: '8px 16px',
          borderRadius: '20px',
          border: '2px solid',
          borderColor: borrowedOnly ? '#e65100' : '#ccc',
          backgroundColor: borrowedOnly ? '#fff3e0' : '#fff',
          color: borrowedOnly ? '#e65100' : '#555',
          fontWeight: borrowedOnly ? 'bold' : 'normal',
          cursor: 'pointer',
          fontSize: '14px',
        }}
      >
        {borrowedOnly ? '✕ ' : ''}Vis kun utlånt
      </button>

      <div style={{ display: 'flex', gap: '6px', marginBottom: '12px', flexWrap: 'wrap' }}>
        {SORT_OPTIONS.map(opt => {
          const active = sortKey === opt.key;
          return (
            <button
              key={opt.key}
              onClick={() => handleSort(opt.key)}
              style={{
                padding: '6px 12px',
                borderRadius: '20px',
                border: '2px solid',
                borderColor: active ? '#1976d2' : '#ccc',
                backgroundColor: active ? '#e3f2fd' : '#fff',
                color: active ? '#1976d2' : '#555',
                fontWeight: active ? 'bold' : 'normal',
                cursor: 'pointer',
                fontSize: '13px',
              }}
            >
              {opt.label} {active ? (sortDir === 'asc' ? '↑' : '↓') : ''}
            </button>
          );
        })}
      </div>

      {error && <div style={{ color: '#c62828', padding: '10px', backgroundColor: '#ffebee', borderRadius: '6px', marginBottom: '12px' }}>{error}</div>}

      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '8px' }}>
        <span style={{ color: '#555', fontSize: '14px' }}>
          {loading ? 'Laster...' : `${displayedAssets.length} gjenstander`}
        </span>
        {canManage && (
          <a
            href="/api/inventory/assets/export"
            download
            style={{ fontSize: '13px', color: '#1976d2', textDecoration: 'none', padding: '4px 10px', border: '1px solid #1976d2', borderRadius: '6px' }}
          >
            Eksporter CSV
          </a>
        )}
      </div>

      {displayedAssets.map(asset => (
        <div
          key={asset.id}
          style={{
            display: 'flex', gap: '12px', alignItems: 'flex-start',
            padding: '12px 14px',
            backgroundColor: '#fff',
            border: '1px solid',
            borderColor: asset.currentBorrowId ? '#ff9800' : '#e0e0e0',
            borderRadius: '8px',
            marginBottom: '8px',
          }}
        >
          {asset.photoUrl && (
            <AssetPhoto
              url={asset.photoUrl}
              tag={asset.tag}
              style={{ width: '52px', height: '52px', objectFit: 'cover', borderRadius: '4px', flexShrink: 0 }}
            />
          )}
          <div style={{ flex: 1, minWidth: 0 }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
              <div>
                <code style={{ fontWeight: 'bold', fontSize: '16px', color: '#1a1a1a' }}>{asset.tag}</code>
                {asset.kategori && <span style={{ marginLeft: '8px', fontSize: '12px', color: '#666', backgroundColor: '#f0f0f0', padding: '2px 6px', borderRadius: '10px' }}>{asset.kategori}</span>}
                {asset.subKategori && <span style={{ marginLeft: '4px', fontSize: '12px', color: '#555', backgroundColor: '#e8eaf6', padding: '2px 6px', borderRadius: '10px' }}>{asset.subKategori}</span>}
              </div>
              {asset.currentBorrowId ? (
                <span style={{ fontSize: '12px', backgroundColor: '#fff3e0', color: '#e65100', padding: '2px 8px', borderRadius: '10px', fontWeight: 'bold', whiteSpace: 'nowrap' }}>Utlånt</span>
              ) : (
                <span style={{ fontSize: '12px', backgroundColor: '#e8f5e9', color: '#2e7d32', padding: '2px 8px', borderRadius: '10px', fontWeight: 'bold', whiteSpace: 'nowrap' }}>Tilgjengelig</span>
              )}
            </div>
            <div style={{ marginTop: '4px', color: '#333' }}>{asset.beskrivelse}</div>
            {(asset.merke || asset.modell || asset.lengdeM) && (
              <div style={{ marginTop: '2px', color: '#666', fontSize: '13px' }}>
                {[asset.merke, asset.modell].filter(Boolean).join(' · ')}
                {asset.lengdeM != null && <span style={{ marginLeft: asset.merke || asset.modell ? '6px' : 0 }}>{asset.lengdeM} m</span>}
              </div>
            )}
            {asset.currentBorrowId && (asset.borrowedByEventName || asset.borrowedByUserName) && (
              <div style={{ marginTop: '6px', fontSize: '13px', color: '#bf360c', backgroundColor: '#fff3e0', padding: '4px 8px', borderRadius: '4px' }}>
                {asset.borrowedByEventName && <span>{asset.borrowedByEventName}</span>}
                {asset.borrowedByEventName && asset.borrowedByUserName && <span> · </span>}
                {asset.borrowedByUserName && <span>{asset.borrowedByUserName}</span>}
              </div>
            )}
            {asset.lastObservedAt && (
              <div style={{ marginTop: '4px', textAlign: 'right', fontSize: '11px', color: '#aaa' }}>
                Observert: {new Date(asset.lastObservedAt).toLocaleString('nb-NO', { day: 'numeric', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit' })}
              </div>
            )}
          </div>
        </div>
      ))}
    </div>
  );
}
