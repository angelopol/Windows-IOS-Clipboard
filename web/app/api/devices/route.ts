import { NextResponse } from 'next/server';
import { sql } from '@/lib/db';
import { getUserId } from '@/lib/auth';
import { generateToken } from '@/lib/tokens';

export const runtime = 'nodejs';
export const dynamic = 'force-dynamic';

// GET /api/devices  —  lista los dispositivos del usuario (sin exponer tokens).
export async function GET() {
  const userId = await getUserId();
  if (!userId) {
    return NextResponse.json({ error: 'unauthorized' }, { status: 401 });
  }

  const rows = await sql`
    SELECT id, name, platform, created_at, last_seen
    FROM devices
    WHERE user_id = ${userId}
    ORDER BY created_at DESC
  `;
  return NextResponse.json({ devices: rows });
}

// POST /api/devices  —  crea un dispositivo y devuelve su token UNA sola vez.
export async function POST(req: Request) {
  const userId = await getUserId();
  if (!userId) {
    return NextResponse.json({ error: 'unauthorized' }, { status: 401 });
  }

  const { name, platform } = await req.json().catch(() => ({}) as any);
  const devName =
    typeof name === 'string' && name.trim() ? name.trim().slice(0, 80) : 'Dispositivo';
  const devPlatform =
    typeof platform === 'string' ? platform.trim().slice(0, 40) : null;

  const { token, hash } = generateToken();
  const rows = await sql`
    INSERT INTO devices (user_id, name, token_hash, platform)
    VALUES (${userId}, ${devName}, ${hash}, ${devPlatform})
    RETURNING id, name, platform, created_at, last_seen
  `;

  // El token en claro solo viaja aquí, una vez. Después solo queda su hash.
  return NextResponse.json({ device: rows[0], token }, { status: 201 });
}
