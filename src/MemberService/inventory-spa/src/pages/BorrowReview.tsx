import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { borrowsApi, BorrowSession } from '../api/borrows';

const TYPE_LABELS: Record<string, string> = {
  Borrow: 'Lån ut',
  Return: 'Returner',
  InventoryCheck: 'Tell lager',
};

export function BorrowReview() {
  const { sessionId } = useParams<{ sessionId: string }>();
  const navigate = useNavigate();

  const [session, setSession] = useState<BorrowSession | null>(null);
  const [error, setError] = useState('');
  const [completing, setCompleting] = useState(false);
  const [completed, setCompleted] = useState(false);

  useEffect(() => {
    if (!sessionId) return;
    borrowsApi.get(sessionId).then(s => {
      setSession(s);
      setCompleted(!!s.completedAt);
    }).catch((e: Error) => setError(e.message));
  }, [sessionId]);

  const handleComplete = async () => {
    if (!sessionId) return;
    setCompleting(true);
    try {
      const updated = await borrowsApi.complete(sessionId);
      setSession(updated);
      setCompleted(true);
    } catch (e: unknown) {
      if (e instanceof Error) setError(e.message);
      setCompleting(false);
    }
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
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '16px' }}>
        {!completed && (
          <button onClick={() => navigate(`/borrow/${sessionId}/scan`)} style={{ background: 'none', border: 'none', fontSize: '14px', color: '#1976d2', cursor: 'pointer', padding: 0 }}>
            ← Tilbake til skanning
          </button>
        )}
        {completed && (
          <button onClick={() => navigate('/')} style={{ background: 'none', border: 'none', fontSize: '14px', color: '#1976d2', cursor: 'pointer', padding: 0 }}>
            ← Hjem
          </button>
        )}
      </div>

      {completed ? (
        <div style={{ padding: '20px', backgroundColor: '#e8f5e9', borderRadius: '8px', textAlign: 'center', marginBottom: '20px' }}>
          <div style={{ fontSize: '48px', marginBottom: '8px' }}>✓</div>
          <div style={{ fontSize: '20px', fontWeight: 'bold', color: '#2e7d32' }}>Fullført!</div>
          <div style={{ color: '#388e3c', marginTop: '4px' }}>{session.items.length} gjenstander {TYPE_LABELS[session.type]?.toLowerCase()}</div>
        </div>
      ) : (
        <h2 style={{ marginBottom: '16px', color: '#1a1a1a' }}>Gjennomgang</h2>
      )}

      <div style={{ backgroundColor: '#f5f5f5', padding: '10px 14px', borderRadius: '6px', marginBottom: '16px' }}>
        <strong>{session.eventName}</strong> · {TYPE_LABELS[session.type] || session.type}
        <br />
        <small style={{ color: '#555' }}>{session.items.length} gjenstander</small>
      </div>

      {error && (
        <div style={{ padding: '10px 12px', backgroundColor: '#ffebee', color: '#c62828', borderRadius: '6px', marginBottom: '12px' }}>
          {error}
        </div>
      )}

      {session.items.map(item => (
        <div key={item.id} style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', padding: '10px 12px', backgroundColor: '#fff', border: '1px solid #e0e0e0', borderRadius: '6px', marginBottom: '6px' }}>
          <div>
            <code style={{ fontWeight: 'bold', color: '#1a1a1a', marginRight: '8px' }}>{item.tag}</code>
            <span style={{ color: '#333', fontSize: '14px' }}>{item.beskrivelse}</span>
          </div>
          {!completed && (
            <button
              onClick={() => handleRemoveItem(item.id)}
              style={{ background: 'none', border: 'none', color: '#c62828', cursor: 'pointer', fontSize: '20px', lineHeight: 1, padding: '0 4px' }}
            >
              ×
            </button>
          )}
        </div>
      ))}

      {session.items.length === 0 && (
        <div style={{ color: '#666', fontStyle: 'italic', padding: '20px', textAlign: 'center', border: '1px dashed #ccc', borderRadius: '8px', marginBottom: '16px' }}>
          Ingen gjenstander
        </div>
      )}

      {!completed && (
        <button
          onClick={handleComplete}
          disabled={completing || session.items.length === 0}
          style={{
            width: '100%',
            padding: '16px',
            marginTop: '16px',
            fontSize: '18px',
            fontWeight: 'bold',
            borderRadius: '8px',
            border: 'none',
            backgroundColor: completing || session.items.length === 0 ? '#e0e0e0' : '#388e3c',
            color: completing || session.items.length === 0 ? '#888' : '#fff',
            cursor: completing || session.items.length === 0 ? 'not-allowed' : 'pointer',
          }}
        >
          {completing ? 'Fullfører...' : `Fullfør ${TYPE_LABELS[session.type]?.toLowerCase() || ''}`}
        </button>
      )}
    </div>
  );
}
