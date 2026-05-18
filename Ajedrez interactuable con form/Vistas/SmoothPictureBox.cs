using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Ajedrez_interactuable_con_form
{
    public class SmoothPanel : Panel
    {
        public SmoothPanel()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
        }
    }
    public class SmoothPictureBox : PictureBox
    {
        private float escala = 1.0f;
        private float escalaObjetivo = 1.0f;
        private readonly System.Windows.Forms.Timer timerZoom;

        private const float EscalaMax = 1.08f;
        private const float EscalaMin = 1.0f;
        private const float VelocidadPaso = 0.012f; // cuánto cambia por tick
        private const int IntervaloMs = 16;      // ~60fps

        public float Escala
        {
            get => escala;
            set
            {
                escala = value;
                Invalidate();
            }
        }

        public SmoothPictureBox()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            SizeMode = PictureBoxSizeMode.Normal;

            timerZoom = new System.Windows.Forms.Timer { Interval = IntervaloMs };
            timerZoom.Tick += OnZoomTick;
        }

        public void AnimarHacia(bool activar)
        {
            escalaObjetivo = activar ? EscalaMax : EscalaMin;
            timerZoom.Start();
        }

        private void OnZoomTick(object? sender, EventArgs e)
        {
            if (Math.Abs(escala - escalaObjetivo) <= VelocidadPaso)
            {
                escala = escalaObjetivo;
                timerZoom.Stop();
            }
            else
            {
                escala += escala < escalaObjetivo ? VelocidadPaso : -VelocidadPaso;
            }

            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                timerZoom.Dispose();

            base.Dispose(disposing);
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            pe.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            pe.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            pe.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            if (Image != null)
            {
                int nuevoAncho = (int)(Width * escala);
                int nuevoAlto = (int)(Height * escala);

                int x = (Width - nuevoAncho) / 2;
                int y = (Height - nuevoAlto) / 2;

                pe.Graphics.DrawImage(Image, x, y, nuevoAncho, nuevoAlto);
            }
        }
    }
}