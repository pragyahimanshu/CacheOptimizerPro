import { useLocation } from 'react-router-dom';

const titles = { '/': ['Operations overview', 'Real-time cache intelligence'], '/products': ['Product catalog', 'Manage the data your cache protects'], '/settings': ['Cache settings', 'Tune runtime behavior without redeploying'] };

export default function Header() {
  const { pathname } = useLocation();
  const [title, subtitle] = titles[pathname] || ['Not found', ''];
  return <header className="header"><div><p className="eyebrow">CACHE OPTIMIZER PRO / 05 SEP 2026</p><h1>{title}</h1><p className="subtitle">{subtitle}</p></div><div className="avatar">PH</div></header>;
}
