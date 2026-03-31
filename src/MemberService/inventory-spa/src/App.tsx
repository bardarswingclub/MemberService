import { createContext, useContext } from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { BorrowStart } from './pages/BorrowStart';
import { BorrowScan } from './pages/BorrowScan';
import { BorrowReview } from './pages/BorrowReview';
import { AssetList } from './pages/AssetList';
import { AssetManageList } from './pages/AssetManageList';
import { AssetEditForm } from './pages/AssetEditForm';
import { CsvImport } from './pages/CsvImport';
import { Home } from './pages/Home';

export const PermissionsContext = createContext({ canManage: false });
export const usePermissions = () => useContext(PermissionsContext);

function TopBar() {
  return (
    <div style={{ backgroundColor: '#1a237e', padding: '8px 16px', display: 'flex', alignItems: 'center', gap: '16px' }}>
      <a href="/" style={{ color: '#90caf9', fontSize: '13px', textDecoration: 'none' }}>← BSC</a>
      <span style={{ color: '#fff', fontWeight: 'bold', fontSize: '15px' }}>Lager</span>
    </div>
  );
}

export default function App({ canManage }: { canManage: boolean }) {
  const basename = import.meta.env.PROD ? '/Inventory' : '/';

  return (
    <PermissionsContext.Provider value={{ canManage }}>
      <Router basename={basename}>
        <div style={{ minHeight: '100vh', backgroundColor: '#fafafa', fontFamily: 'sans-serif' }}>
          <TopBar />
          <Routes>
            <Route path="/" element={<Home />} />
            <Route path="/borrow/start" element={<BorrowStart />} />
            <Route path="/borrow/:sessionId/scan" element={<BorrowScan />} />
            <Route path="/borrow/:sessionId/review" element={<BorrowReview />} />
            <Route path="/assets" element={<AssetList />} />
            <Route path="/assets/manage" element={<AssetManageList />} />
            <Route path="/assets/:tag/edit" element={<AssetEditForm />} />
            <Route path="/import" element={<CsvImport />} />
          </Routes>
        </div>
      </Router>
    </PermissionsContext.Provider>
  );
}
