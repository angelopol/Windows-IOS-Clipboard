import { neon, type NeonQueryFunction } from '@neondatabase/serverless';

// Capa fina sobre el driver serverless de Neon.
// El cliente se crea de forma perezosa (en la primera consulta), no al importar,
// para que `next build` no falle cuando aún no hay connection string en el entorno.
//
// A diferencia de @vercel/postgres, el `sql` de Neon devuelve el array de filas
// directamente (no `{ rows }`). Añadimos `.query()` para sentencias sueltas.

let cached: NeonQueryFunction<false, false> | null = null;

function getClient(): NeonQueryFunction<false, false> {
  if (cached) return cached;
  const connectionString = process.env.DATABASE_URL || process.env.POSTGRES_URL;
  if (!connectionString) {
    throw new Error(
      'Falta DATABASE_URL (o POSTGRES_URL). Configúrala en .env.local o en Vercel.',
    );
  }
  // El driver de Neon consulta por HTTP (fetch). Next.js cachea los fetch en su
  // Data Cache, lo que congelaba las lecturas de /api/clips (devolvía siempre el
  // primer resultado). `cache: 'no-store'` fuerza a que cada consulta sea fresca.
  cached = neon(connectionString, { fetchOptions: { cache: 'no-store' } });
  return cached;
}

type Sql = {
  (strings: TemplateStringsArray, ...values: unknown[]): Promise<any[]>;
  query: (text: string, params?: unknown[]) => Promise<any[]>;
};

export const sql: Sql = Object.assign(
  (strings: TemplateStringsArray, ...values: unknown[]): Promise<any[]> =>
    getClient()(strings, ...values) as unknown as Promise<any[]>,
  {
    query: (text: string, params: unknown[] = []): Promise<any[]> =>
      (getClient() as any).query(text, params) as Promise<any[]>,
  },
);
