import Link from 'next/link';
import { getUserId } from '@/lib/auth';

export const dynamic = 'force-dynamic';

export default async function Home() {
  const userId = await getUserId();

  return (
    <>
      <h1>Clipboard multiplataforma</h1>
      <p className="muted">
        Portapapeles compartido entre tus dispositivos Windows e iPhone.
      </p>

      <div className="card">
        {userId ? (
          <div className="row">
            <Link href="/devices">
              <button>Ir a mis dispositivos</button>
            </Link>
          </div>
        ) : (
          <div className="row">
            <Link href="/login">
              <button>Iniciar sesión</button>
            </Link>
            <Link href="/register">
              <button className="secondary">Crear cuenta</button>
            </Link>
          </div>
        )}
      </div>
    </>
  );
}
