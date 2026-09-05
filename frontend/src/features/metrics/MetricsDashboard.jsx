import { useEffect, useState } from 'react';
import { Area, AreaChart, Bar, BarChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts';
import Card from '../../components/common/Card';
import Loader from '../../components/common/Loader';
import ErrorMessage from '../../components/common/ErrorMessage';
import { metricsApi } from '../../api/metrics.api';

export default function MetricsDashboard() {
  const [metrics, setMetrics] = useState(null);
  const [error, setError] = useState(null);
  useEffect(() => { let active = true; const load = () => metricsApi.get().then(({ data }) => active && setMetrics(data)).catch((e) => active && setError(e)); load(); const id = setInterval(load, 5000); return () => { active = false; clearInterval(id); }; }, []);
  if (error) return <ErrorMessage message="Metrics API is unavailable. Start the backend to see live telemetry." />;
  if (!metrics) return <Loader />;
  const hits = metrics.hits ?? metrics.cacheHitCount ?? 0; const misses = metrics.misses ?? metrics.cacheMissCount ?? 0; const ratio = metrics.hitRate ?? metrics.cacheHitRatio ?? 0;
  const totalRequests = metrics.totalRequests ?? metrics.totalOperations ?? hits + misses;
  return <div className="dashboard-grid"><div className="metric-row"><Metric label="Total requests" value={totalRequests.toLocaleString()} tone="ink" /><Metric label="Cache hits" value={hits.toLocaleString()} tone="teal" /><Metric label="Cache misses" value={misses.toLocaleString()} tone="amber" /><Metric label="Hit ratio" value={`${Number(ratio).toFixed(1)}%`} tone="coral" /></div><div className="chart-grid"><Card><div className="card-title"><div><span className="overline">TRAFFIC MIX</span><h2>Cache hit ratio</h2></div><span className="live-pill">● LIVE</span></div><ResponsiveContainer width="100%" height={230}><AreaChart data={[{ name: 'Hits', value: hits }, { name: 'Misses', value: misses }]}><defs><linearGradient id="tealFill" x1="0" y1="0" x2="0" y2="1"><stop offset="0%" stopColor="#11a58b" stopOpacity=".45" /><stop offset="100%" stopColor="#11a58b" stopOpacity=".02" /></linearGradient></defs><CartesianGrid vertical={false} stroke="#e8e5df" /><XAxis dataKey="name" axisLine={false} tickLine={false} /><YAxis axisLine={false} tickLine={false} /><Tooltip /><Area type="monotone" dataKey="value" stroke="#078f79" fill="url(#tealFill)" strokeWidth={3} /></AreaChart></ResponsiveContainer></Card><Card><div className="card-title"><div><span className="overline">LATENCY</span><h2>Response time</h2></div><span className="unit">milliseconds</span></div><ResponsiveContainer width="100%" height={230}><BarChart data={[{ name: 'Cache hit', value: metrics.averageHitResponseTimeMs ?? 0 }, { name: 'Cache miss', value: metrics.averageMissResponseTimeMs ?? 0 }]}><CartesianGrid vertical={false} stroke="#e8e5df" /><XAxis dataKey="name" axisLine={false} tickLine={false} /><YAxis axisLine={false} tickLine={false} /><Tooltip /><Bar dataKey="value" fill="#ef765c" radius={[4, 4, 0, 0]} /></BarChart></ResponsiveContainer></Card></div></div>;
}
function Metric({ label, value, tone }) { return <div className={`metric metric-${tone}`}><span>{label}</span><strong>{value}</strong><small>from live API</small></div>; }
