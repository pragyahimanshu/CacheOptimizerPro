import { useEffect, useState } from 'react';
import { productsApi } from '../api/products.api';
import Button from '../components/common/Button';
import Card from '../components/common/Card';
import Modal from '../components/common/Modal';
import ErrorMessage from '../components/common/ErrorMessage';
import Loader from '../components/common/Loader';
import ProductForm from '../features/products/ProductForm';
import ProductList from '../features/products/ProductList';

export default function ProductsPage() {
  const [products, setProducts] = useState([]); const [loading, setLoading] = useState(true); const [error, setError] = useState(null); const [modal, setModal] = useState(null);
  const load = () => { setLoading(true); productsApi.list().then(({ data }) => setProducts(data.items ?? data.value ?? data ?? [])).catch(setError).finally(() => setLoading(false)); };
  useEffect(load, []);
  const save = (payload) => { const request = modal.product ? productsApi.update(modal.product.id, payload) : productsApi.create(payload); request.then(() => { setModal(null); load(); }).catch(setError); };
  const remove = (product) => { if (window.confirm(`Delete ${product.name}?`)) productsApi.remove(product.id).then(load).catch(setError); };
  return <><div className="page-toolbar"><div><span className="overline">CATALOG</span><h2>Products</h2></div><Button onClick={() => setModal({ product: null })}>+ Add product</Button></div><Card>{error && <ErrorMessage message="Products API is unavailable or does not expose list CRUD yet." />}{loading ? <Loader /> : <ProductList products={products} onEdit={(product) => setModal({ product })} onDelete={remove} />}</Card>{modal && <Modal title={modal.product ? 'Edit product' : 'Add product'} onClose={() => setModal(null)}><ProductForm product={modal.product} onSubmit={save} onCancel={() => setModal(null)} /></Modal>}</>;
}
