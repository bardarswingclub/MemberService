import { useEffect, useRef, useCallback, useState } from 'react';
import { Html5Qrcode, CameraDevice } from 'html5-qrcode';

interface QrScannerProps {
  onScan: (decodedText: string) => void;
  onError?: (error: string) => void;
  fps?: number;
  qrbox?: number;
}

export function QrScanner({ onScan, fps = 10, qrbox = 250 }: QrScannerProps) {
  const scannerRef = useRef<Html5Qrcode | null>(null);
  const runningRef = useRef(false);
  const onScanRef = useRef(onScan);
  const [cameras, setCameras] = useState<CameraDevice[]>([]);
  const [selectedCamera, setSelectedCamera] = useState<string>('');
  const [error, setError] = useState('');

  onScanRef.current = onScan;

  const handleScan = useCallback((text: string) => {
    onScanRef.current(text);
  }, []);

  // List cameras on mount
  useEffect(() => {
    Html5Qrcode.getCameras()
      .then(devices => {
        if (devices.length === 0) { setError('Ingen kamera funnet'); return; }
        setCameras(devices);
        // Prefer back/environment camera
        const back = devices.find(d =>
          /back|rear|environment/i.test(d.label)
        );
        setSelectedCamera((back ?? devices[0]).id);
      })
      .catch(() => setError('Kameratilgang nektet'));
  }, []);

  // Start/restart scanner when selectedCamera changes
  useEffect(() => {
    if (!selectedCamera) return;

    const scanner = new Html5Qrcode('qr-reader-video');
    scannerRef.current = scanner;

    scanner.start(
      selectedCamera,
      { fps, qrbox: { width: qrbox, height: qrbox } },
      handleScan,
      () => {} // ignore per-frame errors
    ).then(() => {
      runningRef.current = true;
    }).catch(err => {
      setError(`Kunne ikke starte kamera: ${err}`);
    });

    return () => {
      if (runningRef.current) {
        scanner.stop().catch(() => {}).finally(() => {
          runningRef.current = false;
        });
      }
    };
  }, [selectedCamera]);

  if (error) {
    return (
      <div style={{ padding: '16px', backgroundColor: '#ffebee', color: '#c62828', borderRadius: '8px', textAlign: 'center' }}>
        {error}
      </div>
    );
  }

  return (
    <div style={{ borderRadius: '8px', overflow: 'hidden', border: '1px solid #e0e0e0' }}>
      {cameras.length > 1 && (
        <div style={{ padding: '8px 10px', backgroundColor: '#1a237e', display: 'flex', alignItems: 'center', gap: '8px' }}>
          <span style={{ color: '#90caf9', fontSize: '13px', whiteSpace: 'nowrap', flexShrink: 0 }}>Bytt kamera</span>
          <select
            value={selectedCamera}
            onChange={e => setSelectedCamera(e.target.value)}
            style={{ flex: 1, padding: '6px 10px', fontSize: '14px', borderRadius: '6px', border: '2px solid #90caf9', backgroundColor: '#283593', color: '#fff', appearance: 'auto', cursor: 'pointer' }}
          >
            {cameras.map(c => (
              <option key={c.id} value={c.id} style={{ backgroundColor: '#283593', color: '#fff' }}>
                {c.label || `Kamera ${c.id}`}
              </option>
            ))}
          </select>
        </div>
      )}
      <div id="qr-reader-video" style={{ width: '100%' }} />
    </div>
  );
}
