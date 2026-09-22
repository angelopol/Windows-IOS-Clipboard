import { redirect } from 'next/navigation';
import { sql } from '@/lib/db';
import { getUserId } from '@/lib/auth';
import DeviceManager, { type Device } from './DeviceManager';

export const runtime = 'nodejs';
export const dynamic = 'force-dynamic';

export default async function DevicesPage() {
  const userId = await getUserId();
  if (!userId) redirect('/login');

  const rows = await sql`
    SELECT id, name, platform, created_at, last_seen
    FROM devices
    WHERE user_id = ${userId}
    ORDER BY created_at DESC
  `;

  return <DeviceManager initial={rows as unknown as Device[]} />;
}
