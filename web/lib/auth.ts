import { SignJWT, jwtVerify } from 'jose';
import { cookies } from 'next/headers';
import bcrypt from 'bcryptjs';

const secret = new TextEncoder().encode(
  process.env.AUTH_SECRET || 'dev-insecure-secret-change-me',
);
const COOKIE = 'session';
const MAX_AGE = 60 * 60 * 24 * 30; // 30 días

export function hashPassword(pw: string): Promise<string> {
  return bcrypt.hash(pw, 10);
}

export function verifyPassword(pw: string, hash: string): Promise<boolean> {
  return bcrypt.compare(pw, hash);
}

export async function createSession(userId: number): Promise<void> {
  const token = await new SignJWT({ uid: userId })
    .setProtectedHeader({ alg: 'HS256' })
    .setIssuedAt()
    .setExpirationTime('30d')
    .sign(secret);

  cookies().set(COOKIE, token, {
    httpOnly: true,
    secure: process.env.NODE_ENV === 'production',
    sameSite: 'lax',
    path: '/',
    maxAge: MAX_AGE,
  });
}

export function destroySession(): void {
  cookies().set(COOKIE, '', { path: '/', maxAge: 0 });
}

export async function getUserId(): Promise<number | null> {
  const value = cookies().get(COOKIE)?.value;
  if (!value) return null;
  try {
    const { payload } = await jwtVerify(value, secret);
    return typeof payload.uid === 'number' ? payload.uid : null;
  } catch {
    return null;
  }
}
