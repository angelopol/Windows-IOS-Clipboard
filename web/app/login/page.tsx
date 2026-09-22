'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';

export default function LoginPage() {
  const router = useRouter();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  async function submit(e: React.FormEvent) {
    e.preventDefault();
    setError('');
    setLoading(true);
    const res = await fetch('/api/auth/login', {
      method: 'POST',
      headers: { 'content-type': 'application/json' },
      body: JSON.stringify({ email, password }),
    });
    setLoading(false);
    if (res.ok) {
      router.push('/devices');
    } else {
      const data = await res.json().catch(() => ({}));
      setError(data.error || 'Error al iniciar sesión');
    }
  }

  return (
    <>
      <h1>Iniciar sesión</h1>
      <form className="card" onSubmit={submit}>
        <label htmlFor="email">Email</label>
        <input
          id="email"
          type="email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          autoComplete="email"
          required
        />
        <label htmlFor="password">Contraseña</label>
        <input
          id="password"
          type="password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          autoComplete="current-password"
          required
        />
        {error && <p className="error">{error}</p>}
        <div className="row" style={{ marginTop: '1rem' }}>
          <button type="submit" disabled={loading}>
            {loading ? '...' : 'Entrar'}
          </button>
          <Link href="/register" className="muted">
            Crear cuenta
          </Link>
        </div>
      </form>
    </>
  );
}
