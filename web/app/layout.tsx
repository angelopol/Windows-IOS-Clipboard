import type { Metadata } from 'next';
import './globals.css';

export const metadata: Metadata = {
  title: 'Clipboard multiplataforma',
  description: 'Portapapeles compartido entre Windows e iPhone',
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="es">
      <body>
        <main>{children}</main>
      </body>
    </html>
  );
}
