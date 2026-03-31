import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { assetsApi, InventoryAsset } from '../api/assets';

export function AssetManageList() {
  const navigate = useNavigate();
  const [assets, setAssets] = useState<InventoryAsset[]>([]);
  const [search, setSearch] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const t = setTimeout(() => {
      setLoading(true);
      assetsApi.list(search).then(data => {
        setAssets(data);
        setLoading(false);
      }).catch((e: Error) => {
        setError(e.message);
        setLoading(false);
      });
    }, 300);
    return () => clearTimeout(t);
  }, [search]);

  return (
    <div style={{ padding: '16px', maxWidth: '700px', margin: '0 auto' }}>
      <div style={{ display: 'flex', alignItems: 'center', gap: '12px', marginBottom: '16px' }}>
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

      {error && <div style={{ color: '#c62828', padding: '10px', backgroundColor: '#ffebee', borderRadius: '6px', marginBottom: '12px' }}>{error}</div>}

      <div style={{ marginBottom: '8px', color: '#555', fontSize: '14px' }}>
        {loading ? 'Laster...' : `${assets.length} gjenstander`}
      </div>

      {assets.map(asset => (
        <div
          key={asset.id}
          style={{
            display: 'flex',
            alignItems: 'center',
            gap: '12px',
            padding: '10px 12px',
            backgroundColor: '#fff',
            border: '1px solid #e0e0e0',
            borderRadius: '8px',
            marginBottom: '6px',
          }}
        >
          {asset.photoUrl ? (
            <img
              src={asset.photoUrl}
              alt={asset.tag}
              style={{ width: '48px', height: '48px', objectFit: 'cover', borderRadius: '4px', flexShrink: 0 }}
              onError={e => { (e.target as HTMLImageElement).style.display = 'none'; }}
            />
          ) : (
            <div style={{ width: '48px', height: '48px', backgroundColor: '#f0f0f0', borderRadius: '4px', flexShrink: 0, display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: '18px', color: '#bbb' }}>
              📦
            </div>
          )}

          <div style={{ flex: 1, minWidth: 0 }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
              <code style={{ fontWeight: 'bold', color: '#1a1a1a' }}>{asset.tag}</code>
              {!asset.inInventory && <span style={{ fontSize: '11px', backgroundColor: '#fce4ec', color: '#c62828', padding: '1px 6px', borderRadius: '10px' }}>Ikke i lager</span>}
            </div>
            <div style={{ color: '#333', fontSize: '14px', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{asset.beskrivelse}</div>
            {(asset.merke || asset.modell) && (
              <div style={{ color: '#888', fontSize: '12px' }}>{[asset.merke, asset.modell].filter(Boolean).join(' · ')}</div>
            )}
          </div>

          <button
            onClick={() => navigate(`/assets/${encodeURIComponent(asset.tag)}/edit`)}
            style={{ padding: '8px 16px', backgroundColor: '#1976d2', color: '#fff', border: 'none', borderRadius: '6px', cursor: 'pointer', fontWeight: 'bold', fontSize: '14px', flexShrink: 0 }}
          >
            Rediger
          </button>
        </div>
      ))}
    </div>
  );
}
