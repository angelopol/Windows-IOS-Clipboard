import { NextResponse } from 'next/server';
import { sql } from '@/lib/db';
import { hashPassword, createSession } from '@/lib/auth';

export const runtime = 'nodejs';
export const dynamic = 'force-dynamic';

export async function POST(req: Request) {
  const { email, password } = await req.json().catch(() => ({}) as any);

  const normEmail = typeof email === 'string' ? email.trim().toLowerCase() : '';
  if (!normEmail || !normEmail.includes('@')) {
    return NextResponse.json({ error: 'email inválido' }, { status: 400 });
  }
  if (typeof password !== 'string' || password.length < 8) {
    return NextResponse.json(
      { error: 'la contraseña debe tener al menos 8 caracteres' },
      { status: 400 },
    );
  }

  const existing = await sql`SELECT id FROM users WHERE email = ${normEmail}`;
  if (existing.length > 0) {
    return NextResponse.json({ error: 'ese email ya existe' }, { status: 409 });
  }

  const hash = await hashPassword(password);
  const rows = await sql`
    INSERT INTO users (email, password_hash)
    VALUES (${normEmail}, ${hash})
    RETURNING id
  `;

  await createSession(rows[0].id as number);
  return NextResponse.json({ ok: true }, { status: 201 });
}
