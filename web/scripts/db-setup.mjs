// Ejecuta schema.sql contra la base de datos (Neon).
// Uso: npm run db:setup   (carga .env.local automáticamente con --env-file)
import { neon } from '@neondatabase/serverless';
import { readFileSync } from 'node:fs';

const connectionString = process.env.DATABASE_URL || process.env.POSTGRES_URL;
if (!connectionString) {
  console.error('Falta DATABASE_URL. Copia .env.example a .env.local y rellénalo.');
  process.exit(1);
}

const sql = neon(connectionString);
const schema = readFileSync(new URL('../schema.sql', import.meta.url), 'utf8');
const statements = schema
  .split(';')
  .map((s) => s.trim())
  .filter((s) => s && !s.startsWith('--'));

for (const stmt of statements) {
  await sql.query(stmt);
  console.log('OK:', stmt.split('\n')[0].slice(0, 60));
}

console.log(`\nListo. ${statements.length} sentencias ejecutadas.`);
process.exit(0);
