import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { assetsApi, InventoryAsset } from '../api/assets';

export function AssetList() {
  const navigate = useNavigate();
  const [assets, setAssets] = useState<InventoryAsset[]>([]);
  const [search, setSearch] = useState('');
  const [borrowedOnly, setBorrowedOnly] = useState(false);
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
          marginBottom: '12px',
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

      {error && <div style={{ color: '#c62828', padding: '10px', backgroundColor: '#ffebee', borderRadius: '6px', marginBottom: '12px' }}>{error}</div>}

      <div style={{ marginBottom: '8px', color: '#555', fontSize: '14px' }}>
        {loading ? 'Laster...' : `${assets.length} gjenstander`}
      </div>

      {assets.map(asset => (
        <div
          key={asset.id}
          style={{
            padding: '12px 14px',
            backgroundColor: '#fff',
            border: '1px solid',
            borderColor: asset.currentBorrowId ? '#ff9800' : '#e0e0e0',
            borderRadius: '8px',
            marginBottom: '8px',
          }}
        >
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
            <div>
              <code style={{ fontWeight: 'bold', fontSize: '16px', color: '#1a1a1a' }}>{asset.tag}</code>
              {asset.kategori && <span style={{ marginLeft: '8px', fontSize: '12px', color: '#666', backgroundColor: '#f0f0f0', padding: '2px 6px', borderRadius: '10px' }}>{asset.kategori}</span>}
            </div>
            {asset.currentBorrowId ? (
              <span style={{ fontSize: '12px', backgroundColor: '#fff3e0', color: '#e65100', padding: '2px 8px', borderRadius: '10px', fontWeight: 'bold', whiteSpace: 'nowrap' }}>Utlånt</span>
            ) : (
              <span style={{ fontSize: '12px', backgroundColor: '#e8f5e9', color: '#2e7d32', padding: '2px 8px', borderRadius: '10px', fontWeight: 'bold', whiteSpace: 'nowrap' }}>Tilgjengelig</span>
            )}
          </div>
          <div style={{ marginTop: '4px', color: '#333' }}>{asset.beskrivelse}</div>
          {(asset.merke || asset.modell) && (
            <div style={{ marginTop: '2px', color: '#666', fontSize: '13px' }}>
              {[asset.merke, asset.modell].filter(Boolean).join(' · ')}
            </div>
          )}
          {asset.currentBorrowId && (asset.borrowedByEventName || asset.borrowedByUserName) && (
            <div style={{ marginTop: '6px', fontSize: '13px', color: '#bf360c', backgroundColor: '#fff3e0', padding: '4px 8px', borderRadius: '4px' }}>
              {asset.borrowedByEventName && <span>{asset.borrowedByEventName}</span>}
              {asset.borrowedByEventName && asset.borrowedByUserName && <span> · </span>}
              {asset.borrowedByUserName && <span>{asset.borrowedByUserName}</span>}
            </div>
          )}
        </div>
      ))}
    </div>
  );
}
