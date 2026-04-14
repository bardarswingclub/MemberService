import { InventoryAsset } from '../types/inventory';

interface AssetCardProps {
  asset: InventoryAsset;
  showStatus?: boolean;
}

export function AssetCard({ asset, showStatus = true }: AssetCardProps) {
  const isAvailable = !asset.currentBorrowId;

  return (
    <div
      style={{
        border: '1px solid #ccc',
        borderRadius: '8px',
        padding: '12px',
        marginBottom: '12px',
        backgroundColor: '#f5f5f5',
      }}
    >
      {asset.photoUrl && (
        <img
          src={asset.photoUrl}
          alt={asset.beskrivelse}
          style={{
            width: '100%',
            height: '100px',
            objectFit: 'cover',
            borderRadius: '4px',
            marginBottom: '8px',
          }}
        />
      )}

      <div style={{ marginBottom: '8px' }}>
        <strong style={{ fontSize: '1.1em' }}>{asset.tag}</strong>
        {showStatus && (
          <div
            style={{
              display: 'inline-block',
              marginLeft: '8px',
              padding: '2px 8px',
              borderRadius: '4px',
              fontSize: '0.85em',
              backgroundColor: isAvailable ? '#4caf50' : '#ff9800',
              color: 'white',
            }}
          >
            {isAvailable ? 'Available' : 'Borrowed'}
          </div>
        )}
      </div>

      <p style={{ margin: '0 0 4px 0', fontSize: '0.95em' }}>
        <strong>{asset.beskrivelse}</strong>
      </p>

      {asset.kategori && (
        <p style={{ margin: '0 0 4px 0', fontSize: '0.85em', color: '#333' }}>
          Category: {asset.kategori}
          {asset.subKategori && ` / ${asset.subKategori}`}
        </p>
      )}

      {(asset.merke || asset.modell) && (
        <p style={{ margin: '0 0 4px 0', fontSize: '0.85em', color: '#333' }}>
          {asset.merke} {asset.modell}
        </p>
      )}

      {asset.detaljer && (
        <p style={{ margin: '0', fontSize: '0.85em', color: '#333' }}>
          {asset.detaljer}
        </p>
      )}
    </div>
  );
}
