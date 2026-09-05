import { NavLink } from 'react-router-dom';

const links = [
  { to: '/', label: 'Overview', icon: '◌' },
  { to: '/products', label: 'Products', icon: '▦' },
  { to: '/settings', label: 'Cache settings', icon: '⌘' },
];

export default function Sidebar() {
  return <aside className="sidebar"><div className="brand"><span className="brand-mark">CO</span><span>Cache<br /><b>Optimizer</b></span></div><nav>{links.map((link) => <NavLink key={link.to} to={link.to} end={link.to === '/'}><span>{link.icon}</span>{link.label}</NavLink>)}</nav><div className="sidebar-foot"><span className="status-dot" /> API connected<br /><small>v1.0.0 / SQL Server</small></div></aside>;
}
