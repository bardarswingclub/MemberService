import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { borrowsApi, BorrowSession } from '../api/borrows';

const EVENT_NAMES = [
  'Winter Jump',
  'Juni Jazz',
  'Shag up',
  'Sosialdanskomiteen',
  "O'Slow",
  'Oslo Balboa Weekend',
  'Just me',
];

const TYPES: { value: BorrowSession['type']; label: string; color: string }[] = [
  { value: 'Borrow', label: 'Lån ut', color: '#1976d2' },
  { value: 'Return', label: 'Returner', color: '#388e3c' },
];

export function BorrowStart() {
  const navigate = useNavigate();
  const [selectedEvent, setSelectedEvent] = useState('');
  const [customEvent, setCustomEvent] = useState('');
  const [selectedType, setSelectedType] = useState<BorrowSession['type']>('Borrow');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const eventName = selectedEvent === '__custom' ? customEvent : selectedEvent;

  const handleStart = async () => {
    if (!eventName.trim()) {
      setError('Velg eller skriv inn et arrangement');
      return;
    }
    setLoading(true);
    setError('');
    try {
      const session = await borrowsApi.start(eventName.trim(), selectedType);
      navigate(`/borrow/${session.id}/scan`);
    } catch (e: any) {
      setError(e.message || 'Noe gikk galt');
      setLoading(false);
    }
  };

  return (
    <div style={{ padding: '24px', maxWidth: '500px', margin: '0 auto' }}>
      <button onClick={() => navigate('/')} style={{ background: 'none', border: 'none', fontSize: '14px', color: '#1976d2', cursor: 'pointer', padding: 0, marginBottom: '16px' }}>
        ← Tilbake
      </button>
      <h2 style={{ marginBottom: '24px', color: '#1a1a1a' }}>Start session</h2>

      <div style={{ marginBottom: '24px' }}>
        <div style={{ fontWeight: 'bold', marginBottom: '8px' }}>Arrangement</div>
        {EVENT_NAMES.map(name => (
          <button
            key={name}
            onClick={() => setSelectedEvent(name)}
            style={{
              display: 'inline-block',
              margin: '4px',
              padding: '8px 14px',
              borderRadius: '20px',
              border: '2px solid',
              borderColor: selectedEvent === name ? '#1976d2' : '#ccc',
              backgroundColor: selectedEvent === name ? '#e3f2fd' : '#fff',
              color: '#1a1a1a',
              cursor: 'pointer',
              fontSize: '14px',
            }}
          >
            {name}
          </button>
        ))}
        <button
          onClick={() => setSelectedEvent('__custom')}
          style={{
            display: 'inline-block',
            margin: '4px',
            padding: '8px 14px',
            borderRadius: '20px',
            border: '2px solid',
            borderColor: selectedEvent === '__custom' ? '#1976d2' : '#ccc',
            backgroundColor: selectedEvent === '__custom' ? '#e3f2fd' : '#fff',
            color: '#1a1a1a',
            cursor: 'pointer',
            fontSize: '14px',
          }}
        >
          Annet...
        </button>
        {selectedEvent === '__custom' && (
          <input
            type="text"
            placeholder="Skriv inn arrangementsnavnet"
            value={customEvent}
            onChange={e => setCustomEvent(e.target.value)}
            style={{ display: 'block', width: '100%', marginTop: '8px', padding: '10px', fontSize: '16px', borderRadius: '6px', border: '1px solid #ccc', boxSizing: 'border-box' }}
            autoFocus
          />
        )}
      </div>

      <div style={{ marginBottom: '24px' }}>
        <div style={{ fontWeight: 'bold', marginBottom: '8px' }}>Type</div>
        {TYPES.map(t => (
          <button
            key={t.value}
            onClick={() => setSelectedType(t.value)}
            style={{
              display: 'inline-block',
              margin: '4px',
              padding: '10px 20px',
              borderRadius: '6px',
              border: '2px solid',
              borderColor: selectedType === t.value ? t.color : '#ccc',
              backgroundColor: selectedType === t.value ? t.color : '#fff',
              color: selectedType === t.value ? '#fff' : '#1a1a1a',
              cursor: 'pointer',
              fontWeight: 'bold',
              fontSize: '15px',
            }}
          >
            {t.label}
          </button>
        ))}
      </div>

      {error && <div style={{ color: '#c62828', marginBottom: '16px', padding: '10px', backgroundColor: '#ffebee', borderRadius: '6px' }}>{error}</div>}

      <button
        onClick={handleStart}
        disabled={loading || !eventName.trim()}
        style={{
          width: '100%',
          padding: '16px',
          fontSize: '18px',
          fontWeight: 'bold',
          borderRadius: '8px',
          border: 'none',
          backgroundColor: loading || !eventName.trim() ? '#e0e0e0' : '#1976d2',
          color: loading || !eventName.trim() ? '#888' : '#fff',
          cursor: loading || !eventName.trim() ? 'not-allowed' : 'pointer',
        }}
      >
        {loading ? 'Starter...' : 'Start'}
      </button>
    </div>
  );
}
