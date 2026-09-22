import { NextResponse } from 'next/server';
import { sql } from '@/lib/db';
import { authenticateDevice } from '@/lib/device-auth';

export const runtime = 'nodejs';
export const dynamic = 'force-dynamic';

const KEEP = 20; // cuántos clips retenemos por usuario
const MAX_LEN = 100_000; // límite de tamaño de un texto

// POST /api/clip  —  un dispositivo sube un texto al portapapeles compartido.
export async function POST(req: Request) {
  const auth = await authenticateDevice(req);
  if (!auth) {
    return NextResponse.json({ error: 'unauthorized' }, { status: 401 });
  }

  let body: unknown;
  try {
    body = await req.json();
  } catch {
    return NextResponse.json({ error: 'invalid json' }, { status: 400 });
  }

  const text =
    body && typeof (body as any).text === 'string' ? (body as any).text : '';
  if (!text.trim()) {
    return NextResponse.json({ error: 'text required' }, { status: 400 });
  }
  if (text.length > MAX_LEN) {
    return NextResponse.json({ error: 'text too large' }, { status: 413 });
  }

  // Dedup: si el texto es idéntico al último clip del usuario, no creamos otro;
  // solo refrescamos su fecha para que suba al principio de la lista. Evita
  // duplicados cuando se copia lo mismo varias veces (sondeo, re-copiado, eco).
  const latest = await sql`
    SELECT id, text FROM clips
    WHERE user_id = ${auth.userId}
    ORDER BY created_at DESC
    LIMIT 1
  `;
  if (latest.length > 0 && latest[0].text === text) {
    await sql`UPDATE clips SET created_at = now() WHERE id = ${latest[0].id}`;
    return NextResponse.json({ id: latest[0].id, deduped: true }, { status: 200 });
  }

  const rows = await sql`
    INSERT INTO clips (user_id, text, source_device_id)
    VALUES (${auth.userId}, ${text}, ${auth.deviceId})
    RETURNING id, created_at
  `;

  // Poda: conserva solo los KEEP más recientes de este usuario.
  await sql`
    DELETE FROM clips
    WHERE user_id = ${auth.userId}
      AND id NOT IN (
        SELECT id FROM clips
        WHERE user_id = ${auth.userId}
        ORDER BY created_at DESC
        LIMIT ${KEEP}
      )
  `;

  return NextResponse.json(
    { id: rows[0].id, created_at: rows[0].created_at },
    { status: 201 },
  );
}
