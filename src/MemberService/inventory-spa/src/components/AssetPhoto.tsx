import { useState } from 'react';

interface Props {
  url: string;
  tag: string;
  style?: React.CSSProperties;
  onLoadError?: () => void;
}

export function AssetPhoto({ url, tag, style, onLoadError }: Props) {
  const [open, setOpen] = useState(false);

  return (
    <>
      <img
        src={url}
        alt={tag}
        style={{ cursor: 'zoom-in', ...style }}
        onClick={() => setOpen(true)}
        onError={onLoadError}
      />
      {open && (
        <div
          onClick={() => setOpen(false)}
          style={{
            position: 'fixed', inset: 0,
            backgroundColor: 'rgba(0,0,0,0.88)',
            display: 'flex', alignItems: 'center', justifyContent: 'center',
            zIndex: 1000, cursor: 'zoom-out',
          }}
        >
          <img
            src={url}
            alt={tag}
            style={{ maxWidth: '92vw', maxHeight: '92vh', objectFit: 'contain', borderRadius: '8px' }}
          />
        </div>
      )}
    </>
  );
}
