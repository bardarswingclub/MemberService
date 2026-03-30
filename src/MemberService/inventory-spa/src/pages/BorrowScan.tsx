import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { QrScanner } from '../components/QrScanner';
import { AssetCard } from '../components/AssetCard';
import { InventoryAsset } from '../types/inventory';
import { MOCK_ASSETS } from '../data/mockAssets';

export function BorrowScan() {
  const navigate = useNavigate();
  const [scannedTags, setScannedTags] = useState<Set<string>>(new Set());
  const [scannedAssets, setScannedAssets] = useState<InventoryAsset[]>([]);
  const [lastScannedTag, setLastScannedTag] = useState<string>('');
  const [notification, setNotification] = useState<string>('');

  const handleScan = (decodedText: string) => {
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
    setLastScannedTag(tag);

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

  return (
    <div style={{ padding: '16px', maxWidth: '600px', margin: '0 auto' }}>
      <h2>Free Scan Mode</h2>
      <p>Point your camera at a QR code to scan items.</p>

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

      <QrScanner onScan={handleScan} qrbox={200} />

      <div style={{ marginTop: '24px' }}>
        <h3>Scanned Items ({scannedAssets.length})</h3>
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
