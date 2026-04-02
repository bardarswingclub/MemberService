import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { borrowsApi } from '../api/borrows';

export function InventoryCheckStart() {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const eventName = `Lagertelling ${new Date().toLocaleDateString('nb-NO', { day: 'numeric', month: 'long', year: 'numeric' })}`;

  const handleStart = async () => {
    setLoading(true);
    setError('');
    try {
      const session = await borrowsApi.start(eventName, 'InventoryCheck');
      navigate(`/borrow/${session.id}/scan`);
    } catch (e: any) {
      setError(e.message || 'Noe gikk galt');
      setLoading(false);
    }
  };

  return (
    <div style={{ padding: '24px', maxWidth: '480px', margin: '0 auto' }}>
      <button onClick={() => navigate('/')} style={{ background: 'none', border: 'none', fontSize: '14px', color: '#1976d2', cursor: 'pointer', padding: 0, marginBottom: '20px' }}>
        ← Tilbake
      </button>
      <h2 style={{ marginBottom: '8px', color: '#1a1a1a' }}>Tell lager</h2>
      <div style={{ color: '#555', fontSize: '15px', marginBottom: '32px' }}>
        Scan alle gjenstander du finner. Hvert scan registrerer tidspunkt for observasjon.
      </div>

      <div style={{ backgroundColor: '#f3e5f5', border: '1px solid #ce93d8', borderRadius: '8px', padding: '12px 16px', marginBottom: '32px', fontSize: '14px', color: '#4a148c' }}>
        Sesjonsnavn: <strong>{eventName}</strong>
      </div>

      {error && (
        <div style={{ color: '#c62828', padding: '10px', backgroundColor: '#ffebee', borderRadius: '6px', marginBottom: '16px' }}>{error}</div>
      )}

      <button
        onClick={handleStart}
        disabled={loading}
        style={{ width: '100%', padding: '16px', fontSize: '18px', fontWeight: 'bold', borderRadius: '8px', border: 'none', backgroundColor: loading ? '#e0e0e0' : '#7b1fa2', color: loading ? '#888' : '#fff', cursor: loading ? 'not-allowed' : 'pointer', boxSizing: 'border-box' }}
      >
        {loading ? 'Starter...' : 'Start lagertelling'}
      </button>
    </div>
  );
}
