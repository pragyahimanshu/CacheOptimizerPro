import { useEffect, useState } from 'react';
import PropTypes from 'prop-types';
import Card from '../../components/common/Card';
import Button from '../../components/common/Button';
import Input from '../../components/common/Input';
import { cacheConfigApi } from '../../api/cacheConfig.api';

export default function CacheConfigForm({ onSaved }) {
  const [form, setForm] = useState({ expirationSeconds: 3600, strategy: 'CacheAside' }); const [status, setStatus] = useState('');
  useEffect(() => { cacheConfigApi.get().then(({ data }) => setForm({ expirationSeconds: Math.round(data.expirationSeconds ?? 3600), strategy: data.strategy ?? 'CacheAside' })).catch(() => {}); }, []);
  const submit = (event) => { event.preventDefault(); setStatus('Saving...'); cacheConfigApi.update({ expirationSeconds: Number(form.expirationSeconds), strategy: form.strategy }).then(() => { setStatus('Configuration saved'); onSaved?.(); }).catch(() => setStatus('Unable to save configuration')); };
  return <Card className="settings-card"><span className="overline">RUNTIME POLICY</span><h2>Control the cache from here</h2><p className="muted">Changes are applied instantly to new product reads and writes.</p><form onSubmit={submit}><Input label="Expiration (seconds)" type="number" min="1" value={form.expirationSeconds} onChange={(e) => setForm({ ...form, expirationSeconds: e.target.value })} /><label className="field">Strategy<select value={form.strategy} onChange={(e) => setForm({ ...form, strategy: e.target.value })}><option value="CacheAside">Cache-Aside</option><option value="WriteThrough">Write-Through</option></select></label><Button type="submit">Save policy</Button>{status && <span className="form-status">{status}</span>}</form></Card>;
}
CacheConfigForm.propTypes = { onSaved: PropTypes.func };
