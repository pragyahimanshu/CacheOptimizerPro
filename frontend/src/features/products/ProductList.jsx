import PropTypes from 'prop-types';
import Button from '../../components/common/Button';

export default function ProductList({ products, onEdit, onDelete }) {
  if (!products.length) return <div className="empty-state"><strong>No products yet</strong><p>Create your first product to exercise the cache.</p></div>;
  return <div className="table-wrap"><table><thead><tr><th>Product</th><th>Price</th><th>Stock</th><th>Created</th><th /></tr></thead><tbody>{products.map((product) => <tr key={product.id}><td><strong>{product.name}</strong><small>{product.description}</small></td><td>${Number(product.price).toFixed(2)}</td><td><span className="stock-badge">{product.stockQuantity} units</span></td><td>{product.createdOnUtc ? new Date(product.createdOnUtc).toLocaleDateString() : '—'}</td><td className="row-actions"><Button variant="ghost" onClick={() => onEdit(product)}>Edit</Button><Button variant="danger" onClick={() => onDelete(product)}>Delete</Button></td></tr>)}</tbody></table></div>;
}
ProductList.propTypes = { products: PropTypes.array.isRequired, onEdit: PropTypes.func.isRequired, onDelete: PropTypes.func.isRequired };
