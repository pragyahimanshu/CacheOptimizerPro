import { Outlet } from 'react-router-dom';
import Sidebar from './Sidebar';
import Header from './Header';

export default function PageLayout() {
  return <div className="app-shell"><Sidebar /><main className="main"><Header /><Outlet /></main></div>;
}
