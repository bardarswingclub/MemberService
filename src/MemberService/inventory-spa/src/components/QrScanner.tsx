import { useEffect, useRef, useCallback } from 'react';
import { Html5QrcodeScanner } from 'html5-qrcode';

interface QrScannerProps {
  onScan: (decodedText: string) => void;
  onError?: (error: string) => void;
  fps?: number;
  qrbox?: number;
}

export function QrScanner({ onScan, onError, fps = 10, qrbox = 250 }: QrScannerProps) {
  const scannerRef = useRef<Html5QrcodeScanner | null>(null);
  const isInitialized = useRef(false);
  const onScanRef = useRef(onScan);
  const onErrorRef = useRef(onError);

  // Keep refs current on every render so the scanner always calls the latest callbacks
  onScanRef.current = onScan;
  onErrorRef.current = onError;

  const handleScan = useCallback((decodedText: string) => {
    onScanRef.current(decodedText);
  }, []);

  const handleError = useCallback((error: string) => {
    if (!error.includes('NotFoundError') && !error.includes('NotFoundException')) {
      onErrorRef.current?.(error);
    }
  }, []);

  useEffect(() => {
    // Only initialize once
    if (isInitialized.current) {
      return;
    }

    try {
      const scanner = new Html5QrcodeScanner(
        'qr-reader',
        {
          fps: fps,
          qrbox: qrbox,
        },
        false
      );

      scannerRef.current = scanner;
      isInitialized.current = true;

      scanner.render(handleScan, handleError);
    } catch (error) {
      console.error('Error initializing scanner:', error);
    }

    return () => {
      // Don't clear on unmount - keep scanner ready
      // Only clear if component is truly being destroyed
      if (scannerRef.current && !document.getElementById('qr-reader')) {
        scannerRef.current.clear().catch((error: string) => {
          console.error('Failed to clear scanner:', error);
        });
      }
    };
  }, []);

  return (
    <div
      id="qr-reader"
      style={{
        width: '100%',
        minHeight: '300px',
      }}
    ></div>
  );
}
