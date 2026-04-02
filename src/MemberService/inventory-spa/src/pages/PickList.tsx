import { useState, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { assetsApi, InventoryAsset } from '../api/assets';
import { borrowsApi } from '../api/borrows';
import { AssetPhoto } from '../components/AssetPhoto';
import { QrScanner } from '../components/QrScanner';

const EVENT_NAMES = [
  'Winter Jump',
  'Juni Jazz',
  'Shag up',
  'Sosialdanskomiteen',
  "O'Slow",
  'Oslo Balboa Weekend',
  'Just me',
];

const extractTags = (text: string): string[] => {
  const matches = text.match(/\b[A-ZÆØÅ]+-\d+\b/gi) ?? [];
  return [...new Set(matches.map(t => t.toUpperCase()))];
};

type Phase = 'select' | 'pick' | 'done';

export function PickList() {
  const navigate = useNavigate();

  // --- Selection phase state ---
  const [text, setText] = useState('');
  const [assets, setAssets] = useState<InventoryAsset[] | null>(null);
  const [notFound, setNotFound] = useState<string[]>([]);
  const [checked, setChecked] = useState<Set<string>>(new Set());
  const [selectedEvent, setSelectedEvent] = useState('');
  const [customEvent, setCustomEvent] = useState('');
  const [lookupLoading, setLookupLoading] = useState(false);
  const [starting, setStarting] = useState(false);
  const [error, setError] = useState('');

  // --- Pick phase state ---
  const [phase, setPhase] = useState<Phase>('select');
  const [sessionId, setSessionId] = useState('');
  const [queue, setQueue] = useState<InventoryAsset[]>([]);
  const [currentIndex, setCurrentIndex] = useState(0);
  const [pickedCount, setPickedCount] = useState(0);
  const [scanError, setScanError] = useState('');
  const [scanOk, setScanOk] = useState(false);
  const lastTagRef = useRef('');

  const eventName = selectedEvent === '__custom' ? customEvent : selectedEvent;
  const extractedTags = extractTags(text);

  // --- Selection phase handlers ---

  const handleLookup = async () => {
    if (extractedTags.length === 0) return;
    setLookupLoading(true);
    setError('');
    try {
      const result = await assetsApi.lookup(extractedTags);
      setAssets(result.assets);
      setNotFound(result.notFound);
      setChecked(new Set(result.assets.map(a => a.tag)));
    } catch (e: any) {
      setError(e.message || 'Noe gikk galt');
    } finally {
      setLookupLoading(false);
    }
  };

  const toggle = (tag: string) => {
    setChecked(prev => {
      const next = new Set(prev);
      if (next.has(tag)) next.delete(tag); else next.add(tag);
      return next;
    });
  };

  const handleStartPick = async () => {
    if (!eventName.trim() || !assets) return;
    setStarting(true);
    setError('');
    try {
      const session = await borrowsApi.start(eventName.trim(), 'Borrow');
      const selectedAssets = assets.filter(a => checked.has(a.tag));
      setSessionId(session.id);
      setQueue(selectedAssets);
      setCurrentIndex(0);
      setPickedCount(0);
      setScanError('');
      setPhase('pick');
    } catch (e: any) {
      setError(e.message || 'Noe gikk galt');
      setStarting(false);
    }
  };

  // --- Pick phase handlers ---

  const currentItem = queue[currentIndex];

  const handleScan = async (tag_scanned: string) => {
    const tag = tag_scanned.trim();
    if (tag === lastTagRef.current || !currentItem) return;
    lastTagRef.current = tag;
    setTimeout(() => { lastTagRef.current = ''; }, 2000);

    if (tag.toUpperCase() !== currentItem.tag.toUpperCase()) {
      setScanError(`Feil gjenstand — skannet ${tag}, forventet ${currentItem.tag}`);
      return;
    }

    if (navigator.vibrate) navigator.vibrate([100, 50, 100]);
    setScanError('');
    setScanOk(true);
    setTimeout(() => setScanOk(false), 600);

    try {
      await borrowsApi.scan(sessionId, tag);
      setPickedCount(c => c + 1);
      setCurrentIndex(i => i + 1);
    } catch (e: any) {
      setScanError(e.message || 'Skanningsfeil');
    }
  };

  const handleManualSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const input = e.currentTarget.elements.namedItem('tag') as HTMLInputElement;
    const tag = input.value.trim();
    if (tag) { handleScan(tag); input.value = ''; }
  };

  const handleSkip = () => {
    setScanError('');
    setCurrentIndex(i => i + 1);
  };

  const handleAbort = () => {
    navigate(`/borrow/${sessionId}/review`);
  };

  // ===================== DONE SCREEN =====================
  if (phase === 'pick' && currentIndex >= queue.length) {
    return (
      <div style={{ padding: '32px 24px', maxWidth: '480px', margin: '0 auto', textAlign: 'center' }}>
        <div style={{ fontSize: '72px', marginBottom: '16px', color: '#2e7d32' }}>✓</div>
        <h2 style={{ marginBottom: '8px', color: '#1a1a1a' }}>Plukking ferdig!</h2>
        <div style={{ color: '#555', fontSize: '16px', marginBottom: '32px' }}>
          {pickedCount} av {queue.length} gjenstander plukket
        </div>
        <button
          onClick={() => navigate(`/borrow/${sessionId}/review`)}
          style={{ width: '100%', padding: '16px', fontSize: '18px', fontWeight: 'bold', borderRadius: '8px', border: 'none', backgroundColor: '#1976d2', color: '#fff', cursor: 'pointer', boxSizing: 'border-box' }}
        >
          Gå til gjennomgang →
        </button>
      </div>
    );
  }

  // ===================== PICK MODE =====================
  if (phase === 'pick' && currentItem) {
    return (
      <div style={{ padding: '12px 16px', maxWidth: '600px', margin: '0 auto' }}>

        {/* Header bar */}
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '12px' }}>
          <div style={{ fontSize: '15px', fontWeight: 'bold', color: '#555' }}>
            {pickedCount} / {queue.length} plukket
          </div>
          <button
            onClick={handleAbort}
            style={{ padding: '8px 16px', border: '1px solid #bbb', backgroundColor: '#fff', borderRadius: '6px', cursor: 'pointer', fontSize: '14px', color: '#333' }}
          >
            Avbryt
          </button>
        </div>

        {/* Progress bar */}
        <div style={{ height: '6px', backgroundColor: '#e0e0e0', borderRadius: '3px', marginBottom: '16px', overflow: 'hidden' }}>
          <div style={{ height: '100%', backgroundColor: '#1976d2', borderRadius: '3px', width: `${(pickedCount / queue.length) * 100}%`, transition: 'width 0.3s' }} />
        </div>

        {/* Current item card */}
        <div
          style={{
            backgroundColor: scanOk ? '#e8f5e9' : '#e3f2fd',
            border: `2px solid ${scanOk ? '#43a047' : '#1976d2'}`,
            borderRadius: '12px',
            padding: '16px',
            marginBottom: '14px',
            textAlign: 'center',
            transition: 'background-color 0.2s, border-color 0.2s',
          }}
        >
          <div style={{ fontSize: '11px', fontWeight: 'bold', letterSpacing: '0.05em', color: scanOk ? '#2e7d32' : '#1565c0', textTransform: 'uppercase', marginBottom: '10px' }}>
            {scanOk ? 'Plukket!' : 'Plukk denne'}
          </div>

          {currentItem.photoUrl && (
            <div style={{ display: 'flex', justifyContent: 'center', marginBottom: '12px' }}>
              <AssetPhoto
                url={currentItem.photoUrl}
                tag={currentItem.tag}
                style={{ width: '160px', height: '160px', objectFit: 'contain', borderRadius: '8px' }}
              />
            </div>
          )}

          <code style={{ fontSize: '24px', fontWeight: 'bold', color: '#1a1a1a', display: 'block', marginBottom: '6px' }}>
            {currentItem.tag}
          </code>
          <div style={{ color: '#222', fontSize: '16px', marginBottom: '4px' }}>{currentItem.beskrivelse}</div>
          {(currentItem.merke || currentItem.modell) && (
            <div style={{ color: '#666', fontSize: '13px' }}>
              {[currentItem.merke, currentItem.modell].filter(Boolean).join(' · ')}
            </div>
          )}
          {currentItem.lokasjon && (
            <div style={{ marginTop: '6px', fontSize: '13px', color: '#1976d2' }}>
              Lokasjon: {currentItem.lokasjon}
            </div>
          )}
        </div>

        {/* Scanner */}
        <QrScanner onScan={handleScan} />

        {/* Manual input */}
        <form onSubmit={handleManualSubmit} style={{ display: 'flex', gap: '8px', marginTop: '10px', marginBottom: '10px' }}>
          <input
            name="tag"
            type="text"
            placeholder="Skriv inn tag manuelt"
            style={{ flex: 1, padding: '10px', fontSize: '16px', borderRadius: '6px', border: '1px solid #ccc' }}
          />
          <button type="submit" style={{ padding: '10px 16px', backgroundColor: '#388e3c', color: '#fff', border: 'none', borderRadius: '6px', fontWeight: 'bold', cursor: 'pointer' }}>
            OK
          </button>
        </form>

        {scanError && (
          <div style={{ padding: '10px 12px', backgroundColor: '#ffebee', color: '#c62828', borderRadius: '6px', marginBottom: '10px', fontSize: '14px' }}>
            {scanError}
          </div>
        )}

        {/* Skip button */}
        <button
          onClick={handleSkip}
          style={{ width: '100%', padding: '12px', border: '2px solid #e0e0e0', backgroundColor: '#fafafa', borderRadius: '8px', cursor: 'pointer', fontSize: '15px', fontWeight: 'bold', color: '#555', boxSizing: 'border-box' }}
        >
          Hopp over →
        </button>

        {/* Next items preview */}
        {queue.slice(currentIndex + 1, currentIndex + 4).length > 0 && (
          <div style={{ marginTop: '14px', color: '#888', fontSize: '13px' }}>
            <span>Neste: </span>
            {queue.slice(currentIndex + 1, currentIndex + 4).map(a => (
              <span key={a.tag} style={{ display: 'inline-block', margin: '2px', padding: '2px 8px', backgroundColor: '#f0f0f0', borderRadius: '8px', fontFamily: 'monospace', fontSize: '12px' }}>
                {a.tag}
              </span>
            ))}
          </div>
        )}
      </div>
    );
  }

  // ===================== SELECTION PHASE =====================
  return (
    <div style={{ padding: '16px', maxWidth: '600px', margin: '0 auto' }}>
      <button onClick={() => navigate('/')} style={{ background: 'none', border: 'none', fontSize: '14px', color: '#1976d2', cursor: 'pointer', padding: 0, marginBottom: '16px' }}>
        ← Hjem
      </button>
      <h2 style={{ marginBottom: '12px', color: '#1a1a1a' }}>Plukkliste</h2>

      <div style={{ marginBottom: '10px', color: '#555', fontSize: '14px' }}>
        Lim inn tekst (e-post, regneark, chat) som inneholder tag-IDer som K-001, S-003 osv.
      </div>

      <textarea
        value={text}
        onChange={e => { setText(e.target.value); setAssets(null); }}
        placeholder="Lim inn tekst her..."
        rows={5}
        style={{ width: '100%', padding: '10px', fontSize: '15px', borderRadius: '6px', border: '1px solid #ccc', boxSizing: 'border-box', resize: 'vertical', fontFamily: 'inherit', marginBottom: '10px' }}
      />

      {extractedTags.length > 0 && (
        <div style={{ marginBottom: '12px' }}>
          <span style={{ fontSize: '13px', color: '#555' }}>Fant {extractedTags.length} tag{extractedTags.length !== 1 ? 'ger' : ''}: </span>
          {extractedTags.map(t => (
            <span key={t} style={{ display: 'inline-block', margin: '2px', padding: '2px 8px', backgroundColor: '#e3f2fd', borderRadius: '10px', fontSize: '13px', fontFamily: 'monospace' }}>{t}</span>
          ))}
        </div>
      )}

      {assets === null && (
        <button
          onClick={handleLookup}
          disabled={lookupLoading || extractedTags.length === 0}
          style={{ width: '100%', padding: '12px', fontSize: '16px', fontWeight: 'bold', borderRadius: '8px', border: 'none', backgroundColor: lookupLoading || extractedTags.length === 0 ? '#e0e0e0' : '#1976d2', color: lookupLoading || extractedTags.length === 0 ? '#888' : '#fff', cursor: lookupLoading || extractedTags.length === 0 ? 'not-allowed' : 'pointer', marginBottom: '16px', boxSizing: 'border-box' }}
        >
          {lookupLoading ? 'Søker...' : 'Søk opp utstyr'}
        </button>
      )}

      {error && (
        <div style={{ color: '#c62828', padding: '10px', backgroundColor: '#ffebee', borderRadius: '6px', marginBottom: '16px' }}>{error}</div>
      )}

      {assets !== null && (
        <>
          {notFound.length > 0 && (
            <div style={{ marginBottom: '12px', padding: '8px 12px', backgroundColor: '#fff8e1', border: '1px solid #ffe082', borderRadius: '6px', fontSize: '13px', color: '#795548' }}>
              Ikke funnet: {notFound.join(', ')}
            </div>
          )}

          <div style={{ marginBottom: '6px', color: '#555', fontSize: '14px' }}>
            {assets.length} gjenstander — {checked.size} valgt
            <button onClick={() => setAssets(null)} style={{ marginLeft: '12px', background: 'none', border: 'none', color: '#1976d2', cursor: 'pointer', fontSize: '13px', padding: 0 }}>
              Endre tekst
            </button>
          </div>

          {assets.map(asset => (
            <div
              key={asset.id}
              onClick={() => toggle(asset.tag)}
              style={{ display: 'flex', alignItems: 'center', gap: '12px', padding: '10px 12px', backgroundColor: checked.has(asset.tag) ? '#f3f7ff' : '#fafafa', border: '1px solid', borderColor: checked.has(asset.tag) ? '#1976d2' : '#e0e0e0', borderRadius: '8px', marginBottom: '6px', cursor: 'pointer' }}
            >
              <input
                type="checkbox"
                checked={checked.has(asset.tag)}
                onChange={() => toggle(asset.tag)}
                onClick={e => e.stopPropagation()}
                style={{ width: '20px', height: '20px', cursor: 'pointer', flexShrink: 0 }}
              />
              {asset.photoUrl && (
                <div onClick={e => e.stopPropagation()}>
                  <AssetPhoto url={asset.photoUrl} tag={asset.tag} style={{ width: '44px', height: '44px', objectFit: 'cover', borderRadius: '4px', flexShrink: 0 }} />
                </div>
              )}
              <div style={{ flex: 1, minWidth: 0 }}>
                <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                  <code style={{ fontWeight: 'bold', color: '#1a1a1a' }}>{asset.tag}</code>
                  {asset.currentBorrowId
                    ? <span style={{ fontSize: '11px', backgroundColor: '#fff3e0', color: '#e65100', padding: '1px 6px', borderRadius: '10px' }}>Utlånt</span>
                    : <span style={{ fontSize: '11px', backgroundColor: '#e8f5e9', color: '#2e7d32', padding: '1px 6px', borderRadius: '10px' }}>Tilgjengelig</span>
                  }
                </div>
                <div style={{ color: '#333', fontSize: '14px', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{asset.beskrivelse}</div>
                {asset.kategori && <div style={{ color: '#888', fontSize: '12px' }}>{asset.kategori}</div>}
              </div>
            </div>
          ))}

          {/* Event selector */}
          <div style={{ marginTop: '20px' }}>
            <div style={{ fontWeight: 'bold', marginBottom: '8px' }}>Arrangement</div>
            {EVENT_NAMES.map(name => (
              <button key={name} onClick={() => setSelectedEvent(name)} style={{ display: 'inline-block', margin: '4px', padding: '8px 14px', borderRadius: '20px', border: '2px solid', borderColor: selectedEvent === name ? '#1976d2' : '#ccc', backgroundColor: selectedEvent === name ? '#e3f2fd' : '#fff', color: '#1a1a1a', cursor: 'pointer', fontSize: '14px' }}>
                {name}
              </button>
            ))}
            <button onClick={() => setSelectedEvent('__custom')} style={{ display: 'inline-block', margin: '4px', padding: '8px 14px', borderRadius: '20px', border: '2px solid', borderColor: selectedEvent === '__custom' ? '#1976d2' : '#ccc', backgroundColor: selectedEvent === '__custom' ? '#e3f2fd' : '#fff', color: '#1a1a1a', cursor: 'pointer', fontSize: '14px' }}>
              Annet...
            </button>
            {selectedEvent === '__custom' && (
              <input type="text" placeholder="Skriv inn arrangementsnavnet" value={customEvent} onChange={e => setCustomEvent(e.target.value)} style={{ display: 'block', width: '100%', marginTop: '8px', padding: '10px', fontSize: '16px', borderRadius: '6px', border: '1px solid #ccc', boxSizing: 'border-box' }} autoFocus />
            )}
          </div>

          <button
            onClick={handleStartPick}
            disabled={starting || checked.size === 0 || !eventName.trim()}
            style={{ width: '100%', marginTop: '16px', padding: '14px', fontSize: '16px', fontWeight: 'bold', borderRadius: '8px', border: 'none', backgroundColor: starting || checked.size === 0 || !eventName.trim() ? '#e0e0e0' : '#00838f', color: starting || checked.size === 0 || !eventName.trim() ? '#888' : '#fff', cursor: starting || checked.size === 0 || !eventName.trim() ? 'not-allowed' : 'pointer', boxSizing: 'border-box' }}
          >
            {starting ? 'Starter...' : `Start plukking av ${checked.size} gjenstander`}
          </button>
        </>
      )}
    </div>
  );
}
