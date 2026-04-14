import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { assetsApi, InventoryAsset } from '../api/assets';
import { AssetPhoto } from '../components/AssetPhoto';

type FormState = {
  beskrivelse: string;
  kategori: string;
  subKategori: string;
  merke: string;
  modell: string;
  detaljer: string;
  lengdeM: string;
  diameter: string;
  photoUrl: string;
  inInventory: boolean;
  lokasjon: string;
};

const field: React.CSSProperties = {
  display: 'block',
  width: '100%',
  padding: '10px',
  fontSize: '16px',
  borderRadius: '6px',
  border: '1px solid #ccc',
  boxSizing: 'border-box',
  marginBottom: '12px',
};

const label: React.CSSProperties = {
  display: 'block',
  fontWeight: 'bold',
  marginBottom: '4px',
  fontSize: '14px',
  color: '#333',
};

function toForm(asset: InventoryAsset): FormState {
  return {
    beskrivelse: asset.beskrivelse ?? '',
    kategori: asset.kategori ?? '',
    subKategori: asset.subKategori ?? '',
    merke: asset.merke ?? '',
    modell: asset.modell ?? '',
    detaljer: asset.detaljer ?? '',
    lengdeM: asset.lengdeM != null ? String(asset.lengdeM) : '',
    diameter: asset.diameter != null ? String(asset.diameter) : '',
    photoUrl: asset.photoUrl ?? '',
    inInventory: asset.inInventory,
    lokasjon: asset.lokasjon ?? '',
  };
}

export function AssetEditForm() {
  const { tag } = useParams<{ tag: string }>();
  const navigate = useNavigate();

  const [form, setForm] = useState<FormState | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [saved, setSaved] = useState(false);
  const [photoError, setPhotoError] = useState(false);

  useEffect(() => {
    if (!tag) return;
    assetsApi.get(decodeURIComponent(tag))
      .then(asset => { setForm(toForm(asset)); setLoading(false); })
      .catch((e: Error) => { setError(e.message); setLoading(false); });
  }, [tag]);

  const set = (key: keyof FormState, value: string | boolean) => {
    if (key === 'photoUrl') setPhotoError(false);
    setForm(f => f ? { ...f, [key]: value } : f);
  };

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!form || !tag) return;
    setSaving(true);
    setError('');
    try {
      await assetsApi.update(decodeURIComponent(tag), {
        beskrivelse: form.beskrivelse,
        kategori: form.kategori || undefined,
        subKategori: form.subKategori || undefined,
        merke: form.merke || undefined,
        modell: form.modell || undefined,
        detaljer: form.detaljer || undefined,
        lengdeM: form.lengdeM ? parseFloat(form.lengdeM.replace(',', '.')) : undefined,
        diameter: form.diameter ? parseInt(form.diameter) : undefined,
        photoUrl: form.photoUrl || undefined,
        inInventory: form.inInventory,
        lokasjon: form.lokasjon || undefined,
      });
      setSaved(true);
      setTimeout(() => navigate('/assets/manage'), 800);
    } catch (e: unknown) {
      if (e instanceof Error) setError(e.message);
      setSaving(false);
    }
  };

  if (loading) return <div style={{ padding: '24px', textAlign: 'center' }}>Laster...</div>;
  if (!form) return <div style={{ padding: '24px', color: '#c62828' }}>{error || 'Ikke funnet'}</div>;

  return (
    <div style={{ padding: '16px', maxWidth: '560px', margin: '0 auto' }}>
      <button onClick={() => navigate('/assets/manage')} style={{ background: 'none', border: 'none', fontSize: '14px', color: '#1976d2', cursor: 'pointer', padding: 0, marginBottom: '16px' }}>
        ← Tilbake til listen
      </button>

      <h2 style={{ marginBottom: '4px' }}>
        <code style={{ fontSize: '22px' }}>{decodeURIComponent(tag ?? '')}</code>
      </h2>
      <div style={{ color: '#666', fontSize: '13px', marginBottom: '20px' }}>Tag kan ikke endres</div>

      {error && <div style={{ color: '#c62828', padding: '10px', backgroundColor: '#ffebee', borderRadius: '6px', marginBottom: '16px' }}>{error}</div>}
      {saved && <div style={{ color: '#2e7d32', padding: '10px', backgroundColor: '#e8f5e9', borderRadius: '6px', marginBottom: '16px' }}>Lagret!</div>}

      {/* Photo preview */}
      {form.photoUrl && (
        <div style={{ marginBottom: '16px', textAlign: 'center' }}>
          {photoError ? (
            <div style={{ padding: '12px', backgroundColor: '#fff3e0', border: '1px solid #ffb74d', borderRadius: '8px', color: '#e65100', fontSize: '13px' }}>
              Bildet kunne ikke lastes. Google Drive-lenker fungerer ikke direkte — bruk en direkte bilde-URL (f.eks. fra Imgur eller Google Foto).
            </div>
          ) : (
            <AssetPhoto
              url={form.photoUrl}
              tag={tag ?? ''}
              style={{ maxWidth: '100%', maxHeight: '240px', objectFit: 'contain', borderRadius: '8px', border: '1px solid #e0e0e0' }}
              onLoadError={() => setPhotoError(true)}
            />
          )}
        </div>
      )}

      <form onSubmit={handleSave}>
        <label style={label}>Bilde-URL</label>
        <input type="url" value={form.photoUrl} onChange={e => set('photoUrl', e.target.value)} placeholder="https://..." style={field} />

        <label style={label}>Beskrivelse *</label>
        <input required value={form.beskrivelse} onChange={e => set('beskrivelse', e.target.value)} style={field} />

        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '12px' }}>
          <div>
            <label style={label}>Kategori</label>
            <input value={form.kategori} onChange={e => set('kategori', e.target.value)} style={{ ...field, marginBottom: 0 }} />
          </div>
          <div>
            <label style={label}>Sub-kategori</label>
            <input value={form.subKategori} onChange={e => set('subKategori', e.target.value)} style={{ ...field, marginBottom: 0 }} />
          </div>
        </div>
        <div style={{ marginBottom: '12px' }} />

        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '12px' }}>
          <div>
            <label style={label}>Merke</label>
            <input value={form.merke} onChange={e => set('merke', e.target.value)} style={{ ...field, marginBottom: 0 }} />
          </div>
          <div>
            <label style={label}>Modell</label>
            <input value={form.modell} onChange={e => set('modell', e.target.value)} style={{ ...field, marginBottom: 0 }} />
          </div>
        </div>
        <div style={{ marginBottom: '12px' }} />

        <label style={label}>Detaljer</label>
        <textarea value={form.detaljer} onChange={e => set('detaljer', e.target.value)} rows={2} style={{ ...field, resize: 'vertical', fontFamily: 'inherit' }} />

        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '12px' }}>
          <div>
            <label style={label}>Lengde (m)</label>
            <input type="text" inputMode="decimal" value={form.lengdeM} onChange={e => set('lengdeM', e.target.value)} placeholder="f.eks. 2.5" style={{ ...field, marginBottom: 0 }} />
          </div>
          <div>
            <label style={label}>Diameter (mm)</label>
            <input type="number" value={form.diameter} onChange={e => set('diameter', e.target.value)} style={{ ...field, marginBottom: 0 }} />
          </div>
        </div>
        <div style={{ marginBottom: '12px' }} />

        <label style={label}>Lokasjon</label>
        <input value={form.lokasjon} onChange={e => set('lokasjon', e.target.value)} style={field} />

        <label style={{ display: 'flex', alignItems: 'center', gap: '10px', marginBottom: '20px', cursor: 'pointer' }}>
          <input
            type="checkbox"
            checked={form.inInventory}
            onChange={e => set('inInventory', e.target.checked)}
            style={{ width: '20px', height: '20px', cursor: 'pointer' }}
          />
          <span style={{ fontSize: '15px' }}>I lager (aktiv)</span>
        </label>

        <button
          type="submit"
          disabled={saving || saved}
          style={{
            width: '100%',
            padding: '14px',
            fontSize: '16px',
            fontWeight: 'bold',
            borderRadius: '8px',
            border: 'none',
            backgroundColor: saved ? '#388e3c' : saving ? '#e0e0e0' : '#1976d2',
            color: saving ? '#888' : '#fff',
            cursor: saving || saved ? 'not-allowed' : 'pointer',
          }}
        >
          {saved ? 'Lagret!' : saving ? 'Lagrer...' : 'Lagre'}
        </button>
      </form>
    </div>
  );
}
