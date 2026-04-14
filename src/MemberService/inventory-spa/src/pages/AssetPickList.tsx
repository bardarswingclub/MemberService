import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { assetsApi, InventoryAsset } from '../api/assets';
import { AssetPhoto } from '../components/AssetPhoto';

export const PICK_LIST_KEY = 'inventory-asset-picklist';

export const loadPickList = (): string[] => {
  try { return JSON.parse(localStorage.getItem(PICK_LIST_KEY) || '[]'); }
  catch { return []; }
};

export const savePickList = (tags: string[]) => {
  localStorage.setItem(PICK_LIST_KEY, JSON.stringify(tags));
};

export function AssetPickList() {
  const navigate = useNavigate();
  const [tags, setTags] = useState<string[]>(() => loadPickList());
  const [assets, setAssets] = useState<InventoryAsset[]>([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (tags.length === 0) { setAssets([]); return; }
    setLoading(true);
    assetsApi.lookup(tags).then(r => {
      // Preserve order from pick list
      const map = new Map(r.assets.map(a => [a.tag, a]));
      setAssets(tags.map(t => map.get(t)).filter(Boolean) as InventoryAsset[]);
    }).finally(() => setLoading(false));
  }, []);

  const remove = (tag: string) => {
    const updated = tags.filter(t => t !== tag);
    setTags(updated);
    savePickList(updated);
    setAssets(prev => prev.filter(a => a.tag !== tag));
  };

  const clear = () => {
    savePickList([]);
    setTags([]);
    setAssets([]);
  };

  const emailBody = assets.map(a => {
    const line1 = `${a.tag} – ${a.beskrivelse || [a.merke, a.modell].filter(Boolean).join(' ')}`;
    const line2 = [
      [a.merke, a.modell].filter(Boolean).join(' · '),
      a.lengdeM != null ? `${a.lengdeM} m` : '',
    ].filter(Boolean).join(', ');
    return line2 ? `${line1}\n${line2}` : line1;
  }).join('\n\n');

  const mailtoHref = `mailto:?subject=${encodeURIComponent('Plukkliste – utstyr')}&body=${encodeURIComponent(
    `Hei,\n\nHer er utstyrslisten:\n\n${emailBody}\n\nMed vennlig hilsen`
  )}`;

  return (
    <div style={{ padding: '16px', maxWidth: '700px', margin: '0 auto' }}>
      <button
        onClick={() => navigate('/assets')}
        style={{ background: 'none', border: 'none', fontSize: '14px', color: '#1976d2', cursor: 'pointer', padding: 0, marginBottom: '16px' }}
      >
        ← Tilbake til utstyrslisten
      </button>

      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '12px' }}>
        <h2 style={{ margin: 0, color: '#1a1a1a' }}>Plukkliste ({tags.length})</h2>
        {tags.length > 0 && (
          <button
            onClick={clear}
            style={{ background: 'none', border: '1px solid #ccc', borderRadius: '6px', padding: '4px 10px', fontSize: '13px', color: '#666', cursor: 'pointer' }}
          >
            Tøm liste
          </button>
        )}
      </div>

      <div style={{ backgroundColor: '#e3f2fd', borderRadius: '8px', padding: '10px 14px', marginBottom: '16px', fontSize: '13px', color: '#1565c0' }}>
        Sveip et element til <strong>høyre</strong> i utstyrslisten for å legge det til, sveip til <strong>venstre</strong> for å fjerne det.
      </div>

      {loading && <div style={{ color: '#555', padding: '20px', textAlign: 'center' }}>Laster...</div>}

      {!loading && tags.length === 0 && (
        <div style={{ color: '#666', fontStyle: 'italic', padding: '24px', textAlign: 'center', border: '1px dashed #ccc', borderRadius: '8px', marginBottom: '16px' }}>
          Ingen gjenstander i listen ennå
        </div>
      )}

      {assets.map(asset => (
        <div
          key={asset.id}
          style={{ display: 'flex', gap: '12px', alignItems: 'flex-start', padding: '12px 14px', backgroundColor: '#fff', border: '1px solid #ce93d8', borderRadius: '8px', marginBottom: '8px' }}
        >
          {asset.photoUrl && (
            <AssetPhoto
              url={asset.photoUrl}
              tag={asset.tag}
              style={{ width: '48px', height: '48px', objectFit: 'cover', borderRadius: '4px', flexShrink: 0 }}
            />
          )}
          <div style={{ flex: 1, minWidth: 0 }}>
            <code style={{ fontWeight: 'bold', fontSize: '15px', color: '#1a1a1a' }}>{asset.tag}</code>
            {asset.beskrivelse && <div style={{ color: '#333', fontSize: '13px', marginTop: '2px' }}>{asset.beskrivelse}</div>}
            {(asset.merke || asset.modell || asset.lengdeM) && (
              <div style={{ color: '#666', fontSize: '12px', marginTop: '2px' }}>
                {[asset.merke, asset.modell].filter(Boolean).join(' · ')}
                {asset.lengdeM != null && <span style={{ marginLeft: asset.merke || asset.modell ? '6px' : 0 }}>{asset.lengdeM} m</span>}
              </div>
            )}
          </div>
          <button
            onClick={() => remove(asset.tag)}
            style={{ background: 'none', border: 'none', color: '#c62828', cursor: 'pointer', fontSize: '20px', lineHeight: 1, padding: '0 4px', flexShrink: 0 }}
          >
            ×
          </button>
        </div>
      ))}

      {tags.length > 0 && (
        <a
          href={mailtoHref}
          style={{ display: 'block', marginTop: '16px', padding: '14px', backgroundColor: '#1976d2', color: '#fff', borderRadius: '8px', textAlign: 'center', textDecoration: 'none', fontWeight: 'bold', fontSize: '16px', boxSizing: 'border-box' }}
        >
          Send e-post med plukkliste
        </a>
      )}
    </div>
  );
}
