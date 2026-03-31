import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { assetsApi, CsvImportResult } from '../api/assets';

export function CsvImport() {
  const navigate = useNavigate();
  const [content, setContent] = useState('');
  const [loading, setLoading] = useState(false);
  const [result, setResult] = useState<CsvImportResult | null>(null);
  const [error, setError] = useState('');

  const handleFileChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    const text = await file.text();
    setContent(text);
    setResult(null);
  };

  const handleImport = async () => {
    if (!content.trim()) return;
    setLoading(true);
    setError('');
    setResult(null);
    try {
      const res = await assetsApi.importCsv(content);
      setResult(res);
    } catch (e: unknown) {
      if (e instanceof Error) setError(e.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{ padding: '16px', maxWidth: '600px', margin: '0 auto' }}>
      <button onClick={() => navigate('/')} style={{ background: 'none', border: 'none', fontSize: '14px', color: '#1976d2', cursor: 'pointer', padding: 0, marginBottom: '16px' }}>
        ← Hjem
      </button>
      <h2 style={{ marginBottom: '20px' }}>Importer CSV</h2>

      <div style={{ marginBottom: '16px' }}>
        <label style={{ display: 'block', fontWeight: 'bold', marginBottom: '8px' }}>Last opp CSV-fil</label>
        <input
          type="file"
          accept=".csv,text/csv"
          onChange={handleFileChange}
          style={{ display: 'block', marginBottom: '8px' }}
        />
      </div>

      <div style={{ marginBottom: '16px' }}>
        <label style={{ display: 'block', fontWeight: 'bold', marginBottom: '8px' }}>Eller lim inn CSV-innhold</label>
        <textarea
          value={content}
          onChange={e => { setContent(e.target.value); setResult(null); }}
          placeholder="No,Tag,Inventory,Lokasjon,Kategori,Sub-kategori,Beskrivelse,..."
          rows={8}
          style={{ width: '100%', padding: '10px', fontSize: '13px', fontFamily: 'monospace', borderRadius: '6px', border: '1px solid #ccc', boxSizing: 'border-box', resize: 'vertical' }}
        />
      </div>

      {error && <div style={{ color: '#c62828', padding: '10px', backgroundColor: '#ffebee', borderRadius: '6px', marginBottom: '12px' }}>{error}</div>}

      <button
        onClick={handleImport}
        disabled={loading || !content.trim()}
        style={{
          width: '100%',
          padding: '14px',
          fontSize: '16px',
          fontWeight: 'bold',
          borderRadius: '8px',
          border: 'none',
          backgroundColor: loading || !content.trim() ? '#ccc' : '#f57c00',
          color: '#fff',
          cursor: loading || !content.trim() ? 'not-allowed' : 'pointer',
          marginBottom: '20px',
        }}
      >
        {loading ? 'Importerer...' : 'Importer'}
      </button>

      {result && (
        <div style={{ padding: '16px', backgroundColor: result.errorCount === 0 ? '#e8f5e9' : '#fff8e1', borderRadius: '8px' }}>
          <div style={{ fontWeight: 'bold', fontSize: '16px', marginBottom: '8px', color: result.errorCount === 0 ? '#2e7d32' : '#f57c00' }}>
            {result.successCount} rader importert{result.errorCount > 0 ? `, ${result.errorCount} feil` : ''}
          </div>
          {result.errors.map(e => (
            <div key={e.rowNumber} style={{ padding: '6px 10px', backgroundColor: '#ffebee', borderRadius: '4px', marginBottom: '4px', fontSize: '13px', color: '#c62828' }}>
              Rad {e.rowNumber}: {e.message}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
