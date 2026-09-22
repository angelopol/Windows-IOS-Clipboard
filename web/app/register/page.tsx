'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';

export default function RegisterPage() {
  const router = useRouter();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  async function submit(e: React.FormEvent) {
    e.preventDefault();
    setError('');
    setLoading(true);
    const res = await fetch('/api/auth/register', {
      method: 'POST',
      headers: { 'content-type': 'application/json' },
      body: JSON.stringify({ email, password }),
    });
    setLoading(false);
    if (res.ok) {
      router.push('/devices');
    } else {
      const data = await res.json().catch(() => ({}));
      setError(data.error || 'Error al crear la cuenta');
    }
  }

  return (
    <>
      <h1>Crear cuenta</h1>
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
        <label htmlFor="password">Contraseña (mín. 8 caracteres)</label>
        <input
          id="password"
          type="password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          autoComplete="new-password"
          minLength={8}
          required
        />
        {error && <p className="error">{error}</p>}
        <div className="row" style={{ marginTop: '1rem' }}>
          <button type="submit" disabled={loading}>
            {loading ? '...' : 'Crear cuenta'}
          </button>
          <Link href="/login" className="muted">
            Ya tengo cuenta
          </Link>
        </div>
      </form>
    </>
  );
}
