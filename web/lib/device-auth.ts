import { sql } from './db';
import { hashToken } from './tokens';

export interface DeviceAuth {
  deviceId: number;
  userId: number;
}

// Autentica una petición de dispositivo mediante `Authorization: Bearer <token>`.
// Devuelve el dispositivo/usuario o null si el token es inválido o revocado.
export async function authenticateDevice(
  req: Request,
): Promise<DeviceAuth | null> {
  const header = req.headers.get('authorization') || '';
  const match = header.match(/^Bearer\s+(.+)$/i);
  if (!match) return null;

  const tokenHash = hashToken(match[1].trim());
  const rows = await sql`
    SELECT id, user_id FROM devices WHERE token_hash = ${tokenHash} LIMIT 1
  `;
  if (rows.length === 0) return null;

  const device = rows[0];
  await sql`UPDATE devices SET last_seen = now() WHERE id = ${device.id}`;
  return { deviceId: device.id as number, userId: device.user_id as number };
}
