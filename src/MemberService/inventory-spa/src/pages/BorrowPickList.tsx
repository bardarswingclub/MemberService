import { useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { QrScanner } from '../components/QrScanner';
import { AssetCard } from '../components/AssetCard';
import { InventoryAsset } from '../types/inventory';
import { MOCK_ASSETS } from '../data/mockAssets';

type Tab = 'paste' | 'qr';

interface LocationState {
  scannedAssets?: InventoryAsset[];
}

export function BorrowPickList() {
  const navigate = useNavigate();
  const location = useLocation();
  const [activeTab, setActiveTab] = useState<Tab>('paste');
  const [pasteValue, setPasteValue] = useState<string>('');
  const [scannedTags, setScannedTags] = useState<Set<string>>(new Set());
  const [scannedAssets, setScannedAssets] = useState<InventoryAsset[]>(
    (location.state as LocationState)?.scannedAssets || []
  );
  const [notification, setNotification] = useState<string>('');

  const handlePaste = () => {
    // Parse tags from paste (comma or newline separated)
    const tags = pasteValue
      .split(/[\n,]/)
      .map((t) => t.trim())
      .filter((t) => t.length > 0);

    if (tags.length === 0) {
      setNotification('No tags to process');
      setTimeout(() => setNotification(''), 2000);
      return;
    }

    let addedCount = 0;
    const notFound: string[] = [];

    for (const tag of tags) {
      if (scannedTags.has(tag)) {
        continue; // Skip already scanned
      }

      const asset = MOCK_ASSETS.find((a) => a.tag === tag);
      if (!asset) {
        notFound.push(tag);
      } else {
        setScannedTags((prev) => new Set([...prev, tag]));
        setScannedAssets((prev) => [...prev, asset]);
        addedCount++;
      }
    }

    setPasteValue('');
    let msg = `Added ${addedCount} items.`;
    if (notFound.length > 0) {
      msg += ` Not found: ${notFound.join(', ')}`;
    }
    setNotification(msg);
    setTimeout(() => setNotification(''), 3000);
  };

  const handleQrScan = (decodedText: string) => {
    const tag = decodedText.trim();

    // Skip if already scanned
    if (scannedTags.has(tag)) {
      beepError();
      return;
    }

    // Find asset by tag
    const asset = MOCK_ASSETS.find((a) => a.tag === tag);
    if (!asset) {
      beepError();
      setNotification(`Unknown tag: ${tag}`);
      setTimeout(() => setNotification(''), 2000);
      return;
    }

    // Add to scanned
    setScannedTags((prev) => new Set([...prev, tag]));
    setScannedAssets((prev) => [...prev, asset]);

    // Haptic feedback
    try {
      navigator.vibrate([200]);
    } catch (e) {
      // Vibration not supported
    }

    // Show notification
    setNotification(`Scanned: ${asset.beskrivelse}`);
    setTimeout(() => setNotification(''), 2000);
  };

  const beepError = () => {
    try {
      const audioContext = new (window.AudioContext || (window as any).webkitAudioContext)();
      const oscillator = audioContext.createOscillator();
      const gain = audioContext.createGain();
      oscillator.connect(gain);
      gain.connect(audioContext.destination);
      oscillator.frequency.value = 800;
      oscillator.type = 'sine';
      gain.gain.setValueAtTime(0.3, audioContext.currentTime);
      gain.gain.exponentialRampToValueAtTime(0.01, audioContext.currentTime + 0.1);
      oscillator.start(audioContext.currentTime);
      oscillator.stop(audioContext.currentTime + 0.1);
    } catch (e) {
      // Audio not supported
    }
  };

  const removeScannedAsset = (tag: string) => {
    setScannedTags((prev) => {
      const newSet = new Set(prev);
      newSet.delete(tag);
      return newSet;
    });
    setScannedAssets((prev) => prev.filter((a) => a.tag !== tag));
  };

  // Get next item to display photo
  const nextUnscannedAsset = scannedAssets.length > 0 ? scannedAssets[scannedAssets.length - 1] : null;

  return (
    <div style={{ padding: '16px', maxWidth: '600px', margin: '0 auto' }}>
      <h2>Pick List</h2>

      {notification && (
        <div
          style={{
            padding: '12px',
            marginBottom: '16px',
            backgroundColor: '#e8f5e9',
            border: '1px solid #4caf50',
            borderRadius: '4px',
            color: '#2e7d32',
          }}
        >
          {notification}
        </div>
      )}

      <div
        style={{
          display: 'flex',
          gap: '8px',
          borderBottom: '1px solid #ddd',
          marginBottom: '16px',
        }}
      >
        <button
          onClick={() => setActiveTab('paste')}
          style={{
            border: 'none',
            padding: '8px 16px',
            backgroundColor: activeTab === 'paste' ? '#1976d2' : '#f0f0f0',
            color: activeTab === 'paste' ? 'white' : 'black',
            cursor: 'pointer',
          }}
        >
          Paste Tags
        </button>
        <button
          onClick={() => setActiveTab('qr')}
          style={{
            border: 'none',
            padding: '8px 16px',
            backgroundColor: activeTab === 'qr' ? '#1976d2' : '#f0f0f0',
            color: activeTab === 'qr' ? 'white' : 'black',
            cursor: 'pointer',
          }}
        >
          Scan QR
        </button>
      </div>

      {activeTab === 'paste' && (
        <div style={{ marginBottom: '24px' }}>
          <label>
            Enter tags (comma or newline separated):
            <textarea
              value={pasteValue}
              onChange={(e) => setPasteValue(e.target.value)}
              placeholder="S-001, K-003, H-015"
              style={{
                width: '100%',
                height: '100px',
                padding: '8px',
                marginTop: '8px',
                fontFamily: 'monospace',
              }}
            />
          </label>
          <button onClick={handlePaste} style={{ marginTop: '8px', width: '100%' }}>
            Add to Scan List
          </button>
        </div>
      )}

      {activeTab === 'qr' && (
        <div style={{ marginBottom: '24px' }}>
          {nextUnscannedAsset && (
            <div style={{ marginBottom: '16px' }}>
              <p style={{ margin: '0 0 8px 0', fontSize: '0.9em', color: '#333' }}>
                Next to scan:
              </p>
              <AssetCard asset={nextUnscannedAsset} showStatus={false} />
            </div>
          )}
          <QrScanner onScan={handleQrScan} qrbox={200} />
        </div>
      )}

      <div style={{ marginTop: '24px' }}>
        <h3>Items ({scannedAssets.length})</h3>
        {scannedAssets.length > 0 ? (
          <>
            {scannedAssets.map((asset) => (
              <div key={asset.id} style={{ position: 'relative' }}>
                <AssetCard asset={asset} showStatus={false} />
                <button
                  onClick={() => removeScannedAsset(asset.tag)}
                  style={{
                    position: 'absolute',
                    top: '8px',
                    right: '8px',
                    padding: '4px 8px',
                    fontSize: '0.8em',
                  }}
                >
                  Remove
                </button>
              </div>
            ))}
          </>
        ) : (
          <p style={{ color: '#333' }}>No items scanned yet.</p>
        )}
      </div>

      <div
        style={{
          display: 'flex',
          gap: '12px',
          marginTop: '24px',
          justifyContent: 'space-between',
        }}
      >
        <button onClick={() => navigate('/')}>Back</button>
        <button
          onClick={() => navigate('/review', { state: { scannedAssets } })}
          disabled={scannedAssets.length === 0}
        >
          Review & Complete
        </button>
      </div>
    </div>
  );
}
