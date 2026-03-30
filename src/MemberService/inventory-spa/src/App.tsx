import { BrowserRouter as Router, Routes, Route, useNavigate } from 'react-router-dom';
import { BorrowScan } from './pages/BorrowScan';
import { BorrowPickList } from './pages/BorrowPickList';
import { MOCK_ASSETS } from './data/mockAssets';

function Home() {
  const navigate = useNavigate();

  return (
    <div style={{ padding: '16px', maxWidth: '600px', margin: '0 auto' }}>
      <h1>Inventory POC</h1>
      <p>This is a proof of concept for the inventory QR scanning system.</p>

      <div style={{ marginBottom: '24px' }}>
        <h2>Test Scenarios</h2>
        <button
          onClick={() => navigate('/borrow-scan')}
          style={{ display: 'block', marginBottom: '8px', width: '100%' }}
        >
          Free Scan Mode
        </button>
        <button
          onClick={() => navigate('/pick-list')}
          style={{ display: 'block', width: '100%' }}
        >
          Pick List (Paste + QR)
        </button>
      </div>

      <div style={{ marginBottom: '24px' }}>
        <h2>Available Test Tags</h2>
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(2, 1fr)', gap: '8px' }}>
          {MOCK_ASSETS.map((asset) => (
            <div
              key={asset.id}
              style={{
                padding: '8px',
                border: '1px solid #ddd',
                borderRadius: '4px',
                backgroundColor: '#f5f5f5',
              }}
            >
              <code style={{ fontWeight: 'bold', color: '#000' }}>{asset.tag}</code>
              <br />
              <small style={{ color: '#333' }}>{asset.beskrivelse}</small>
            </div>
          ))}
        </div>
        <p style={{ fontSize: '0.85em', color: '#333', marginTop: '12px' }}>
          To test QR scanning, you can generate QR codes for these tags using an online QR code
          generator and point your camera at them.
        </p>
      </div>

      <div style={{ marginBottom: '24px', padding: '12px', backgroundColor: '#e3f2fd', borderRadius: '4px', color: '#1565c0' }}>
        <h3 style={{ marginTop: 0, color: '#0d47a1' }}>Testing Tips</h3>
        <ul style={{ margin: 0, color: '#1565c0' }}>
          <li>Test on actual iOS and Android devices</li>
          <li>Use <code style={{ backgroundColor: '#fff', padding: '2px 4px', borderRadius: '2px', color: '#000' }}>npm run dev</code> for development</li>
          <li>For mobile testing, use <code style={{ backgroundColor: '#fff', padding: '2px 4px', borderRadius: '2px', color: '#000' }}>ngrok</code> or similar tunnel to expose localhost</li>
          <li>Camera requires HTTPS (or localhost)</li>
          <li>Vibration feedback works on most mobile devices</li>
        </ul>
      </div>
    </div>
  );
}

export default function App() {
  const basename = import.meta.env.PROD ? '/dist/inventory' : '/';

  return (
    <Router basename={basename}>
      <div style={{ minHeight: '100vh', backgroundColor: '#fafafa' }}>
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/borrow-scan" element={<BorrowScan />} />
          <Route path="/pick-list" element={<BorrowPickList />} />
          <Route path="/review" element={<ReviewPage />} />
        </Routes>
      </div>
    </Router>
  );
}

function ReviewPage() {
  const navigate = useNavigate();

  return (
    <div style={{ padding: '16px', maxWidth: '600px', margin: '0 auto' }}>
      <h2>Review & Complete</h2>
      <p style={{ color: '#666' }}>Session summary would be shown here.</p>
      <button onClick={() => navigate('/')}>Back to Home</button>
    </div>
  );
}
