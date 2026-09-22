import { NextResponse } from 'next/server';
import { sql } from '@/lib/db';
import { verifyPassword, createSession } from '@/lib/auth';

export const runtime = 'nodejs';
export const dynamic = 'force-dynamic';

export async function POST(req: Request) {
  const { email, password } = await req.json().catch(() => ({}) as any);

  const normEmail = typeof email === 'string' ? email.trim().toLowerCase() : '';
  if (!normEmail || typeof password !== 'string') {
    return NextResponse.json({ error: 'credenciales inválidas' }, { status: 400 });
  }

  const rows = await sql`
    SELECT id, password_hash FROM users WHERE email = ${normEmail} LIMIT 1
  `;
  if (rows.length === 0) {
    return NextResponse.json({ error: 'email o contraseña incorrectos' }, { status: 401 });
  }

  const ok = await verifyPassword(password, rows[0].password_hash as string);
  if (!ok) {
    return NextResponse.json({ error: 'email o contraseña incorrectos' }, { status: 401 });
  }

  await createSession(rows[0].id as number);
  return NextResponse.json({ ok: true });
}
