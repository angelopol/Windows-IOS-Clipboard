'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';

export interface Device {
  id: number;
  name: string;
  platform: string | null;
  created_at: string;
  last_seen: string | null;
}

export default function DeviceManager({ initial }: { initial: Device[] }) {
  const router = useRouter();
  const [devices, setDevices] = useState<Device[]>(initial);
  const [name, setName] = useState('');
  const [platform, setPlatform] = useState('windows');
  const [newToken, setNewToken] = useState<string | null>(null);
  const [copied, setCopied] = useState(false);
  const [loading, setLoading] = useState(false);

  async function createDevice(e: React.FormEvent) {
    e.preventDefault();
    setLoading(true);
    setNewToken(null);
    setCopied(false);
    const res = await fetch('/api/devices', {
      method: 'POST',
      headers: { 'content-type': 'application/json' },
      body: JSON.stringify({ name: name || 'Dispositivo', platform }),
    });
    setLoading(false);
    if (res.ok) {
      const data = await res.json();
      setDevices((d) => [data.device, ...d]);
      setNewToken(data.token);
      setName('');
    }
  }

  async function revoke(id: number) {
    if (!confirm('¿Revocar este dispositivo? Su token dejará de funcionar.')) return;
    const res = await fetch(`/api/devices/${id}`, { method: 'DELETE' });
    if (res.ok) setDevices((d) => d.filter((x) => x.id !== id));
  }

  async function logout() {
    await fetch('/api/auth/logout', { method: 'POST' });
    router.push('/login');
  }

  function copyToken() {
    if (newToken) {
      navigator.clipboard.writeText(newToken);
      setCopied(true);
    }
  }

  return (
    <>
      <div className="row" style={{ justifyContent: 'space-between' }}>
        <h1>Mis dispositivos</h1>
        <button className="secondary" onClick={logout}>
          Cerrar sesión
        </button>
      </div>
      <p className="muted">
        Crea un token por dispositivo (Windows, iPhone…). Cópialo en el agente o
        en el atajo. Puedes revocar cualquiera sin afectar a los demás.
      </p>

      <form className="card" onSubmit={createDevice}>
        <h2 style={{ marginTop: 0 }}>Enlazar nuevo dispositivo</h2>
        <label htmlFor="name">Nombre</label>
        <input
          id="name"
          type="text"
          placeholder="Ej. PC de casa"
          value={name}
          onChange={(e) => setName(e.target.value)}
        />
        <label htmlFor="platform">Plataforma</label>
        <select
          id="platform"
          value={platform}
          onChange={(e) => setPlatform(e.target.value)}
          style={{
            width: '100%',
            padding: '0.6rem 0.7rem',
            background: '#0e1014',
            border: '1px solid var(--border)',
            borderRadius: 8,
            color: 'var(--text)',
          }}
        >
          <option value="windows">Windows</option>
          <option value="ios">iPhone / iOS</option>
          <option value="other">Otro</option>
        </select>
        <div className="row" style={{ marginTop: '1rem' }}>
          <button type="submit" disabled={loading}>
            {loading ? '...' : 'Crear token'}
          </button>
        </div>

        {newToken && (
          <div style={{ marginTop: '1rem' }}>
            <p className="muted">
              Copia este token ahora. <strong>No se volverá a mostrar.</strong>
            </p>
            <div className="token">{newToken}</div>
            <div className="row" style={{ marginTop: '0.5rem' }}>
              <button type="button" className="secondary" onClick={copyToken}>
                {copied ? 'Copiado ✓' : 'Copiar'}
              </button>
            </div>
          </div>
        )}
      </form>

      <div className="card">
        <h2 style={{ marginTop: 0 }}>Dispositivos enlazados</h2>
        {devices.length === 0 ? (
          <p className="muted">Aún no has enlazado ningún dispositivo.</p>
        ) : (
          devices.map((d) => (
            <div className="device" key={d.id}>
              <div>
                <strong>{d.name}</strong>{' '}
                <span className="muted">({d.platform || 'otro'})</span>
                <br />
                <span className="muted">
                  Última actividad:{' '}
                  {d.last_seen
                    ? new Date(d.last_seen).toLocaleString()
                    : 'nunca'}
                </span>
              </div>
              <button className="danger" onClick={() => revoke(d.id)}>
                Revocar
              </button>
            </div>
          ))
        )}
      </div>
    </>
  );
}
