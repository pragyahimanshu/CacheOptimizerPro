import { useState } from 'react';
import PropTypes from 'prop-types';
import Button from '../../components/common/Button';
import Input from '../../components/common/Input';

export default function ProductForm({ product, onSubmit, onCancel }) {
  const [form, setForm] = useState(product || { name: '', description: '', price: '', stockQuantity: 0 });
  const update = (key, value) => setForm({ ...form, [key]: value });
  return <form onSubmit={(e) => { e.preventDefault(); onSubmit({ ...form, price: Number(form.price), stockQuantity: Number(form.stockQuantity) }); }}><Input label="Product name" required value={form.name} onChange={(e) => update('name', e.target.value)} /><Input label="Description" required value={form.description} onChange={(e) => update('description', e.target.value)} /><div className="form-row"><Input label="Price" type="number" min="0.01" step="0.01" required value={form.price} onChange={(e) => update('price', e.target.value)} /><Input label="Stock quantity" type="number" min="0" required value={form.stockQuantity} onChange={(e) => update('stockQuantity', e.target.value)} /></div><div className="actions"><Button type="button" variant="ghost" onClick={onCancel}>Cancel</Button><Button type="submit">Save product</Button></div></form>;
}
ProductForm.propTypes = { product: PropTypes.object, onSubmit: PropTypes.func.isRequired, onCancel: PropTypes.func.isRequired };
