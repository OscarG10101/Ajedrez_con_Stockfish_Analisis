using System.Drawing;
using System.Drawing.Drawing2D;

namespace Ajedrez_interactuable_con_form
{
    public class GloboComic
    {
        private Font fuente = new Font("Comic Sans MS", 14, FontStyle.Bold);

        public void Dibujar(Graphics g, Rectangle rect, string texto)
        {
            if (string.IsNullOrEmpty(texto)) return;

            // Suavizado
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Globo redondeado
            using (GraphicsPath path = new GraphicsPath())
            {
                int radio = 20;
                path.AddArc(rect.X, rect.Y, radio, radio, 180, 90);
                path.AddArc(rect.Right - radio, rect.Y, radio, radio, 270, 90);
                path.AddArc(rect.Right - radio, rect.Bottom - radio, radio, radio, 0, 90);
                path.AddArc(rect.X, rect.Bottom - radio, radio, radio, 90, 90);
                path.CloseFigure();

                g.FillPath(Brushes.White, path);
                g.DrawPath(Pens.Black, path);
            }

            // Colita del globo (hacia abajo)
            Point[] colita =
            {
                new Point(rect.X + 40, rect.Bottom),
                new Point(rect.X + 70, rect.Bottom + 30),
                new Point(rect.X + 80, rect.Bottom)
            };
            g.FillPolygon(Brushes.White, colita);
            g.DrawPolygon(Pens.Black, colita);

            // Texto centrado
            g.DrawString(texto, fuente, Brushes.Black, rect,
                new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
        }
    }
}
