import { NextResponse } from 'next/server';
import { sql } from '@/lib/db';
import { authenticateDevice } from '@/lib/device-auth';

export const runtime = 'nodejs';
export const dynamic = 'force-dynamic';

// GET /api/clips  —  devuelve los últimos 5 textos (más reciente primero).
//   ?scope=shared    (por defecto) todos los dispositivos de la cuenta.
//   ?scope=personal  solo lo que subió ESTE dispositivo (source_device_id).
// Lo consumen los atajos de iOS ("Ver Personal" / "Ver Compartido") y el
// agente de Windows (que usa el scope compartido por defecto).
export async function GET(req: Request) {
  const auth = await authenticateDevice(req);
  if (!auth) {
    return NextResponse.json({ error: 'unauthorized' }, { status: 401 });
  }

  const scope =
    new URL(req.url).searchParams.get('scope') === 'personal'
      ? 'personal'
      : 'shared';

  const rows =
    scope === 'personal'
      ? await sql`
          SELECT id, text, created_at
          FROM clips
          WHERE user_id = ${auth.userId}
            AND source_device_id = ${auth.deviceId}
          ORDER BY created_at DESC
          LIMIT 5
        `
      : await sql`
          SELECT id, text, created_at
          FROM clips
          WHERE user_id = ${auth.userId}
          ORDER BY created_at DESC
          LIMIT 5
        `;

  return NextResponse.json({ clips: rows, scope });
}
