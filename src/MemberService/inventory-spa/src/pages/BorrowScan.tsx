import { useState, useEffect, useRef } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { borrowsApi, BorrowSession } from '../api/borrows';
import { assetsApi, InventoryAsset } from '../api/assets';
import { ApiError } from '../api/client';
import { QrScanner } from '../components/QrScanner';

const TYPE_LABELS: Record<string, string> = {
  Borrow: 'Lån ut',
  Return: 'Returner',
  InventoryCheck: 'Tell lager',
};

export function BorrowScan() {
  const { sessionId } = useParams<{ sessionId: string }>();
  const navigate = useNavigate();

  const [session, setSession] = useState<BorrowSession | null>(null);
  const [borrowedAssets, setBorrowedAssets] = useState<InventoryAsset[]>([]);
  const [error, setError] = useState('');
  const [lastScanned, setLastScanned] = useState('');
  const [scanning, setScanning] = useState(false);
  const [manualTag, setManualTag] = useState('');
  const lastTagRef = useRef('');

  useEffect(() => {
    if (!sessionId) return;
    borrowsApi.get(sessionId).then(s => {
      setSession(s);
      if (s.type === 'Return') {
        assetsApi.list(undefined, true).then(all => {
          setBorrowedAssets(all.filter(a => a.borrowedByEventName === s.eventName));
        }).catch(() => {});
      }
    }).catch((e: Error) => setError(e.message));
  }, [sessionId]);

  const speak = (text: string) => {
    if (!window.speechSynthesis) return;
    window.speechSynthesis.cancel();
    const u = new SpeechSynthesisUtterance(text);
    u.lang = 'nb-NO';
    u.rate = 1.2;
    window.speechSynthesis.speak(u);
  };

  const handleScan = async (tag_scanned: string) => {
    const tag = tag_scanned.trim();
    if (!sessionId || tag === lastTagRef.current) return;
    lastTagRef.current = tag;
    setTimeout(() => { lastTagRef.current = ''; }, 2000);

    setLastScanned(tag);

    const alreadyInSession = session?.items.some(i => i.tag === tag);
    if (alreadyInSession) {
      if (navigator.vibrate) navigator.vibrate([80, 60, 80]);
      speak('Scannet fra før');
      return;
    }

    try {
      const updated = await borrowsApi.scan(sessionId, tag);
      setSession(updated);
      setError('');
      if (navigator.vibrate) navigator.vibrate(120);
      speak('Scannet');
    } catch (e: unknown) {
      if (e instanceof ApiError && e.status === 404) {
        if (navigator.vibrate) navigator.vibrate([100, 50, 100, 50, 100]);
        speak('Ukjent QR kode');
        setError(`Tag "${tag}" ikke funnet i lageret`);
      } else if (e instanceof Error) {
        if (navigator.vibrate) navigator.vibrate([100, 50, 100, 50, 100]);
        setError(e.message || 'Skanningsfeil');
      }
    }
  };

  const handleManualSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!manualTag.trim()) return;
    await handleScan(manualTag.trim());
    setManualTag('');
  };

  const handleRemoveItem = async (itemId: string) => {
    if (!sessionId) return;
    try {
      await borrowsApi.removeItem(sessionId, itemId);
      const updated = await borrowsApi.get(sessionId);
      setSession(updated);
    } catch (e: unknown) {
      if (e instanceof Error) setError(e.message);
    }
  };

  if (!session) {
    return <div style={{ padding: '24px', textAlign: 'center' }}>Laster...</div>;
  }

  return (
    <div style={{ padding: '16px', maxWidth: '600px', margin: '0 auto' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '12px' }}>
        <button onClick={() => navigate('/')} style={{ background: 'none', border: 'none', fontSize: '14px', color: '#1976d2', cursor: 'pointer', padding: 0 }}>
          ← Hjem
        </button>
        <button
          onClick={() => navigate(`/borrow/${sessionId}/review`)}
          style={{ padding: '10px 20px', backgroundColor: '#1976d2', color: '#fff', border: 'none', borderRadius: '6px', fontWeight: 'bold', cursor: 'pointer', fontSize: '15px' }}
        >
          Gjennomgang ({session.items.length})
        </button>
      </div>

      <div style={{ backgroundColor: '#e3f2fd', padding: '10px 14px', borderRadius: '6px', marginBottom: '12px' }}>
        <strong>{session.eventName}</strong> · {TYPE_LABELS[session.type] || session.type}
      </div>

      <button
        onClick={() => setScanning(s => !s)}
        style={{
          width: '100%',
          padding: '14px',
          fontSize: '16px',
          fontWeight: 'bold',
          borderRadius: '8px',
          border: '2px solid #1976d2',
          backgroundColor: scanning ? '#1976d2' : '#fff',
          color: scanning ? '#fff' : '#1976d2',
          cursor: 'pointer',
          marginBottom: '12px',
        }}
      >
        {scanning ? 'Stopp kamera' : 'Start kamera'}
      </button>

      {scanning && <QrScanner onScan={handleScan} />}

      <form onSubmit={handleManualSubmit} style={{ display: 'flex', gap: '8px', marginBottom: '16px' }}>
        <input
          type="text"
          value={manualTag}
          onChange={e => setManualTag(e.target.value)}
          placeholder="Skriv inn tag (f.eks. S-001)"
          style={{ flex: 1, padding: '10px', fontSize: '16px', borderRadius: '6px', border: '1px solid #ccc' }}
        />
        <button type="submit" style={{ padding: '10px 16px', backgroundColor: '#388e3c', color: '#fff', border: 'none', borderRadius: '6px', fontWeight: 'bold', cursor: 'pointer' }}>
          +
        </button>
      </form>

      {lastScanned && (
        <div style={{ padding: '8px 12px', backgroundColor: '#e8f5e9', borderRadius: '6px', marginBottom: '8px', color: '#1b5e20', fontFamily: 'monospace' }}>
          Skannet: {lastScanned}
        </div>
      )}

      {error && (
        <div style={{ padding: '10px 12px', backgroundColor: '#ffebee', color: '#c62828', borderRadius: '6px', marginBottom: '8px' }}>
          {error}
        </div>
      )}

      <div style={{ fontWeight: 'bold', marginBottom: '8px', color: '#333' }}>
        Skannet ({session.items.length})
      </div>
      {session.items.length === 0 && (
        <div style={{ color: '#666', fontStyle: 'italic', padding: '20px', textAlign: 'center', border: '1px dashed #ccc', borderRadius: '8px' }}>
          Ingen gjenstander skannet ennå
        </div>
      )}
      {[...session.items].reverse().map(item => (
        <div key={item.id} style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', padding: '10px 12px', backgroundColor: '#fff', border: '1px solid #e0e0e0', borderRadius: '6px', marginBottom: '6px' }}>
          <div style={{ minWidth: 0 }}>
            <code style={{ fontWeight: 'bold', color: '#1a1a1a', display: 'block' }}>{item.tag}</code>
            {item.beskrivelse && <div style={{ color: '#333', fontSize: '13px' }}>{item.beskrivelse}</div>}
            {(item.merke || item.modell) && (
              <div style={{ color: '#666', fontSize: '12px' }}>{[item.merke, item.modell].filter(Boolean).join(' · ')}</div>
            )}
          </div>
          <button
            onClick={() => handleRemoveItem(item.id)}
            style={{ background: 'none', border: 'none', color: '#c62828', cursor: 'pointer', fontSize: '20px', lineHeight: 1, padding: '0 4px', flexShrink: 0 }}
          >
            ×
          </button>
        </div>
      ))}

      {session.type === 'Return' && borrowedAssets.length > 0 && (
        <div style={{ marginTop: '24px' }}>
          <div style={{ fontWeight: 'bold', marginBottom: '8px', color: '#333' }}>
            Lånt av {session.eventName} ({borrowedAssets.length})
          </div>
          {borrowedAssets.map(asset => {
            const alreadyScanned = session.items.some(i => i.tag === asset.tag);
            return (
              <div
                key={asset.id}
                style={{
                  display: 'flex', justifyContent: 'space-between', alignItems: 'center',
                  padding: '10px 12px',
                  backgroundColor: alreadyScanned ? '#e8f5e9' : '#fff',
                  border: '1px solid',
                  borderColor: alreadyScanned ? '#a5d6a7' : '#e0e0e0',
                  borderRadius: '6px', marginBottom: '6px',
                  opacity: alreadyScanned ? 0.6 : 1,
                }}
              >
                <div>
                  <code style={{ fontWeight: 'bold', color: '#1a1a1a', marginRight: '8px' }}>{asset.tag}</code>
                  <span style={{ color: '#333', fontSize: '14px' }}>{asset.beskrivelse}</span>
                </div>
                {alreadyScanned && <span style={{ fontSize: '18px', color: '#43a047' }}>✓</span>}
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}
