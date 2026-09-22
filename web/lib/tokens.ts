import { randomBytes, createHash } from 'node:crypto';

// El token en claro solo se muestra una vez al usuario. En la base guardamos
// únicamente su hash SHA-256, así una fuga de la DB no expone tokens usables.
export function generateToken(): { token: string; hash: string } {
  const raw = randomBytes(24).toString('base64url');
  const token = `clip_${raw}`;
  return { token, hash: hashToken(token) };
}

export function hashToken(token: string): string {
  return createHash('sha256').update(token).digest('hex');
}
