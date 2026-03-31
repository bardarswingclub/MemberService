import { useNavigate } from 'react-router-dom';
import { usePermissions } from '../App';

const btn: React.CSSProperties = {
  display: 'block',
  width: '100%',
  padding: '16px',
  marginBottom: '12px',
  fontSize: '18px',
  fontWeight: 'bold',
  borderRadius: '8px',
  border: 'none',
  cursor: 'pointer',
  color: '#fff',
};

export function Home() {
  const navigate = useNavigate();
  const { canManage } = usePermissions();

  return (
    <div style={{ padding: '24px', maxWidth: '500px', margin: '0 auto' }}>
      <h1 style={{ textAlign: 'center', marginBottom: '32px', color: '#1a1a1a' }}>Lager</h1>

      <button onClick={() => navigate('/borrow/start')} style={{ ...btn, backgroundColor: '#1976d2' }}>
        Lån / Retur / Tell
      </button>

      <button onClick={() => navigate('/pick-list')} style={{ ...btn, backgroundColor: '#00838f' }}>
        Plukkliste
      </button>

      <button onClick={() => navigate('/assets')} style={{ ...btn, backgroundColor: '#388e3c' }}>
        Utstyrsliste
      </button>

      {canManage && (
        <button onClick={() => navigate('/assets/manage')} style={{ ...btn, backgroundColor: '#6a1b9a' }}>
          Rediger utstyr
        </button>
      )}

      {canManage && (
        <button onClick={() => navigate('/import')} style={{ ...btn, backgroundColor: '#f57c00' }}>
          Importer CSV
        </button>
      )}

      <a href="/" style={{ ...btn, backgroundColor: '#546e7a', textAlign: 'center', textDecoration: 'none', display: 'block', marginTop: '24px', boxSizing: 'border-box' }}>
        Tilbake til BSC
      </a>
    </div>
  );
}
