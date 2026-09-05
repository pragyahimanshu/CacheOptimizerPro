import { Routes, Route } from 'react-router-dom';
import PageLayout from '../components/layout/PageLayout';
import DashboardPage from '../pages/DashboardPage';
import ProductsPage from '../pages/ProductsPage';
import SettingsPage from '../pages/SettingsPage';
import NotFoundPage from '../pages/NotFoundPage';

export default function AppRoutes() { return <Routes><Route element={<PageLayout />}><Route path="/" element={<DashboardPage />} /><Route path="/products" element={<ProductsPage />} /><Route path="/settings" element={<SettingsPage />} /><Route path="*" element={<NotFoundPage />} /></Route></Routes>; }
