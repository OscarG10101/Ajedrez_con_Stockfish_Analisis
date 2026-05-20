using System.Drawing;
using System.Drawing.Drawing2D;

namespace Ajedrez_interactuable_con_form.Vistas
{
    public class GloboComic
    {
        private readonly Font fuente = new Font("Segoe UI", 11, FontStyle.Bold);
        private readonly Color colorFondo = Color.FromArgb(240, 49, 47, 43);  // gris carbón semitransparente
        private readonly Color colorBorde = Color.FromArgb(129, 182, 76);     // verde chess
        private readonly Color colorTexto = Color.White;

        public void Dibujar(Graphics g, Rectangle rect, string texto)
        {
            if (string.IsNullOrEmpty(texto)) return;

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // ── Sombra ───────────────────────────────────────────────
            Rectangle rectSombra = new Rectangle(rect.X + 3, rect.Y + 3, rect.Width, rect.Height);
            using (GraphicsPath pathSombra = ConstruirGlobo(rectSombra, 14))
            using (SolidBrush brochaS = new SolidBrush(Color.FromArgb(80, 0, 0, 0)))
                g.FillPath(brochaS, pathSombra);

            // ── Fondo del globo ──────────────────────────────────────
            using (GraphicsPath path = ConstruirGlobo(rect, 14))
            {
                using (SolidBrush brocha = new SolidBrush(colorFondo))
                    g.FillPath(brocha, path);

                using (Pen borde = new Pen(colorBorde, 1.5f))
                    g.DrawPath(borde, path);
            }

            // ── Colita triangular (apunta hacia abajo-izquierda) ─────
            Point punta = new Point(rect.X + 80, rect.Bottom + 18);
            Point[] cola =
            {
                new Point(rect.X + 40, rect.Bottom - 1),
                new Point(rect.X + 65, rect.Bottom - 1),
                punta
            };

            using (SolidBrush brocha = new SolidBrush(colorFondo))
                g.FillPolygon(brocha, cola);

            // Bordes de la colita — solo los dos lados exteriores
            using (Pen borde = new Pen(colorBorde, 1.5f))
            {
                g.DrawLine(borde, cola[0], punta);
                g.DrawLine(borde, cola[1], punta);
            }

            // ── Texto ────────────────────────────────────────────────
            using (SolidBrush brochaTexto = new SolidBrush(colorTexto))
            {
                StringFormat fmt = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.Word
                };
                // Pequeño padding interior para que el texto no toque el borde
                Rectangle rectTexto = new Rectangle(
                    rect.X + 10, rect.Y + 6,
                    rect.Width - 20, rect.Height - 12);

                g.DrawString(texto, fuente, brochaTexto, rectTexto, fmt);
            }
        }

        private GraphicsPath ConstruirGlobo(Rectangle rect, int radio)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radio, radio, 180, 90);
            path.AddArc(rect.Right - radio, rect.Y, radio, radio, 270, 90);
            path.AddArc(rect.Right - radio, rect.Bottom - radio, radio, radio, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radio, radio, radio, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}